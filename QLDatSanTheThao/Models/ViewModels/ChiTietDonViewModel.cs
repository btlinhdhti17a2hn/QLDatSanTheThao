// Họ và tên: Tạ Đình Kim 
// Mã sinh viên: 23103100315
// Nội dung thực hiện: ViewModel hiển thị đơn đặt sân + danh sách dịch vụ đã dùng
// + tổng tiền (tiền sân + tiền dịch vụ - tiền cọc).

using QLDatSanTheThao.Models.Entities;

namespace QLDatSanTheThao.Models.ViewModels
{
    public class ChiTietDonViewModel
    {
        public DatSan DatSan { get; set; } = null!;
        public List<ChiTietDatSan> ChiTiets { get; set; } = new();

        public double SoGio { get; set; }
        public decimal TienSan { get; set; }
        public decimal TienDichVu { get; set; }

        public decimal TongTien => TienSan + TienDichVu;
        public decimal ConPhaiTra => Math.Max(TongTien - DatSan.TienCoc, 0);

        // Chỉ cho thêm/xóa dịch vụ khi đơn đang ở trạng thái "Đang xử lý"
        public bool ChoPhepThem { get; set; }
    }
}
