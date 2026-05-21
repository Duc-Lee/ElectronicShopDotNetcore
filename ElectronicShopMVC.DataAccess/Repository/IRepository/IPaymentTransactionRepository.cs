using ElectronicShopMVC.Model;

namespace ElectronicShopMVC.DataAccess.Repository.IRepository
{
    public interface IPaymentTransactionRepository : IRepository<PaymentTransaction>
    {
        void Update(PaymentTransaction paymentTransaction);
    }
}
