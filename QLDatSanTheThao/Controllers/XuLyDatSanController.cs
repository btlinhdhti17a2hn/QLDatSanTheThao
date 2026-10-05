// Họ và tên: [Điền họ tên SV 4]
// Mã sinh viên: [Điền mã SV 4]
// Nội dung thực hiện: Controller XuLyDatSan - Tiếp nhận đặt sân, Xác nhận/Hủy, Kiểm tra trùng lịch,
// Quản lý luồng trạng thái (Module 4)

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDatSanTheThao.Data;
using QLDatSanTheThao.Models.Entities;
using QLDatSanTheThao.Models.ViewModels;
using QLDatSanTheThao.Services;

namespace QLDatSanTheThao.Controllers
{
    public class XuLyDatSanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public XuLyDatSanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Kiểm tra quyền Admin / Nhân viên tại Controller (không chỉ ẩn menu trên View).
        // Trả về null nếu hợp lệ, ngược lại trả về kết quả chuyển hướng.
        private IActionResult? KiemTraQuyenQuanLy()
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");

            if (string.IsNullOrEmpty(vaiTro))
            {
                TempData["Error"] = "Vui lòng đăng nhập để sử dụng chức năng này!";
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            if (vaiTro != "Admin" && vaiTro != "NhanVien")
            {
                TempData["Error"] = "Bạn không có quyền truy cập chức năng này!";
                return RedirectToAction("Index", "Home");
            }

            return null;
        }

