using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicShopMVC.Model.DTO
{
    public class CartDTO
    {
        public IEnumerable<CartItemDTO>? Items { get; set; }
        public int ItemsQuantity { get; set; }
        public PriceDTO? Subtotal { get; set; }
        public PriceDTO? Shipping { get; set; }
        public PriceDTO? Vat { get; set; }
        public PriceDTO? Total { get; set; }
    }
}
