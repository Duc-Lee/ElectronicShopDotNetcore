using ElectronicShopMVC.Model;

namespace ElectronicShopMVC.DataAccess.Repository.IRepository
{
    public interface IOrderStatusHistoryRepository : IRepository<OrderStatusHistory>
    {
        void Update(OrderStatusHistory orderStatusHistory);
    }
}
