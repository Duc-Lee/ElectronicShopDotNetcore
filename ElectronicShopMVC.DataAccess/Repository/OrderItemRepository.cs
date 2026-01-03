using ElectronicShopMVC.DataAccess.Data;
using ElectronicShopMVC.DataAccess.Repository.IRepository;
using ElectronicShopMVC.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicShopMVC.DataAccess.Repository
{
    public class OrderItemRepository : Repository<OrderItem>, IOrderItemRepository
    {
        private readonly ApplicationDbContext _db;

        public OrderItemRepository(ApplicationDbContext db) : base(db) 
        {
            _db = db;
        }

        public OrderItem? GetById(int id)
        {
            return _db.OrderItems
                 .Include(p => p.Product)
                 .FirstOrDefault(p => p.Id == id);
        }
    }
}
