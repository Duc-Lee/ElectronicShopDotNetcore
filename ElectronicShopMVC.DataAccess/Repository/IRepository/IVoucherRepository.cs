using ElectronicShopMVC.Model;

namespace ElectronicShopMVC.DataAccess.Repository.IRepository
{
    public interface IVoucherRepository : IRepository<Voucher>
    {
        void Update(Voucher voucher);
    }
}
