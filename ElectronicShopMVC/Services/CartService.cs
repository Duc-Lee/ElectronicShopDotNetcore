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

                    foreach (var item in items)
                    {
                        var product = scopedUnitOfWork.Product.GetById(item.productId);
                        
                        if (product == null)
                        {
                            _logger.LogWarning("Product not found when placing order: {ProductId}", item.productId);
                            return new ServiceResult { Success = false, Message = $"Sản phẩm với ID {item.productId} không tồn tại." };
                        }

                        order.Items.Add(new OrderItem
                        {
                            ProductId = item.productId,
                            Quantity = item.quantity,
                            Price = product.Price
                        });
                    }

                    scopedUnitOfWork.Order.Add(order);
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
