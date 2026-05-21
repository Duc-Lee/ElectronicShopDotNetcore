using ElectronicShopMVC.DataAccess.Repository.IRepository;
using ElectronicShopMVC.Model;
using ElectronicShopMVC.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ElectronicShopMVC.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ILogger<OrderController> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Challenge();
                }

                // Retrieve all orders of this customer ordered by Date descending
                var orders = (await _unitOfWork.Order.GetAllAsync(
                    filter: o => o.UserId == userId,
                    includeProperties: "Items"
                )).OrderByDescending(o => o.Date).ToList();

                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user orders");
                TempData["error"] = "Không thể tải danh sách đơn hàng lúc này.";
                return RedirectToAction("Index", "Home");
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    return Challenge();
                }

                var order = await Task.Run(() => _unitOfWork.Order.Get(
                    o => o.Id == id,
                    includeProperties: "Items,Items.Product,Voucher"
                ));

                if (order == null)
                {
                    _logger.LogWarning("Order {OrderId} not found for details view", id);
                    return NotFound();
                }

                // Check security: Customer can only view their own orders. Admin can view any.
                if (order.UserId != userId && !User.IsInRole(StaticDetails.Role_Admin))
                {
                    _logger.LogWarning("Unauthorized order access attempt by User {UserId} for Order {OrderId}", userId, id);
                    return Forbid();
                }

                // Fetch Order status histories ordered by ChangedAt ascending
                var histories = await Task.Run(() => _unitOfWork.OrderStatusHistory.GetAll()
                    .Where(h => h.OrderId == id)
                    .OrderBy(h => h.ChangedAt)
                    .ToList());

                // Fetch Payment transactions
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
                _logger.LogError(ex, "Error fetching order details for Order {OrderId}", id);
                TempData["error"] = "Không thể tải thông tin chi tiết đơn hàng.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
