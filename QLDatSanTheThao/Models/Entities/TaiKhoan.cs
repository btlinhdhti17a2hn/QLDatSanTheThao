using System.ComponentModel.DataAnnotations;

namespace QLDatSanTheThao.Models.Entities
{
    public class TaiKhoan
    {
        [Key]
        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50)]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100)]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Vai trò")]
        public string VaiTro { get; set; } // "Admin", "NhanVien", "KhachHang"

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;

        // Navigation
        public KhachHang KhachHang { get; set; }
    }
}

