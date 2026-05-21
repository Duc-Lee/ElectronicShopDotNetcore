using System;
using System.ComponentModel.DataAnnotations;

namespace ElectronicShopMVC.Model
{
    public class Voucher
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Code { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Title { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string? DiscountType { get; set; } // "Percentage" | "FixedAmount" | "FreeShipping"

        [Required]
        public decimal DiscountValue { get; set; }

        public decimal MinOrderAmount { get; set; }

        public decimal? MaxDiscountAmount { get; set; }

        public int MaxUses { get; set; }

        public int UsedCount { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
    }
}
