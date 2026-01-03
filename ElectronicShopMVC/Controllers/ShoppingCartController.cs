using ElectronicShopMVC.DataAccess.Repository.IRepository;
using ElectronicShopMVC.Model.ViewModels;
using ElectronicShopMVC.Model;
using ElectronicShopMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ElectronicShopMVC.Utility;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ElectronicShopMVC.Controllers
{
    [Authorize(Roles = StaticDetails.Role_Cust + "," + StaticDetails.Role_Admin)]
    public class ShoppingCartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICartService _cartService;
        private readonly VNPayService _vnPayService;
        private readonly ILogger<ShoppingCartController> _logger;

        public ShoppingCartController(
            IUnitOfWork unitOfWork,
            ICartService cartService,
            UserManager<ApplicationUser> userManager,
            VNPayService vnPayService,
            ILogger<ShoppingCartController> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _cartService = cartService ?? throw new ArgumentNullException(nameof(cartService));
            _vnPayService = vnPayService ?? throw new ArgumentNullException(nameof(vnPayService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                string userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID is null in ShoppingCart Index");
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                var cart = await Task.Run(() => _unitOfWork.ShoppingCart.GetCart(userId));
                return View(cart ?? new Cart());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading shopping cart");
                TempData["error"] = "Đã xảy ra lỗi khi tải giỏ hàng.";
                return View(new Cart());
            }
        }

        public async Task<IActionResult> Summary()
        {
            try
            {
                string userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID is null in ShoppingCart Summary");
                    return RedirectToAction("Login", "Account", new { area = "Identity" });
                }

                var cart = await Task.Run(() => _unitOfWork.ShoppingCart.GetCart(userId));
                var user = await _userManager.GetUserAsync(User);

                if (user == null)
                {
                    _logger.LogWarning("User not found in ShoppingCart Summary");
                    TempData["error"] = "Không tìm thấy thông tin người dùng.";
                    return RedirectToAction(nameof(Index));
                }

                var summaryVM = new SummaryVM
                {
                    Cart = cart ?? new Cart(),
                    StreetAddress = user.StreetAddress ?? string.Empty,
                    City = user.City ?? string.Empty,
                    State = user.State ?? string.Empty,
                    PostalCode = user.PostalCode ?? string.Empty,
                    PhoneNumber = user.PhoneNumber ?? string.Empty
                };
                
                return View(summaryVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading summary");
                TempData["error"] = "Đã xảy ra lỗi khi tải thông tin thanh toán.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(SummaryVM summaryVM)
        {
            try
            {
                if (summaryVM == null)
                {
                    _logger.LogWarning("PlaceOrder called with null SummaryVM");
                    TempData["error"] = "Thông tin đơn hàng không hợp lệ.";
                    return RedirectToAction(nameof(Summary));
                }

                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogWarning("User not found in PlaceOrder");
                    TempData["error"] = "Không tìm thấy thông tin người dùng.";
                    return RedirectToAction(nameof(Summary));
                }

                var cart = await Task.Run(() => _unitOfWork.ShoppingCart.GetCart(user.Id));
                if (cart == null || cart.Total <= 0)
                {
                    _logger.LogWarning("Empty cart attempted to place order for user {UserId}", user.Id);
                    TempData["error"] = "Giỏ hàng trống, không thể đặt hàng!";
                    return RedirectToAction(nameof(Summary));
                }

                if (summaryVM.RememberAddress)
                {
                    user.State = summaryVM.State;
                    user.City = summaryVM.City;
                    user.StreetAddress = summaryVM.StreetAddress;
                    user.PostalCode = summaryVM.PostalCode;
                    user.PhoneNumber = summaryVM.PhoneNumber;
                    
                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        _logger.LogWarning("Failed to update user address: {Errors}", string.Join(", ", updateResult.Errors));
                    }
                }

                summaryVM.Cart = cart;
                var result = _cartService.PlaceOrder(summaryVM);
                
                if (!result.Success)
                {
                    _logger.LogWarning("Place order failed: {Message}", result.Message);
                    TempData["error"] = result.Message ?? "Lỗi khi đặt hàng";
                    return RedirectToAction(nameof(Summary));
                }

                TempData["success"] = "Đơn hàng đã được đặt thành công!";
                _logger.LogInformation("Order placed successfully for user {UserId}", user.Id);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing order");
                TempData["error"] = "Đã xảy ra lỗi khi đặt hàng.";
                return RedirectToAction(nameof(Summary));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VNPayCheckout(SummaryVM summaryVM)
        {
            try
            {
                string userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID is null in VNPayCheckout");
                    TempData["error"] = "Không tìm thấy thông tin người dùng.";
                    return RedirectToAction(nameof(Index));
                }

                var cart = await Task.Run(() => _unitOfWork.ShoppingCart.GetCart(userId));
                if (cart == null || cart.Total <= 0)
                {
                    _logger.LogWarning("Empty cart attempted VNPay checkout for user {UserId}", userId);
                    TempData["error"] = "Giỏ hàng trống, không thể thanh toán.";
                    return RedirectToAction(nameof(Index));
                }

                decimal totalAmount = cart.Total;
                string orderId = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
                
                HttpContext.Session.SetString("PendingOrderId", orderId);
                HttpContext.Session.SetString("PendingUserId", userId);

                string paymentUrl = _vnPayService.CreatePaymentUrl(HttpContext, totalAmount, orderId, "Checkout");
                _logger.LogInformation("VNPay checkout initiated. OrderId: {OrderId}, UserId: {UserId}, Amount: {Amount}", 
                    orderId, userId, totalAmount);
                
                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VNPayCheckout");
                TempData["error"] = "Đã xảy ra lỗi khi khởi tạo thanh toán.";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> PaymentReturn()
        {
            try
            {
                var vnpayData = Request.Query;
                string vnp_ResponseCode = vnpayData["vnp_ResponseCode"].ToString();
                string pendingOrderId = HttpContext.Session.GetString("PendingOrderId") ?? string.Empty;
                string userId = HttpContext.Session.GetString("PendingUserId") ?? string.Empty;

                if (string.IsNullOrEmpty(pendingOrderId) || string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Missing session data in PaymentReturn");
                    TempData["error"] = "Không tìm thấy đơn hàng.";
                    return RedirectToAction(nameof(Summary));
                }

                var cart = await Task.Run(() => _unitOfWork.ShoppingCart.GetCart(userId));

                if (vnp_ResponseCode == "00") // Thành công
                {
                    if (cart != null && cart.Items.Any())
                    {
                        var summaryVM = new SummaryVM { Cart = cart };
                        var result = _cartService.PlaceOrder(summaryVM);

                        if (result.Success)
                        {
                            await Task.Run(() => _unitOfWork.ShoppingCart.ClearCart(userId));
                            HttpContext.Session.Remove("PendingOrderId");
                            HttpContext.Session.Remove("PendingUserId");

                            _logger.LogInformation("Payment successful and order placed. OrderId: {OrderId}, UserId: {UserId}", 
                                pendingOrderId, userId);
                            
                            TempData["success"] = "Thanh toán thành công! Đơn hàng của bạn đã được đặt.";
                            return RedirectToAction("Index", "Home");
                        }
                        
                        _logger.LogWarning("Payment successful but order placement failed. OrderId: {OrderId}", pendingOrderId);
                        TempData["error"] = "Thanh toán thành công nhưng không thể tạo đơn hàng!";
                    }
                    else
                    {
                        _logger.LogWarning("Payment successful but cart is empty. OrderId: {OrderId}", pendingOrderId);
                        TempData["error"] = "Thanh toán thành công nhưng giỏ hàng trống!";
                    }
                }
                else
                {
                    _logger.LogInformation("Payment failed or cancelled. ResponseCode: {ResponseCode}, OrderId: {OrderId}", 
                        vnp_ResponseCode, pendingOrderId);
                    TempData["error"] = "Thanh toán thất bại hoặc bị hủy!";
                }

                return RedirectToAction(nameof(Summary));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PaymentReturn");
                TempData["error"] = "Đã xảy ra lỗi khi xử lý kết quả thanh toán.";
                return RedirectToAction(nameof(Summary));
            }
        }
    }
}
