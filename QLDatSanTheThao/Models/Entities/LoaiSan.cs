using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDatSanTheThao.Models.Entities
{
    public class LoaiSan
    {
        [Key]
        public int MaLoaiSan { get; set; }

        [Required(ErrorMessage = "Tên loại sân không được để trống")]
        [StringLength(100)]
        [Display(Name = "Tên loại")]
        public string TenLoai { get; set; }

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string MoTa { get; set; }

        [Range(1, 100, ErrorMessage = "Số người tối đa phải lớn hơn 0")]
        [Display(Name = "Số người tối đa")]
        public int SoNguoiToiDa { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá phải >= 0")]
        [Display(Name = "Đơn giá theo giờ")]
        public decimal DonGiaTheoGio { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        // Navigation
        public ICollection<SanTheThao> DanhSachSan { get; set; }
    }
}
