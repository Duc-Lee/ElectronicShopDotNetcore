using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicShopMVC.Model.DTO
{
    public class CartItemDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public PriceDTO? UnitPrice { get; set; }
        public PriceDTO? TotalPrice { get; set; }
    }
}
