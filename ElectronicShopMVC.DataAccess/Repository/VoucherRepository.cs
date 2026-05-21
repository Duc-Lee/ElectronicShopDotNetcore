using ElectronicShopMVC.DataAccess.Data;
using ElectronicShopMVC.DataAccess.Repository.IRepository;
using ElectronicShopMVC.Model;
using System;
using System.Linq;

namespace ElectronicShopMVC.DataAccess.Repository
{
    public class VoucherRepository : Repository<Voucher>, IVoucherRepository
    {
        private readonly ApplicationDbContext _db;

        public VoucherRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Voucher voucher)
        {
            _db.Vouchers.Update(voucher);
        }
    }
}
