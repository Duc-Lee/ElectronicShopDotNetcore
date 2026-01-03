using ElectronicShopMVC.DataAccess.Repository.IRepository;
using ElectronicShopMVC.Model.ViewModels;
using ElectronicShopMVC.Model;
using ElectronicShopMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ElectronicShopMVC.Utility;

namespace ElectronicShopMVC.Areas.Admin.Controllers
{

    [Area("Admin")]
    [Authorize(Roles = StaticDetails.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IImageService _imageService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(
            IUnitOfWork unitOfWork, 
            IWebHostEnvironment webHostEnvironment, 
            IImageService imageService,
            ILogger<ProductController> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _webHostEnvironment = webHostEnvironment ?? throw new ArgumentNullException(nameof(webHostEnvironment));
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await Task.Run(() => 
                    _unitOfWork.Product.GetAll(includeProperties: "Category").ToList());
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products in admin Index");
                TempData["error"] = "Đã xảy ra lỗi khi tải danh sách sản phẩm.";
                return View(new List<Product>());
            }
        }

        // GET: Product/Upsert/{id?}
        public async Task<IActionResult> Upsert(int? id)
        {
            try
            {
                var categories = await Task.Run(() => _unitOfWork.Category.GetAll().ToList());
                
                ProductVM productVM = new()
                {
                    CategoryList = categories.Select(u => new SelectListItem
                    {
                        Text = u.Name ?? string.Empty,
                        Value = u.Id.ToString()
                    }),
                    Product = new Product(),
                };

                if (id == null || id == 0)
                {
                    // Tạo mới
                    return View(productVM);
                }

                // Cập nhật
                var product = await Task.Run(() => _unitOfWork.Product.GetById((int)id));
                
                if (product == null)
                {
                    TempData["error"] = "Không tìm thấy sản phẩm.";
                    return RedirectToAction(nameof(Index));
                }
                
                productVM.Product = product;
                return View(productVM);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product for upsert with id: {ProductId}", id);
                TempData["error"] = "Đã xảy ra lỗi khi tải thông tin sản phẩm.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Product/Upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(ProductVM productVM, IFormFile? file)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var categories = await Task.Run(() => _unitOfWork.Category.GetAll().ToList());
                    productVM.CategoryList = categories.Select(u => new SelectListItem
                    {
                        Text = u.Name ?? string.Empty,
                        Value = u.Id.ToString()
                    });
                    return View(productVM);
                }

                if (file != null && file.Length > 0)
                {
                    try
                    {
                        string wwwRootPath = _webHostEnvironment.WebRootPath ?? string.Empty;
                        if (string.IsNullOrEmpty(wwwRootPath))
                        {
                            _logger.LogError("WebRootPath is null or empty");
                            TempData["error"] = "Lỗi cấu hình server.";
                            return View(productVM);
                        }

                        string productPath = Path.Combine(wwwRootPath, "images");
                        
                        // Ensure directory exists
                        if (!Directory.Exists(productPath))
                        {
                            Directory.CreateDirectory(productPath);
                        }

                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                        {
                            _imageService.DeleteIfExists(wwwRootPath, productVM.Product.ImageUrl);
                        }

                        string fileExtension = Path.GetExtension(file.FileName);
                        if (string.IsNullOrEmpty(fileExtension))
                        {
                            fileExtension = ".jpg";
                        }

                        string fileName = $"{Guid.NewGuid()}{fileExtension}";
                        string filePath = Path.Combine(productPath, fileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }

                        productVM.Product.ImageUrl = $"/images/{fileName}";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error saving product image");
                        TempData["error"] = "Đã xảy ra lỗi khi lưu hình ảnh.";
                        var categories = await Task.Run(() => _unitOfWork.Category.GetAll().ToList());
                        productVM.CategoryList = categories.Select(u => new SelectListItem
                        {
                            Text = u.Name ?? string.Empty,
                            Value = u.Id.ToString()
                        });
                        return View(productVM);
                    }
                }

                if (productVM.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(productVM.Product);
                    _logger.LogInformation("Adding new product: {ProductTitle}", productVM.Product.Title);
                }
                else
                {
                    _unitOfWork.Product.Update(productVM.Product);
                    _logger.LogInformation("Updating product: {ProductId}", productVM.Product.Id);
                }

                _unitOfWork.Save();
                
                TempData["success"] = productVM.Product.Id == 0
                    ? "Thêm sản phẩm thành công"
                    : "Cập nhật sản phẩm thành công";
                
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Upsert POST for product id: {ProductId}", productVM.Product.Id);
                TempData["error"] = "Đã xảy ra lỗi khi lưu sản phẩm.";
                
                var categories = await Task.Run(() => _unitOfWork.Category.GetAll().ToList());
                productVM.CategoryList = categories.Select(u => new SelectListItem
                {
                    Text = u.Name ?? string.Empty,
                    Value = u.Id.ToString()
                });
                return View(productVM);
            }
        }

        #region API_CALLS
        [HttpGet]
        [AllowAnonymous] // temporary: allow anonymous access for debugging UI data load
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var products = await Task.Run(() => 
                    _unitOfWork.Product.GetAll(includeProperties: "Category").ToList());
                return Json(new { data = products });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                return Json(new { data = new List<Product>(), error = "Đã xảy ra lỗi khi tải dữ liệu." });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (id == null || id <= 0)
                {
                    _logger.LogWarning("Invalid product id for deletion: {ProductId}", id);
                    return Json(new { success = false, message = "ID sản phẩm không hợp lệ." });
                }

                var productToBeDeleted = await Task.Run(() => _unitOfWork.Product.GetById((int)id));
                
                if (productToBeDeleted == null)
                {
                    _logger.LogWarning("Product not found for deletion: {ProductId}", id);
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm." });
                }

                // Xóa ảnh nếu tồn tại
                if (!string.IsNullOrEmpty(productToBeDeleted.ImageUrl))
                {
                    try
                    {
                        string wwwRootPath = _webHostEnvironment.WebRootPath ?? string.Empty;
                        if (!string.IsNullOrEmpty(wwwRootPath))
                        {
                            _imageService.DeleteIfExists(wwwRootPath, productToBeDeleted.ImageUrl);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error deleting image for product {ProductId}", id);
                        // Continue with product deletion even if image deletion fails
                    }
                }

                _unitOfWork.Product.Remove(productToBeDeleted);
                _unitOfWork.Save();
                
                _logger.LogInformation("Product deleted successfully: {ProductId}", id);
                return Json(new { success = true, message = "Xóa thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa sản phẩm." });
            }
        }
        #endregion
    }
}
