using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDatSanTheThao.Models.Entities
{
    public class DichVu
    {
        [Key]
        public int MaDichVu { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Tên dịch vụ")]
        public string TenDichVu { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Đơn giá")]
        public decimal DonGia { get; set; }

        [StringLength(20)]
        [Display(Name = "Đơn vị tính")]
        public string DonViTinh { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        // Navigation
        public ICollection<ChiTietDatSan> ChiTietDatSans { get; set; }
    }
}