        // Chuyển hướng sau khi xử lý: về trang người dùng đang đứng (giữ nguyên bộ lọc) nếu returnUrl hợp lệ
        private IActionResult QuayLai(string? returnUrl, int id)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction(nameof(Details), new { id });
        }

        // ====================================================================
        // 1. DANH SÁCH ĐẶT SÂN CẦN XỬ LÝ (TÌM KIẾM + LỌC + SẮP XẾP + PHÂN TRANG)
        // ====================================================================
        public async Task<IActionResult> Index(
            string? searchString,
            int? maLoaiSan,
            string? trangThai,
            DateTime? tuNgay,
            DateTime? denNgay,
            string? sortOrder,
            int pageIndex = 1)
        {
            var chan = KiemTraQuyenQuanLy();
            if (chan != null) return chan;

            const int pageSize = 10;
            string? canhBaoLoc = null;

            // Trạng thái lọc không hợp lệ thì bỏ qua
            if (!string.IsNullOrEmpty(trangThai) && !TrangThaiDatSan.TatCa.Contains(trangThai))
                trangThai = null;

            // Khoảng ngày sai thứ tự thì hoán đổi và cảnh báo
            if (tuNgay.HasValue && denNgay.HasValue && tuNgay.Value.Date > denNgay.Value.Date)
            {
                (tuNgay, denNgay) = (denNgay, tuNgay);
                canhBaoLoc = "Từ ngày lớn hơn Đến ngày nên hệ thống đã tự hoán đổi hai giá trị.";
            }

            // Truy vấn IQueryable - chỉ thực thi khi ToListAsync/CountAsync
            // SỬA: thêm Include ChiTietDatSans để tính tổng thanh toán (sân + dịch vụ)
            var query = _context.DatSans
                .AsNoTracking()
                .Include(d => d.KhachHang)
                .Include(d => d.SanTheThao)
                    .ThenInclude(s => s.LoaiSan)
                .Include(d => d.ChiTietDatSans)
                .AsQueryable();

            // --- Tìm kiếm: theo khách hàng (tên, SĐT) hoặc tên sân ---
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();
                query = query.Where(d =>
                    d.KhachHang.HoTen.Contains(searchString) ||
                    d.KhachHang.SoDienThoai.Contains(searchString) ||
                    d.SanTheThao.TenSan.Contains(searchString));
            }

            // --- Lọc: loại sân, trạng thái, khoảng ngày (theo ngày sử dụng sân) ---
            if (maLoaiSan.HasValue && maLoaiSan.Value > 0)
                query = query.Where(d => d.SanTheThao.MaLoaiSan == maLoaiSan.Value);

            if (!string.IsNullOrEmpty(trangThai))
                query = query.Where(d => d.TrangThai == trangThai);

            if (tuNgay.HasValue)
            {
                var tu = tuNgay.Value.Date;
                query = query.Where(d => d.GioBatDau >= tu);
            }

            if (denNgay.HasValue)
            {
                var den = denNgay.Value.Date.AddDays(1);
                query = query.Where(d => d.GioBatDau < den);
            }

            // --- Sắp xếp (thêm MaDatSan để thứ tự ổn định khi phân trang) ---
            query = sortOrder switch
            {
                "ngay_asc" => query.OrderBy(d => d.GioBatDau).ThenBy(d => d.MaDatSan),
                "khach_asc" => query.OrderBy(d => d.KhachHang.HoTen).ThenBy(d => d.MaDatSan),
                "khach_desc" => query.OrderByDescending(d => d.KhachHang.HoTen).ThenBy(d => d.MaDatSan),
                "san_asc" => query.OrderBy(d => d.SanTheThao.TenSan).ThenBy(d => d.MaDatSan),
                "san_desc" => query.OrderByDescending(d => d.SanTheThao.TenSan).ThenBy(d => d.MaDatSan),
                _ => query.OrderByDescending(d => d.GioBatDau).ThenBy(d => d.MaDatSan) // mặc định: mới nhất trước
            };

            // --- Phân trang trên truy vấn (Skip/Take), không tải toàn bộ dữ liệu ---
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (pageIndex < 1) pageIndex = 1;
            if (totalPages > 0 && pageIndex > totalPages) pageIndex = totalPages;

            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // --- Thống kê nhanh số đơn theo trạng thái (LINQ GroupBy) ---
            var thongKe = await _context.DatSans
                .GroupBy(d => d.TrangThai)
                .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
                .ToDictionaryAsync(x => x.TrangThai, x => x.SoLuong);

            var vm = new DatSanXuLyListViewModel
            {
                Items = items,
                LoaiSans = await _context.LoaiSans.AsNoTracking().OrderBy(l => l.TenLoai).ToListAsync(),
                TrangThais = TrangThaiDatSan.TatCa,
                SearchString = searchString,
                MaLoaiSan = maLoaiSan,
                TrangThai = trangThai,
                TuNgay = tuNgay,
                DenNgay = denNgay,
                SortOrder = sortOrder,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalItems = totalItems,
                ThongKeTrangThai = thongKe,
                CanhBaoLoc = canhBaoLoc
            };

            return View(vm);
        }

        // ====================================================================
        // 2. CHI TIẾT ĐẶT SÂN + CẢNH BÁO TRÙNG LỊCH / KHẢ NĂNG ĐÁP ỨNG
        // ====================================================================
        public async Task<IActionResult> Details(int? id)
        {
            var chan = KiemTraQuyenQuanLy();
            if (chan != null) return chan;
            if (id == null) return NotFound();

            var datSan = await _context.DatSans
                .AsNoTracking()
                .Include(d => d.KhachHang)
                .Include(d => d.SanTheThao)
                    .ThenInclude(s => s.LoaiSan)
                .Include(d => d.ChiTietDatSans)
                    .ThenInclude(c => c.DichVu)
                .FirstOrDefaultAsync(d => d.MaDatSan == id);

            if (datSan == null) return NotFound();

            var vm = new DatSanXuLyChiTietViewModel
            {
                DatSan = datSan,
                CoTheXacNhan = DatSanNghiepVu.CoTheXacNhan(datSan),
                CoTheHoanThanh = DatSanNghiepVu.CoTheHoanThanh(datSan),
                CoTheHuy = DatSanNghiepVu.CoTheHuy(datSan),
                LyDoKhongTheHuy = DatSanNghiepVu.LyDoKhongTheHuy(datSan)
            };

            // Lịch đã xác nhận/hoàn thành của cùng sân đang trùng khung giờ
            vm.LichDaXacNhanTrung = await DatSanNghiepVu
                .TimLichTrung(_context, datSan.MaSan, datSan.GioBatDau, datSan.GioKetThuc,
                    datSan.MaDatSan, TrangThaiDatSan.DangChiemLich)
                .AsNoTracking()
                .Include(x => x.KhachHang)
                .OrderBy(x => x.GioBatDau)
                .ToListAsync();

            // Số đơn khác đang chờ xử lý cũng trùng khung giờ này
            vm.SoDonChoTrung = await DatSanNghiepVu
                .TimLichTrung(_context, datSan.MaSan, datSan.GioBatDau, datSan.GioKetThuc,
                    datSan.MaDatSan, new[] { TrangThaiDatSan.ChoXuLy })
                .CountAsync();

            // Số lịch đã xác nhận/hoàn thành của sân trong ngày đặt
            var dauNgay = datSan.GioBatDau.Date;
            var cuoiNgay = dauNgay.AddDays(1);
            vm.SoLichTrongNgay = await _context.DatSans.CountAsync(x =>
                x.MaSan == datSan.MaSan &&
                x.MaDatSan != datSan.MaDatSan &&
                TrangThaiDatSan.DangChiemLich.Contains(x.TrangThai) &&
                x.GioBatDau >= dauNgay && x.GioBatDau < cuoiNgay);

            // Chỉ kiểm tra điều kiện xác nhận khi đơn đang chờ xử lý
            if (vm.CoTheXacNhan)
                vm.CanhBao = await DatSanNghiepVu.KiemTraDieuKienXacNhan(_context, datSan);

            return View(vm);
        }

        // ====================================================================
        // 3. XÁC NHẬN: Chờ xử lý -> Đang xử lý
        // ====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhan(int id, string? returnUrl)
        {
            var chan = KiemTraQuyenQuanLy();
            if (chan != null) return chan;

            var datSan = await _context.DatSans.FirstOrDefaultAsync(d => d.MaDatSan == id);
            if (datSan == null) return NotFound();

            // Chỉ trạng thái "Chờ xử lý" mới được xác nhận
            if (!DatSanNghiepVu.CoTheXacNhan(datSan))
            {
                TempData["Error"] = $"Không thể xác nhận: đơn #{id} đang ở trạng thái \"{datSan.TrangThai}\".";
                return QuayLai(returnUrl, id);
            }

            // Kiểm tra giờ mở cửa, giờ kết thúc, tiền cọc, trùng lịch... ngay trước khi lưu
            var loi = await DatSanNghiepVu.KiemTraDieuKienXacNhan(_context, datSan);
            if (loi.Any())
            {
                TempData["Error"] = $"Không thể xác nhận đơn #{id}: " + string.Join(" ", loi);
                return QuayLai(returnUrl, id);
            }

            datSan.TrangThai = TrangThaiDatSan.DangXuLy;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã xác nhận đơn #{id}. Đơn chuyển sang trạng thái \"Đang xử lý\".";
            return QuayLai(returnUrl, id);
        }

        // ====================================================================
        // 4. HOÀN THÀNH: Đang xử lý -> Hoàn thành
        // ====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HoanThanh(int id, string? returnUrl)
        {
            var chan = KiemTraQuyenQuanLy();
            if (chan != null) return chan;

            var datSan = await _context.DatSans.FirstOrDefaultAsync(d => d.MaDatSan == id);
            if (datSan == null) return NotFound();

            // Phải qua bước xác nhận (Đang xử lý) mới được hoàn thành, không nhảy cóc trạng thái
            if (!DatSanNghiepVu.CoTheHoanThanh(datSan))
            {
                TempData["Error"] = $"Không thể hoàn thành: đơn #{id} đang ở trạng thái \"{datSan.TrangThai}\" (chỉ đơn \"Đang xử lý\" mới được hoàn thành).";
                return QuayLai(returnUrl, id);
            }

            datSan.TrangThai = TrangThaiDatSan.HoanThanh;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã hoàn thành đơn #{id}.";
            return QuayLai(returnUrl, id);
        }

        // ====================================================================
        // 5. HỦY: Chờ xử lý / Đang xử lý (chưa tới giờ) -> Đã hủy
        //    Không xóa dữ liệu, chỉ đổi trạng thái để giữ lịch sử.
        // ====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(int id, string? returnUrl)
        {
            var chan = KiemTraQuyenQuanLy();
            if (chan != null) return chan;

            var datSan = await _context.DatSans.FirstOrDefaultAsync(d => d.MaDatSan == id);
            if (datSan == null) return NotFound();

            var lyDo = DatSanNghiepVu.LyDoKhongTheHuy(datSan);
            if (lyDo != null)
            {
                TempData["Error"] = $"Không thể hủy đơn #{id}: {lyDo}";
                return QuayLai(returnUrl, id);
            }

            datSan.TrangThai = TrangThaiDatSan.DaHuy;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã hủy đơn #{id}.";
            return QuayLai(returnUrl, id);
        }
    }
}