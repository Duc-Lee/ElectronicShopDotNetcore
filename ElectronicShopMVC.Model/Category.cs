using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicShopMVC.Model
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(30)]
        [Display(Name = "Tên thể loại")]
        public string? Name { get; set; }
        [Display(Name = "Thứ tự hiển thị")]
        [Range(1, 100, ErrorMessage = "Thứ tự hiển thị phải nằm trong khoảng từ 1 đến 100.")]
        public int DisplayOrder { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [MaxLength(100)]
        public string? UpdatedBy { get; set; }
    }
}
