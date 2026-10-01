// Họ và tên: [Điền tên SV 5]
// Mã sinh viên: [Điền mã SV 5]
// Nội dung thực hiện: Entity ChiTietDatSan - Quản lý chi tiết dịch vụ đi kèm đặt sân

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDatSanTheThao.Models.Entities
{
    [Table("ChiTietDatSan")]
    public class ChiTietDatSan
    {
        [Key]
        public int MaChiTiet { get; set; }

        [Required]
        public int MaDatSan { get; set; }
        [ForeignKey("MaDatSan")]
        public virtual DatSan DatSan { get; set; } = null!;

        [Required]
        public int MaDichVu { get; set; }
        [ForeignKey("MaDichVu")]
        public virtual DichVu DichVu { get; set; } = null!;

        [Required(ErrorMessage = "Số lượng bắt buộc nhập")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int SoLuong { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá không hợp lệ")]
        public decimal DonGia { get; set; }

        [Required]
        public decimal ThanhTien { get; set; }

        public string? GhiChu { get; set; }

        [StringLength(50)]
        public string? TrangThai { get; set; }
    }
}