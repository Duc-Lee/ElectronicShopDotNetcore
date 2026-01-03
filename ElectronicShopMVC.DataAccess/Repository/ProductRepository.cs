using ElectronicShopMVC.DataAccess.Data;
using ElectronicShopMVC.DataAccess.Repository.IRepository;
using ElectronicShopMVC.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicShopMVC.DataAccess.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductRepository(ApplicationDbContext db) : base(db) 
        {
            _db = db;
        }

        public Product? GetById(int id)
        {
            return _db.Products.FirstOrDefault(p => p.Id == id);
        }

        public void Update(Product product)
        {
            _db.Products.Update(product);
            Product productFromDb = _db.Products.FirstOrDefault(p => p.Id == product.Id)!;
            if (productFromDb != null)
            {
                productFromDb.Title = product.Title;
                productFromDb.Description = product.Description;
                productFromDb.Price = product.Price;
                productFromDb.Price50 = product.Price50;
                productFromDb.Price100 = product.Price100;
                productFromDb.CategoryId = product.CategoryId;
                productFromDb.Author = product.Author;
                productFromDb.ISBN = product.ISBN;
                productFromDb.Stock = product.Stock;

                if (product.ImageUrl != null)
                {
                    productFromDb.ImageUrl = product.ImageUrl;
                }

            }
        }
    }
}
