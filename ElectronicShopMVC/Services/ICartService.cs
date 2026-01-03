using ElectronicShopMVC.Model.ViewModels;

namespace ElectronicShopMVC.Services
{
    public interface ICartService
    {
        ServiceResult AddItem(int productId, int quantity, string userId);
        ServiceResult PlaceOrder(SummaryVM summaryVM);
        ServiceResult UpdateQuantity(int productId, int quantity, string? userId);
    }
}
