using ElectronicShopMVC.DataAccess.Repository.IRepository;
using ElectronicShopMVC.Model;
using ElectronicShopMVC.Model.ViewModels;
using ElectronicShopMVC.Utility;
using Microsoft.Extensions.DependencyInjection;

namespace ElectronicShopMVC.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<CartService> _logger;

        public CartService(
            IUnitOfWork unitOfWork, 
            IServiceScopeFactory serviceScopeFactory,
            ILogger<CartService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ServiceResult AddItem(int productId, int quantity, string userId)
        {
            try
            {
                if (productId <= 0)
                {
                    _logger.LogWarning("Invalid product id: {ProductId}", productId);
                    return new ServiceResult { Success = false, Message = "ID sản phẩm không hợp lệ." };
                }

                if (quantity <= 0)
                {
                    _logger.LogWarning("Invalid quantity: {Quantity}", quantity);
                    return new ServiceResult { Success = false, Message = "Số lượng phải lớn hơn 0." };
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("Invalid user id");
                    return new ServiceResult { Success = false, Message = "Thông tin người dùng không hợp lệ." };
                }

                var product = _unitOfWork.Product.GetById(productId);
                if (product == null)
                {
                    _logger.LogWarning("Product not found: {ProductId}", productId);
                    return new ServiceResult { Success = false, Message = "Sản phẩm không tồn tại." };
                }

                var itemInCart = _unitOfWork.CartItem.GetByUserId(userId)
                    .FirstOrDefault(p => p.productId == productId);

                if (itemInCart != null)
                {
                    itemInCart.quantity += quantity;
                    _unitOfWork.CartItem.Update(itemInCart);
                    _logger.LogInformation("Updated cart item quantity for product {ProductId}, user {UserId}", productId, userId);
                }
                else
                {
                    var shoppingCartItem = new ShoppingCartItem
                    {
                        productId = productId,
                        quantity = quantity,
                        userId = userId
                    };
                    _unitOfWork.CartItem.Add(shoppingCartItem);
                    _logger.LogInformation("Added new cart item for product {ProductId}, user {UserId}", productId, userId);
                }
                
                _unitOfWork.Save();
                return new ServiceResult { Success = true, Message = "Sản phẩm đã được thêm vào giỏ hàng." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart. ProductId: {ProductId}, UserId: {UserId}", productId, userId);
                return new ServiceResult { Success = false, Message = "Đã xảy ra lỗi khi thêm sản phẩm vào giỏ hàng." };
            }
        }

        public ServiceResult PlaceOrder(SummaryVM summaryVM)
        {
            try
            {
                if (summaryVM == null)
                {
                    _logger.LogWarning("PlaceOrder called with null SummaryVM");
                    return new ServiceResult { Success = false, Message = "Thông tin đơn hàng không hợp lệ." };
                }

                if (summaryVM.Cart == null || !summaryVM.Cart.Items.Any())
                {
                    _logger.LogWarning("PlaceOrder called with empty cart for user {UserId}", summaryVM.Cart?.UserId);
                    return new ServiceResult { Success = false, Message = "Giỏ hàng trống." };
                }

                if (string.IsNullOrWhiteSpace(summaryVM.Cart.UserId))
                {
                    _logger.LogWarning("PlaceOrder called with invalid user id");
                    return new ServiceResult { Success = false, Message = "Thông tin người dùng không hợp lệ." };
                }

                var order = new Order
                {
                    OrderId = Guid.NewGuid().ToString(),
                    UserId = summaryVM.Cart.UserId,
                    Date = DateTime.UtcNow,
                    Status = Constants.OrderStatus.Pending,
                    StreetAddress = summaryVM.StreetAddress,
                    City = summaryVM.City,
                    State = summaryVM.State,
                    PostalCode = summaryVM.PostalCode,
                    PhoneNumber = summaryVM.PhoneNumber,
                };

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var scopedUnitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    var items = summaryVM.Cart.Items.ToList();
                    decimal subtotal = summaryVM.Cart.Subtotal;
                    decimal shipping = summaryVM.Cart.Shipping;
                    decimal discount = 0;

                    Voucher? voucher = null;
                    if (!string.IsNullOrEmpty(summaryVM.CouponCode))
                    {
                        string code = summaryVM.CouponCode.Trim().ToUpper();
                        voucher = scopedUnitOfWork.Voucher.Get(v => v.Code == code && v.IsActive && !v.IsDeleted);
                        if (voucher != null)
                        {
                            if (DateTime.UtcNow < voucher.StartDate || DateTime.UtcNow > voucher.EndDate)
                            {
                                _logger.LogWarning("Voucher {Code} has expired or not started yet", code);
                                return new ServiceResult { Success = false, Message = $"Mã giảm giá '{code}' đã hết hạn hoặc chưa đến thời gian áp dụng." };
                            }
                            if (voucher.MaxUses > 0 && voucher.UsedCount >= voucher.MaxUses)
                            {
                                _logger.LogWarning("Voucher {Code} usage limit reached", code);
                                return new ServiceResult { Success = false, Message = $"Mã giảm giá '{code}' đã đạt số lượng sử dụng tối đa." };
                            }
                            if (subtotal < voucher.MinOrderAmount)
                            {
                                _logger.LogWarning("Order subtotal {Subtotal} is less than minimum order amount {MinAmount} for voucher {Code}", subtotal, voucher.MinOrderAmount, code);
                                return new ServiceResult { Success = false, Message = $"Đơn hàng tối thiểu phải đạt {voucher.MinOrderAmount:N0}đ để áp dụng mã giảm giá này." };
                            }

                            if (string.Equals(voucher.DiscountType, "Percentage", StringComparison.OrdinalIgnoreCase))
                            {
                                discount = subtotal * (voucher.DiscountValue / 100m);
                                if (voucher.MaxDiscountAmount.HasValue)
                                {
                                    discount = Math.Min(discount, voucher.MaxDiscountAmount.Value);
                                }
                            }
                            else if (string.Equals(voucher.DiscountType, "FixedAmount", StringComparison.OrdinalIgnoreCase))
                            {
                                discount = Math.Min(subtotal, voucher.DiscountValue);
                            }
                            else if (string.Equals(voucher.DiscountType, "FreeShipping", StringComparison.OrdinalIgnoreCase))
                            {
                                discount = shipping;
                            }

                            order.VoucherId = voucher.Id;
                            order.VoucherCode = voucher.Code;
                            order.VoucherDiscount = discount;

                            voucher.UsedCount += 1;
                            scopedUnitOfWork.Voucher.Update(voucher);
                        }
                        else
                        {
                            return new ServiceResult { Success = false, Message = $"Mã giảm giá '{code}' không hợp lệ hoặc không tồn tại." };
                        }
                    }

                    // Populate payment and fulfillment statuses
                    order.PaymentStatus = summaryVM.PaymentStatus ?? "Pending";
                    order.FulfillmentStatus = "Pending";

                    decimal totalDistributed = 0;
                    int itemCount = items.Count;

                    for (int i = 0; i < itemCount; i++)
                    {
                        var item = items[i];
                        var product = scopedUnitOfWork.Product.GetById(item.productId);
                        
                        if (product == null)
                        {
                            _logger.LogWarning("Product not found when placing order: {ProductId}", item.productId);
                            return new ServiceResult { Success = false, Message = $"Sản phẩm với ID {item.productId} không tồn tại." };
                        }

                        decimal originalItemSubtotal = product.Price * item.quantity;
                        decimal itemDiscount = 0;

                        if (discount > 0 && subtotal > 0)
                        {
                            if (i == itemCount - 1)
                            {
                                itemDiscount = discount - totalDistributed;
                            }
                            else
                            {
                                itemDiscount = Math.Round(discount * (originalItemSubtotal / subtotal), 2);
                                totalDistributed += itemDiscount;
                            }
                        }

                        decimal discountedPrice = Math.Max(0, (originalItemSubtotal - itemDiscount) / item.quantity);

                        order.Items.Add(new OrderItem
                        {
                            ProductId = item.productId,
                            Quantity = item.quantity,
                            Price = Math.Round(discountedPrice, 2),
                            OriginalPrice = product.Price,
                            DiscountAmount = Math.Round(itemDiscount, 2)
                        });
                    }

                    scopedUnitOfWork.Order.Add(order);

                    // Record initial order status history
                    var statusHistory = new OrderStatusHistory
                    {
                        Order = order,
                        OldStatus = "None",
                        NewStatus = Constants.OrderStatus.Pending.ToString(),
                        ChangedBy = summaryVM.Cart.UserId,
                        ChangedAt = DateTime.UtcNow,
                        Notes = "Đơn hàng được tạo thành công hệ thống."
                    };
                    scopedUnitOfWork.OrderStatusHistory.Add(statusHistory);

                    // Record payment transaction
                    string paymentMethod = summaryVM.PaymentMethod ?? "COD";
                    var transaction = new PaymentTransaction
                    {
                        Order = order,
                        PaymentMethod = paymentMethod,
                        Amount = Math.Max(0, summaryVM.Cart.Total - discount),
                        Status = (paymentMethod == "VNPay" ? "Success" : "Pending"),
                        TransactionReference = summaryVM.TransactionReference,
                        ResponsePayload = paymentMethod == "VNPay" 
                            ? "Thanh toán qua cổng VNPay thành công." 
                            : "Thanh toán bằng tiền mặt khi nhận hàng (COD).",
                        CreatedAt = DateTime.UtcNow
                    };
                    scopedUnitOfWork.PaymentTransaction.Add(transaction);

                    scopedUnitOfWork.ShoppingCart.ClearCart(summaryVM.Cart.UserId);
                    scopedUnitOfWork.Save();
                }

                _logger.LogInformation("Order placed successfully. OrderId: {OrderId}, UserId: {UserId}", order.OrderId, order.UserId);
                return new ServiceResult { Success = true, Message = "Đơn hàng đã được đặt thành công." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error placing order for user {UserId}", summaryVM?.Cart?.UserId);
                return new ServiceResult { Success = false, Message = "Đã xảy ra lỗi khi đặt hàng." };
            }
        }

        public ServiceResult UpdateQuantity(int productId, int quantity, string userId)
        {
            try
            {
                if (productId <= 0)
                {
                    _logger.LogWarning("Invalid product id: {ProductId}", productId);
                    return new ServiceResult { Success = false, Message = "ID sản phẩm không hợp lệ." };
                }

                if (quantity <= 0)
                {
                    _logger.LogWarning("Invalid quantity: {Quantity}", quantity);
                    return new ServiceResult { Success = false, Message = "Số lượng phải lớn hơn 0." };
                }

                if (string.IsNullOrWhiteSpace(userId))
                {
                    _logger.LogWarning("Invalid user id");
                    return new ServiceResult { Success = false, Message = "Thông tin người dùng không hợp lệ." };
                }

                var shoppingCartItem = _unitOfWork.CartItem.GetByUserId(userId)
                    .FirstOrDefault(p => p.productId == productId);

                if (shoppingCartItem == null)
                {
                    _logger.LogWarning("Cart item not found. ProductId: {ProductId}, UserId: {UserId}", productId, userId);
                    return new ServiceResult { Success = false, Message = "Không tìm thấy sản phẩm trong giỏ hàng." };
                }

                shoppingCartItem.quantity = quantity;
                _unitOfWork.CartItem.Update(shoppingCartItem);
                _unitOfWork.Save();
                
                _logger.LogInformation("Updated cart item quantity. ProductId: {ProductId}, UserId: {UserId}, Quantity: {Quantity}", 
                    productId, userId, quantity);
                
                return new ServiceResult { Success = true, Message = "Đã cập nhật giỏ hàng." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart quantity. ProductId: {ProductId}, UserId: {UserId}", productId, userId);
                return new ServiceResult { Success = false, Message = "Đã xảy ra lỗi khi cập nhật giỏ hàng." };
            }
        }
    }
}
