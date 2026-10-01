// Họ và tên: [Điền tên SV 4]
// Mã sinh viên: [Điền mã SV 4]
// Nội dung thực hiện: Entity DatSan - Xử lý đặt sân

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDatSanTheThao.Models.Entities
{
    [Table("DatSan")]
    public class DatSan
    {
        [Key]
        public int MaDatSan { get; set; }

        [Required]
        public int MaKhachHang { get; set; }
        [ForeignKey("MaKhachHang")]
        public virtual KhachHang KhachHang { get; set; } = null!;

        [Required]
        public int MaSan { get; set; }
        [ForeignKey("MaSan")]
        public virtual SanTheThao SanTheThao { get; set; } = null!;

        [Required]
        public DateTime NgayDat { get; set; } = DateTime.Now;

        [Required]
        public DateTime GioBatDau { get; set; }

        [Required]
        public DateTime GioKetThuc { get; set; }

        [Range(0, double.MaxValue)]
        public decimal DonGia { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TienCoc { get; set; }

        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = "Chờ xử lý";

        public virtual ICollection<ChiTietDatSan> ChiTietDatSans { get; set; } = new List<ChiTietDatSan>();
    }
}