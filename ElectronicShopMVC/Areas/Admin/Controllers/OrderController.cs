using ElectronicShopMVC.DataAccess.Repository.IRepository;
using ElectronicShopMVC.Model;
using ElectronicShopMVC.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ElectronicShopMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin)]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IUnitOfWork unitOfWork, ILogger<OrderController> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index(string? status)
        {
            try
            {
                var orders = await Task.Run(() => _unitOfWork.Order.GetAll(includeProperties: "User,Items").ToList());

                if (!string.IsNullOrEmpty(status) && status != "All")
                {
                    if (Enum.TryParse<Constants.OrderStatus>(status, true, out var parsedStatus))
                    {
                        orders = orders.Where(o => o.Status == parsedStatus).ToList();
                    }
                }

                ViewBag.ActiveStatus = status ?? "All";
                return View(orders.OrderByDescending(o => o.Date).ToList());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting orders for Admin dashboard");
                TempData["error"] = "Không thể tải danh sách đơn hàng.";
                return View(new List<Order>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var order = await Task.Run(() => _unitOfWork.Order.Get(
                    o => o.Id == id,
                    includeProperties: "User,Items,Items.Product,Voucher"
                ));

                if (order == null)
                {
                    _logger.LogWarning("Admin order details: Order {OrderId} not found", id);
                    return NotFound();
                }

                var histories = await Task.Run(() => _unitOfWork.OrderStatusHistory.GetAll()
                    .Where(h => h.OrderId == id)
                    .OrderBy(h => h.ChangedAt)
                    .ToList());

                var transactions = await Task.Run(() => _unitOfWork.PaymentTransaction.GetAll()
                    .Where(t => t.OrderId == id)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToList());

                ViewBag.Histories = histories;
                ViewBag.Transactions = transactions;

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading order details for Admin, Order ID: {OrderId}", id);
                TempData["error"] = "Lỗi khi tải chi tiết đơn hàng.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status, string? notes, string? trackingNumber)
        {
            try
            {
                var order = await Task.Run(() => _unitOfWork.Order.Get(o => o.Id == id));
                if (order == null)
                {
                    return NotFound();
                }

                if (!Enum.TryParse<Constants.OrderStatus>(status, true, out var newStatus))
                {
                    TempData["error"] = "Trạng thái đơn hàng không hợp lệ.";
                    return RedirectToAction(nameof(Details), new { id = id });
                }

                var oldStatus = order.Status;
                if (oldStatus == newStatus)
                {
                    // No status change, but check if tracking number changed
                    if (!string.IsNullOrEmpty(trackingNumber) && trackingNumber != order.TrackingNumber)
                    {
                        order.TrackingNumber = trackingNumber;
                        _unitOfWork.Order.Update(order);
                        _unitOfWork.Save();
                        TempData["success"] = "Đã cập nhật mã vận đơn thành công.";
                    }
                    return RedirectToAction(nameof(Details), new { id = id });
                }

                // Update Order fields
                order.Status = newStatus;
                
                if (!string.IsNullOrEmpty(trackingNumber))
                {
                    order.TrackingNumber = trackingNumber;
                }

                // Map fulfillment and payment statuses based on order state
                if (newStatus == Constants.OrderStatus.Completed)
                {
                    order.FulfillmentStatus = "Delivered";
                    order.PaymentStatus = "Approved";

                    // Update payment transaction if COD
                    var codTransaction = _unitOfWork.PaymentTransaction.GetAll()
                        .FirstOrDefault(t => t.OrderId == id && t.PaymentMethod == "COD");
                    if (codTransaction != null)
                    {
                        codTransaction.Status = "Success";
                        codTransaction.ResponsePayload = "COD thanh toán trực tiếp khi nhận hàng - Thành công.";
                        _unitOfWork.PaymentTransaction.Update(codTransaction);
                    }
                }
                else if (newStatus == Constants.OrderStatus.Cancelled)
                {
                    order.FulfillmentStatus = "Cancelled";
                    order.PaymentStatus = "Failed";

                    // Update transactions to failed/refunded
                    var transactions = _unitOfWork.PaymentTransaction.GetAll().Where(t => t.OrderId == id);
                    foreach (var trans in transactions)
                    {
                        if (trans.PaymentMethod == "VNPay")
                        {
                            trans.Status = "Refunded";
                            trans.ResponsePayload += " | Đơn hàng bị hủy bởi Admin - Chờ hoàn tiền.";
                        }
                        else
                        {
                            trans.Status = "Failed";
                            trans.ResponsePayload += " | Đơn hàng bị hủy bởi Admin.";
                        }
                        _unitOfWork.PaymentTransaction.Update(trans);
                    }
                }
                else if (newStatus == Constants.OrderStatus.Processing || newStatus == Constants.OrderStatus.Approved)
                {
                    order.FulfillmentStatus = "Processing";
                }

                _unitOfWork.Order.Update(order);

                // Add state tracking history entry
                var history = new OrderStatusHistory
                {
                    OrderId = id,
                    OldStatus = oldStatus.ToString(),
                    NewStatus = newStatus.ToString(),
                    ChangedBy = User.Identity?.Name ?? "Admin",
                    ChangedAt = DateTime.UtcNow,
                    Notes = string.IsNullOrWhiteSpace(notes) ? $"Trạng thái cập nhật bởi hệ thống quản lý." : notes
                };
                _unitOfWork.OrderStatusHistory.Add(history);

                _unitOfWork.Save();

                _logger.LogInformation("Admin updated order status for Order {OrderId} from {OldStatus} to {NewStatus}", id, oldStatus, newStatus);
                TempData["success"] = $"Cập nhật trạng thái đơn hàng thành '{status}' thành công.";
                return RedirectToAction(nameof(Details), new { id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for Order {OrderId}", id);
                TempData["error"] = "Đã xảy ra lỗi khi cập nhật trạng thái đơn hàng.";
                return RedirectToAction(nameof(Details), new { id = id });
            }
        }
    }
}
