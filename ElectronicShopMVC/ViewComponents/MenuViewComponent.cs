using ElectronicShopMVC.DataAccess.Repository.IRepository;
using ElectronicShopMVC.Model;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicShopMVC.ViewComponents
{
    public class MenuViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MenuViewComponent> _logger;

        public MenuViewComponent(IUnitOfWork unitOfWork, ILogger<MenuViewComponent> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IViewComponentResult Invoke()
        {
            try
            {
                var categoryList = _unitOfWork.Category?.GetAll() ?? Enumerable.Empty<Category>();
                return View(categoryList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading categories in MenuViewComponent");
                return View(Enumerable.Empty<Category>());
            }
        }
    }
}
