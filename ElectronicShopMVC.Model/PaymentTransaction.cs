using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicShopMVC.Model
{
    public class PaymentTransaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        [MaxLength(200)]
        public string? TransactionReference { get; set; }

        [Required]
        [MaxLength(50)]
        public string? PaymentMethod { get; set; } // "COD" | "VNPay" | "BankTransfer"

        [Required]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Status { get; set; } // "Pending" | "Success" | "Failed" | "Refunded"

        public string? ResponsePayload { get; set; } // raw gateway payload

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
