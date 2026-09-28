using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDatSanTheThao.Models.Entities
{
    public class KhachHang
    {
        [Key]
        public int MaKhachHang { get; set; }

        [Required]
        [Display(Name = "Tài khoản")]
        public int MaTaiKhoan { get; set; }

        [ForeignKey("MaTaiKhoan")]
        public TaiKhoan TaiKhoan { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; }

        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }

        [StringLength(10)]
        [Display(Name = "Giới tính")]
        public string GioiTinh { get; set; }

        [Required, Phone]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(200)]
        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }

        [Display(Name = "Ngày đăng ký")]
        public DateTime NgayDangKy { get; set; } = DateTime.Now;

        [Display(Name = "Điểm tích lũy")]
        public int DiemTichLuy { get; set; } = 0;

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; }

        // Navigation
        public ICollection<DatSan> DanhSachDatSan { get; set; }
    }
}
