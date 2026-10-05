// Họ và tên: [Điền họ tên SV 4]
// Mã sinh viên: [Điền mã SV 4]
// Nội dung thực hiện: ViewModel DatSanXuLyChiTietViewModel - Gom thông tin chi tiết, cảnh báo trùng lịch
// và các thao tác được phép cho một lượt đặt sân (Module 4)

using QLDatSanTheThao.Models.Entities;

namespace QLDatSanTheThao.Models.ViewModels
{
    public class DatSanXuLyChiTietViewModel
    {
        public DatSan DatSan { get; set; } = null!;

        // Các lịch đã xác nhận/hoàn thành đang trùng khung giờ với đơn này
        public List<DatSan> LichDaXacNhanTrung { get; set; } = new List<DatSan>();

        // Số đơn khác đang "Chờ xử lý" cùng sân, trùng khung giờ (sẽ bị chặn nếu đơn này được xác nhận)
        public int SoDonChoTrung { get; set; }

        // Số lịch đã xác nhận/hoàn thành của sân trong ngày đặt (kiểm tra khả năng đáp ứng)
        public int SoLichTrongNgay { get; set; }

        // Các lỗi điều kiện khiến chưa thể xác nhận (chỉ tính khi đơn đang Chờ xử lý)
        public List<string> CanhBao { get; set; } = new List<string>();

        public bool CoTheXacNhan { get; set; }
        public bool CoTheHoanThanh { get; set; }
        public bool CoTheHuy { get; set; }
        public string? LyDoKhongTheHuy { get; set; }

        public decimal TongTienDichVu => DatSan.ChiTietDatSans.Sum(c => c.ThanhTien);
    }
}