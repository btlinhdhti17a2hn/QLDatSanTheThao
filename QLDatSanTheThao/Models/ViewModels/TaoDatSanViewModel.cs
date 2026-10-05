// Họ và tên: [Điền tên SV 3]
// Mã sinh viên: [Điền mã SV 3]
// Nội dung thực hiện: ViewModel TaoDatSanViewModel - Phục vụ form Tạo đặt sân kèm chọn Dịch vụ (Module 3 & Module 5)

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace QLDatSanTheThao.Models.ViewModels
{
    public class DichVuDatSanItem
    {
        public int MaDichVu { get; set; }
        public string TenDichVu { get; set; } = null!;
        public decimal DonGia { get; set; }
        public int SoLuong { get; set; } = 0;
    }

    public class TaoDatSanViewModel
    {
        public int MaSan { get; set; }

        [ValidateNever]
        public string TenSan { get; set; } = string.Empty;

        [ValidateNever]
        public string TenLoaiSan { get; set; } = string.Empty;

        public decimal DonGiaTheoGio { get; set; }
        public TimeSpan GioMoCua { get; set; }
        public TimeSpan GioDongCua { get; set; }

        [ValidateNever]
        public string DiaChi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn ngày sử dụng sân")]
        [DataType(DataType.Date)]
        public DateTime NgaySuDung { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Vui lòng chọn giờ bắt đầu")]
        public TimeSpan GioBatDau { get; set; } = new TimeSpan(17, 0, 0); // Mặc định 17:00

        [Required(ErrorMessage = "Vui lòng chọn giờ kết thúc")]
        public TimeSpan GioKetThuc { get; set; } = new TimeSpan(19, 0, 0); // Mặc định 19:00

        [Range(0, double.MaxValue, ErrorMessage = "Tiền cọc không hợp lệ")]
        public decimal TienCoc { get; set; } = 100000;

        public string? GhiChu { get; set; }

        // Danh sách dịch vụ đi kèm (Nước uống, thuê bóng, thuê vợt...)
        public List<DichVuDatSanItem> DanhSachDichVu { get; set; } = new List<DichVuDatSanItem>();

        // Tiền thuê sân
        public decimal TienThueSan
        {
            get
            {
                double soGio = (GioKetThuc - GioBatDau).TotalHours;
                if (soGio <= 0) return 0;
                return (decimal)soGio * DonGiaTheoGio;
            }
        }

        // Tiền dịch vụ
        public decimal TongTienDichVu
        {
            get
            {
                if (DanhSachDichVu == null || !DanhSachDichVu.Any()) return 0;
                return DanhSachDichVu.Sum(d => d.SoLuong * d.DonGia);
            }
        }

        // Tổng tiền dự kiến (Sân + Dịch vụ)
        public decimal TongTienDuKien => TienThueSan + TongTienDichVu;
    }
}
