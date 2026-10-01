// Họ và tên: [Điền tên SV 1]
// Mã sinh viên: [Điền mã SV 1]
// Nội dung thực hiện: Entity LoaiSan - Quản lý dữ liệu nền

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDatSanTheThao.Models.Entities
{
    [Table("LoaiSan")]
    public class LoaiSan
    {
        [Key]
        public int MaLoaiSan { get; set; }

        [Required(ErrorMessage = "Tên loại sân không được để trống")]
        [StringLength(100)]
        public string TenLoai { get; set; } = null!;

        public string? MoTa { get; set; }

        [Range(1, 100, ErrorMessage = "Số người tối đa phải lớn hơn 0")]
        public int SoNguoiToiDa { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn hoặc bằng 0")]
        public decimal DonGiaTheoGio { get; set; }

        public bool TrangThai { get; set; } = true;

        public virtual ICollection<SanTheThao> SanTheThaos { get; set; } = new List<SanTheThao>();
    }
}