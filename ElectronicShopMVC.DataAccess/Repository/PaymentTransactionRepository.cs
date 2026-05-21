using ElectronicShopMVC.DataAccess.Data;
using ElectronicShopMVC.DataAccess.Repository.IRepository;
using ElectronicShopMVC.Model;
using System;

namespace ElectronicShopMVC.DataAccess.Repository
{
    public class PaymentTransactionRepository : Repository<PaymentTransaction>, IPaymentTransactionRepository
    {
        private readonly ApplicationDbContext _db;

        public PaymentTransactionRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(PaymentTransaction paymentTransaction)
        {
            _db.PaymentTransactions.Update(paymentTransaction);
        }
    }
}
