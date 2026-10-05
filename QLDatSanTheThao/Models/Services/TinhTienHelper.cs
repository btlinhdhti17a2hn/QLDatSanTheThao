// Họ và tên: Tạ Đình Kim 
// Mã sinh viên: 23103100315
// Nội dung thực hiện: Lớp hỗ trợ tính tiền sân (đơn giá theo giờ x số giờ) dùng chung cho
// module Dịch vụ - Tính tiền, Dashboard và Thống kê.

namespace QLDatSanTheThao.Services
{
    public static class TinhTienHelper
    {
        // Số giờ thuê = GioKetThuc - GioBatDau (cả hai là DateTime trong entity DatSan)
        public static double SoGio(DateTime gioBatDau, DateTime gioKetThuc)
            => Math.Max((gioKetThuc - gioBatDau).TotalHours, 0);

        // Tiền sân = Đơn giá theo giờ x số giờ
        public static decimal TienSan(decimal donGia, DateTime gioBatDau, DateTime gioKetThuc)
            => Math.Round(donGia * (decimal)SoGio(gioBatDau, gioKetThuc), 0);
    }
}
