using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDatSanTheThao.Models.Entities
{
    public class DatSan
    {
        [Key]
        public int MaDatSan { get; set; }

        [Required]
        [Display(Name = "Khách hàng")]
        public int MaKhachHang { get; set; }

        [ForeignKey("MaKhachHang")]
        public KhachHang KhachHang { get; set; }

        [Required]
        [Display(Name = "Sân")]
        public int MaSan { get; set; }

        [ForeignKey("MaSan")]
        public SanTheThao SanTheThao { get; set; }

        [Required]
        [Display(Name = "Ngày đặt")]
        public DateTime NgayDat { get; set; }

        [Required]
        [Display(Name = "Giờ bắt đầu")]
        public TimeSpan GioBatDau { get; set; }

        [Required]
        [Display(Name = "Giờ kết thúc")]
        public TimeSpan GioKetThuc { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Đơn giá")]
        public decimal DonGia { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tiền cọc")]
        public decimal TienCoc { get; set; }

        [Required, StringLength(20)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "ChoXuLy"; // Luồng bắt buộc: ChoXuLy -> DangXuLy -> HoanThanh (nhánh Huy/TuChoi chỉ khi đủ điều kiện)

        // Navigation
        public ICollection<ChiTietDatSan> ChiTietDichVu { get; set; }
    }
}
