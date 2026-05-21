using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElectronicShopMVC.Model
{
    public class OrderStatusHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        [Required]
        [MaxLength(50)]
        public string? OldStatus { get; set; }

        [Required]
        [MaxLength(50)]
        public string? NewStatus { get; set; }

        [MaxLength(100)]
        public string? ChangedBy { get; set; }

        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
