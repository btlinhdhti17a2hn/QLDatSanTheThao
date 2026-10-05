// Họ và tên: [Điền họ tên SV 4]
// Mã sinh viên: [Điền mã SV 4]
// Nội dung thực hiện: Lớp xử lý nghiệp vụ đặt sân - Luồng trạng thái, kiểm tra trùng lịch,
// kiểm tra điều kiện trước khi xác nhận (Module 4)

using Microsoft.EntityFrameworkCore;
using QLDatSanTheThao.Data;
using QLDatSanTheThao.Models.Entities;

namespace QLDatSanTheThao.Services
{
    // Các trạng thái hợp lệ của một lượt đặt sân
    public static class TrangThaiDatSan
    {
        public const string ChoXuLy = "Chờ xử lý";
        public const string DangXuLy = "Đang xử lý";
        public const string HoanThanh = "Hoàn thành";
        public const string DaHuy = "Đã hủy";

        public static readonly string[] TatCa = { ChoXuLy, DangXuLy, HoanThanh, DaHuy };

        // Các trạng thái đang "chiếm" lịch của sân (đã được nhân viên xác nhận)
        public static readonly string[] DangChiemLich = { DangXuLy, HoanThanh };
    }

    public static class DatSanNghiepVu
    {
        // LUỒNG TRẠNG THÁI:
        //   Chờ xử lý --(Xác nhận)--> Đang xử lý --(Hoàn thành)--> Hoàn thành
        //   Chờ xử lý / Đang xử lý --(Hủy)--> Đã hủy
        // Hoàn thành và Đã hủy là trạng thái cuối, không được chuyển tiếp.
        public static bool CoTheXacNhan(DatSan d) => d.TrangThai == TrangThaiDatSan.ChoXuLy;

        public static bool CoTheHoanThanh(DatSan d) => d.TrangThai == TrangThaiDatSan.DangXuLy;

        public static bool CoTheHuy(DatSan d) => LyDoKhongTheHuy(d) == null;

        // Trả về null nếu được phép hủy, ngược lại trả về lý do không được hủy
        public static string? LyDoKhongTheHuy(DatSan d)
        {
            switch (d.TrangThai)
            {
                case TrangThaiDatSan.ChoXuLy:
                    return null;
                case TrangThaiDatSan.DangXuLy:
                    // Đã xác nhận thì chỉ được hủy khi chưa đến giờ bắt đầu
                    return d.GioBatDau > DateTime.Now
                        ? null
                        : "Lịch đặt đã đến/qua giờ bắt đầu nên không thể hủy.";
                case TrangThaiDatSan.HoanThanh:
                    return "Lượt đặt đã hoàn thành, không được hủy.";
                case TrangThaiDatSan.DaHuy:
                    return "Lượt đặt đã được hủy trước đó.";
                default:
                    return "Trạng thái không hợp lệ.";
            }
        }

        // KIỂM TRA TRÙNG LỊCH (LINQ)
        // Hai khoảng thời gian trùng nhau khi: bắt đầu A < kết thúc B và kết thúc A > bắt đầu B
        public static IQueryable<DatSan> TimLichTrung(
            ApplicationDbContext context,
            int maSan,
            DateTime batDau,
            DateTime ketThuc,
            int boQuaMaDatSan,
            string[] cacTrangThai)
        {
            return context.DatSans.Where(x =>
                x.MaSan == maSan &&
                x.MaDatSan != boQuaMaDatSan &&
                cacTrangThai.Contains(x.TrangThai) &&
                x.GioBatDau < ketThuc &&
                x.GioKetThuc > batDau);
        }

        // KIỂM TRA ĐIỀU KIỆN TRƯỚC KHI XÁC NHẬN
        // Trả về danh sách lỗi; danh sách rỗng nghĩa là đủ điều kiện xác nhận.
        public static async Task<List<string>> KiemTraDieuKienXacNhan(ApplicationDbContext context, DatSan d)
        {
            var loi = new List<string>();

            // 1. Khách hàng tồn tại và đang hoạt động
            var khachHang = await context.KhachHangs
                .Include(k => k.TaiKhoan)
                .FirstOrDefaultAsync(k => k.MaKhachHang == d.MaKhachHang);
            if (khachHang == null)
            {
                loi.Add("Khách hàng không tồn tại.");
            }
            else
            {
                if (!khachHang.TrangThai)
                    loi.Add("Khách hàng đang bị khóa/ngừng hoạt động.");
                if (khachHang.TaiKhoan != null && !khachHang.TaiKhoan.TrangThai)
                    loi.Add("Tài khoản của khách hàng đang bị khóa.");
            }

            // 2. Sân tồn tại, đang hoạt động và không bảo trì vào ngày đặt
            var san = await context.SanTheThaos.FirstOrDefaultAsync(s => s.MaSan == d.MaSan);
            if (san == null)
            {
                loi.Add("Sân thể thao không tồn tại.");
            }
            else
            {
                if (!san.TrangThai)
                    loi.Add("Sân hiện không khả dụng (ngừng hoạt động).");

                if (san.NgayBaoTri.HasValue && san.NgayBaoTri.Value.Date == d.GioBatDau.Date)
                    loi.Add($"Sân đang bảo trì vào ngày {d.GioBatDau:dd/MM/yyyy}.");

                // 3. Chỉ được đặt trong giờ mở cửa
                if (d.GioBatDau.Date != d.GioKetThuc.Date)
                {
                    loi.Add("Lịch đặt phải bắt đầu và kết thúc trong cùng một ngày.");
                }
                else if (d.GioBatDau.TimeOfDay < san.GioMoCua || d.GioKetThuc.TimeOfDay > san.GioDongCua)
                {
                    loi.Add($"Lịch đặt nằm ngoài giờ mở cửa của sân ({san.GioMoCua:hh\\:mm} - {san.GioDongCua:hh\\:mm}).");
                }
            }

            // 4. Giờ kết thúc phải sau giờ bắt đầu
            if (d.GioKetThuc <= d.GioBatDau)
                loi.Add("Giờ kết thúc phải sau giờ bắt đầu.");

            // 5. Tiền cọc không vượt tổng tiền dự kiến
            if (d.TienCoc < 0 || d.DonGia < 0)
                loi.Add("Đơn giá hoặc tiền cọc không hợp lệ (phải >= 0).");
            else if (d.TienCoc > d.DonGia)
                loi.Add($"Tiền cọc ({d.TienCoc:N0} VNĐ) vượt quá tổng tiền dự kiến ({d.DonGia:N0} VNĐ).");

            // 6. Không trùng lịch với các lượt đặt đã được xác nhận của cùng sân
            if (d.GioKetThuc > d.GioBatDau)
            {
                var lichTrung = await TimLichTrung(context, d.MaSan, d.GioBatDau, d.GioKetThuc,
                        d.MaDatSan, TrangThaiDatSan.DangChiemLich)
                    .OrderBy(x => x.GioBatDau)
                    .ToListAsync();

                foreach (var t in lichTrung)
                {
                    loi.Add($"Trùng lịch với đơn #{t.MaDatSan} ({t.GioBatDau:HH:mm} - {t.GioKetThuc:HH:mm}, trạng thái: {t.TrangThai}).");
                }
            }

            return loi;
        }
    }
}