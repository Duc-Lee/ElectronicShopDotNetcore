using ElectronicShopMVC.DataAccess.Repository.IRepository;
using ElectronicShopMVC.Model;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicShopMVC.ViewComponents
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FooterViewComponent> _logger;

        public FooterViewComponent(IUnitOfWork unitOfWork, ILogger<FooterViewComponent> logger)
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
                _logger.LogError(ex, "Error loading categories in FooterViewComponent");
                return View(Enumerable.Empty<Category>());
            }
        }
    }
}
