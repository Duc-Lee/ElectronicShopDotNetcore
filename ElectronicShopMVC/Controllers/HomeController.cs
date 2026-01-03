using System.Diagnostics;
using ElectronicShopMVC.DataAccess.Repository.IRepository;
using ElectronicShopMVC.Model;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicShopMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var productList = await Task.Run(() => 
                    _unitOfWork.Product.GetAll(includeProperties: "Category").ToList());
                
                return View(productList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products in Index action");
                TempData["error"] = "Đã xảy ra lỗi khi tải danh sách sản phẩm.";
                return View(new List<Product>());
            }
        }

        public async Task<IActionResult> Category(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid category id: {CategoryId}", id);
                    return NotFound();
                }

                var category = await Task.Run(() => _unitOfWork.Category?.GetById(id));
                
                if (category == null)
                {
                    _logger.LogWarning("Category not found with id: {CategoryId}", id);
                    return NotFound();
                }

                ViewData["CategoryName"] = category.Name ?? string.Empty;

                var productList = await Task.Run(() => 
                    _unitOfWork.Product.GetAll()
                        .Where(product => product.CategoryId == id)
                        .ToList());

                return View(productList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading category {CategoryId}", id);
                TempData["error"] = "Đã xảy ra lỗi khi tải danh mục.";
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                if (id <= 0)
                {
                    _logger.LogWarning("Invalid product id: {ProductId}", id);
                    return NotFound();
                }

                var product = await Task.Run(() => 
                    _unitOfWork.Product.Get(u => u.Id == id, includeProperties: "Category"));

                if (product == null)
                {
                    _logger.LogWarning("Product not found with id: {ProductId}", id);
                    return NotFound();
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product details for id: {ProductId}", id);
                TempData["error"] = "Đã xảy ra lỗi khi tải chi tiết sản phẩm.";
                return RedirectToAction(nameof(Index));
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
            });
        }
    }
}
