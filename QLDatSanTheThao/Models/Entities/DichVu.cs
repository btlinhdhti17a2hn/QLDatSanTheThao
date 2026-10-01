// Họ và tên: [Điền tên SV 5]
// Mã sinh viên: [Điền mã SV 5]
// Nội dung thực hiện: Entity DichVu - Quản lý dịch vụ

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDatSanTheThao.Models.Entities
{
    [Table("DichVu")]
    public class DichVu
    {
        [Key]
        public int MaDichVu { get; set; }

        [Required(ErrorMessage = "Tên dịch vụ không được để trống")]
        [StringLength(100)]
        public string TenDichVu { get; set; } = null!;

        public string? MoTa { get; set; }

        [Required(ErrorMessage = "Đơn giá bắt buộc nhập")]
        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá phải lớn hơn hoặc bằng 0")]
        public decimal DonGia { get; set; }

        public bool TrangThai { get; set; } = true;

        public virtual ICollection<ChiTietDatSan> ChiTietDatSans { get; set; } = new List<ChiTietDatSan>();
    }
}