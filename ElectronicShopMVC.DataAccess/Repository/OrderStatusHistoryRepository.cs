using ElectronicShopMVC.DataAccess.Data;
using ElectronicShopMVC.DataAccess.Repository.IRepository;
using ElectronicShopMVC.Model;
using System;

namespace ElectronicShopMVC.DataAccess.Repository
{
    public class OrderStatusHistoryRepository : Repository<OrderStatusHistory>, IOrderStatusHistoryRepository
    {
        private readonly ApplicationDbContext _db;

        public OrderStatusHistoryRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderStatusHistory orderStatusHistory)
        {
            _db.OrderStatusHistories.Update(orderStatusHistory);
        }
    }
}
