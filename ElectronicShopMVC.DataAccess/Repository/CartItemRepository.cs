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
    public class CartItemRepository : Repository<ShoppingCartItem>, ICartItemRepository
    {
        private readonly ApplicationDbContext _db;

        public CartItemRepository(ApplicationDbContext db) :base(db) 
        {
            _db = db;
        }

        public ShoppingCartItem? GetById(int id)
        {
            return _db.UserProductShoppingCarts.Find(id);
        }

        public IEnumerable<ShoppingCartItem> GetByUserId(string userId)
        {
            return _db.UserProductShoppingCarts.Where(u => u.userId == userId).Include("Product");
        }

        public int GetShoppingCartProductsAmount(string userId)
        {
            return _db.UserProductShoppingCarts.Where(u => u.userId == userId).ToList().Count;
        }

        public IEnumerable<Product> GetUserProducts(string userId)
        {
            return _db.UserProductShoppingCarts.Where(u => u.userId == userId).Select(p => p.Product);
        }

        public void Update(ShoppingCartItem userProductShoppingCart)
        {
            _db.UserProductShoppingCarts.Update(userProductShoppingCart);
        }
    }
}
