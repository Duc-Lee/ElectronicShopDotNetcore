using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicShopMVC.Model.ViewModels
{
    public class SummaryVM
    {
        public Cart? Cart { get; set; }
        [Display(Name = "Địa chỉ đường")]
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        [Display(Name = "Mã bưu điệ")]
        public string? PostalCode { get; set; }
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }
        public string? CouponCode { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAfterDiscount { get; set; }
        public bool RememberAddress { get; set; }
        
        // Audit and Transaction metadata
        public string? PaymentMethod { get; set; }
        public string? TransactionReference { get; set; }
        public string? PaymentStatus { get; set; }
    }
}
