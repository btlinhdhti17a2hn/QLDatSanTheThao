// Họ và tên: [Điền tên SV 3]
// Mã sinh viên: [Điền mã SV 3]
// Nội dung thực hiện: Controller KhachHang - Quản lý hồ sơ cá nhân, Đặt sân và Theo dõi lịch đặt (Module 3)

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDatSanTheThao.Data;
using QLDatSanTheThao.Models.Entities;
using QLDatSanTheThao.Models.ViewModels;

namespace QLDatSanTheThao.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly ApplicationDbContext _context;

        public KhachHangController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper: Lấy Mã tài khoản đang đăng nhập từ Session
        private int? GetMaTaiKhoanHienTai()
        {
            return HttpContext.Session.GetInt32("MaTaiKhoan");
        }

        // Helper: Lấy thông tin Khách hàng tương ứng với Mã tài khoản đang đăng nhập
        private async Task<KhachHang?> GetKhachHangHienTaiAsync()
        {
            int? maTaiKhoan = GetMaTaiKhoanHienTai();
            if (maTaiKhoan == null) return null;

            return await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan.Value);
        }

        // ====================================================================
        // 1. QUẢN LÝ HỒ SƠ CÁ NHÂN KHÁCH HÀNG
        // ====================================================================
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            int? maTaiKhoan = GetMaTaiKhoanHienTai();
            if (maTaiKhoan == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem thông tin cá nhân!";
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var khachHang = await GetKhachHangHienTaiAsync();
            if (khachHang == null)
            {
                // Nếu tài khoản mới chưa có bản ghi Khách hàng, tự động khởi tạo
                var taiKhoan = await _context.TaiKhoans.FindAsync(maTaiKhoan.Value);
                khachHang = new KhachHang
                {
                    MaTaiKhoan = maTaiKhoan.Value,
                    HoTen = taiKhoan?.HoTen ?? "Khách hàng",
                    Email = taiKhoan?.Email ?? "",
                    SoDienThoai = "",
                    NgayDangKy = DateTime.Now,
                    DiemTichLuy = 0,
                    TrangThai = true
                };
                _context.KhachHangs.Add(khachHang);
                await _context.SaveChangesAsync();
            }

            return View(khachHang);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(KhachHang model)
        {
            int? maTaiKhoan = GetMaTaiKhoanHienTai();
            if (maTaiKhoan == null) return RedirectToAction("DangNhap", "TaiKhoan");

            var khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.MaKhachHang == model.MaKhachHang && k.MaTaiKhoan == maTaiKhoan.Value);
            if (khachHang == null) return NotFound();

            if (ModelState.IsValid)
            {
                khachHang.HoTen = model.HoTen;
                khachHang.SoDienThoai = model.SoDienThoai;
                khachHang.Email = model.Email;
                khachHang.DiaChi = model.DiaChi;
                khachHang.NgaySinh = model.NgaySinh;
                khachHang.GioiTinh = model.GioiTinh;

                _context.Update(khachHang);
                await _context.SaveChangesAsync();
                
                // Cập nhật lại Họ tên hiển thị trên Session
                HttpContext.Session.SetString("HoTen", khachHang.HoTen);

                TempData["Success"] = "Cập nhật hồ sơ cá nhân thành công!";
                return RedirectToAction(nameof(Profile));
            }

            return View(model);
        }

        // ====================================================================
        // 2. TẠO ĐẶT SÂN (GET: HÀM HIỂN THỊ FORM DÀNH CHO NÚT 'ĐẶT SÂN NGAY')
        // ====================================================================
        [HttpGet]
        public async Task<IActionResult> TaoDatSan(int maSan)
        {
            int? maTaiKhoan = GetMaTaiKhoanHienTai();
            if (maTaiKhoan == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập tài khoản trước khi thực hiện đặt sân!";
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var san = await _context.SanTheThaos
                .Include(s => s.LoaiSan)
                .FirstOrDefaultAsync(s => s.MaSan == maSan && s.TrangThai == true);

            if (san == null)
            {
                TempData["Error"] = "Sân thể thao không tồn tại hoặc hiện đang tạm đóng cửa!";
                return RedirectToAction("Index", "SanTheThao");
            }

            var dichVus = await _context.DichVus.Where(d => d.TrangThai == true).ToListAsync();

            var viewModel = new TaoDatSanViewModel
            {
                MaSan = san.MaSan,
                TenSan = san.TenSan,
                TenLoaiSan = san.LoaiSan.TenLoai,
                DonGiaTheoGio = san.DonGia,
                GioMoCua = san.GioMoCua,
                GioDongCua = san.GioDongCua,
                DiaChi = san.DiaChi,
                NgaySuDung = DateTime.Today.AddDays(1),
                GioBatDau = new TimeSpan(17, 0, 0),
                GioKetThuc = new TimeSpan(19, 0, 0),
                TienCoc = 100000,
                DanhSachDichVu = dichVus.Select(d => new DichVuDatSanItem
                {
                    MaDichVu = d.MaDichVu,
                    TenDichVu = d.TenDichVu,
                    DonGia = d.DonGia,
                    SoLuong = 0
                }).ToList()
            };

            return View(viewModel);
        }

        // TẠO ĐẶT SÂN (POST: XỬ LÝ KIỂM TRA ĐIỀU KIỆN & LƯU ĐƠN)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoDatSan(TaoDatSanViewModel model)
        {
            int? maTaiKhoan = GetMaTaiKhoanHienTai();
            if (maTaiKhoan == null) return RedirectToAction("DangNhap", "TaiKhoan");

            var san = await _context.SanTheThaos.FindAsync(model.MaSan);
            if (san == null || !san.TrangThai)
            {
                TempData["Error"] = "Sân thể thao không khả dụng!";
                return RedirectToAction("Index", "SanTheThao");
            }

            // Gán lại dữ liệu tĩnh cho ViewModel nếu validate lỗi
            model.TenSan = san.TenSan;
            model.DonGiaTheoGio = san.DonGia;
            model.GioMoCua = san.GioMoCua;
            model.GioDongCua = san.GioDongCua;
            model.DiaChi = san.DiaChi;

            // 1. Kiểm tra Giờ kết thúc phải sau Giờ bắt đầu
            if (model.GioKetThuc <= model.GioBatDau)
            {
                ModelState.AddModelError("GioKetThuc", "Giờ kết thúc phải sau giờ bắt đầu!");
            }

            // 2. Kiểm tra giờ chọn phải nằm trong khung giờ hoạt động của sân
            if (model.GioBatDau < san.GioMoCua || model.GioKetThuc > san.GioDongCua)
            {
                ModelState.AddModelError("GioBatDau", $"Sân chỉ mở cửa từ {san.GioMoCua:hh\\:mm} đến {san.GioDongCua:hh\\:mm}!");
            }

            // Tính thời điểm bắt đầu và kết thúc dạng DateTime
            DateTime dtBatDau = model.NgaySuDung.Date.Add(model.GioBatDau);
            DateTime dtKetThuc = model.NgaySuDung.Date.Add(model.GioKetThuc);

            // 3. Kiểm tra thời gian chọn không được ở quá khứ
            if (dtBatDau < DateTime.Now)
            {
                ModelState.AddModelError("NgaySuDung", "Thời gian đặt sân không được nằm trong quá khứ!");
            }

            // 4. KIỂM TRA TRÙNG LỊCH ĐẶT SÂN BẰNG LINQ (Yêu cầu bắt buộc)
            bool bịTrùngLịch = await _context.DatSans
                .AnyAsync(d => d.MaSan == model.MaSan &&
                               d.TrangThai != "Đã hủy" &&
                               d.GioBatDau < dtKetThuc &&
                               d.GioKetThuc > dtBatDau);

            if (bịTrùngLịch)
            {
                ModelState.AddModelError("GioBatDau", "Sân đã có người đặt trong khung giờ này! Vui lòng chọn khung giờ khác.");
            }

            // 5. Kiểm tra Tiền cọc không vượt quá tổng tiền dự kiến
            decimal tongTien = model.TongTienDuKien;
            if (model.TienCoc > tongTien)
            {
                ModelState.AddModelError("TienCoc", "Tiền cọc không được lớn hơn tổng tiền dự kiến!");
            }

            if (ModelState.IsValid)
            {
                // Lấy thông tin Khách hàng (nếu chưa có thì tự khởi tạo)
                var khachHang = await GetKhachHangHienTaiAsync();
                if (khachHang == null)
                {
                    var taiKhoan = await _context.TaiKhoans.FindAsync(maTaiKhoan.Value);
                    khachHang = new KhachHang
                    {
                        MaTaiKhoan = maTaiKhoan.Value,
                        HoTen = taiKhoan?.HoTen ?? "Khách hàng",
                        Email = taiKhoan?.Email ?? "",
                        SoDienThoai = "0987654321",
                        NgayDangKy = DateTime.Now,
                        TrangThai = true
                    };
                    _context.KhachHangs.Add(khachHang);
                    await _context.SaveChangesAsync();
                }

                // Tạo bản ghi Đặt Sân mới
                var datSanMoi = new DatSan
                {
                    MaKhachHang = khachHang.MaKhachHang,
                    MaSan = model.MaSan,
                    NgayDat = DateTime.Now,
                    GioBatDau = dtBatDau,
                    GioKetThuc = dtKetThuc,
                    DonGia = tongTien,
                    TienCoc = model.TienCoc,
                    TrangThai = "Chờ xử lý"
                };

                _context.DatSans.Add(datSanMoi);
                await _context.SaveChangesAsync();

                // Lưu danh sách Chi Tiết Dịch Vụ Đặt Sân (nếu có chọn)
                if (model.DanhSachDichVu != null && model.DanhSachDichVu.Any())
                {
                    foreach (var item in model.DanhSachDichVu.Where(d => d.SoLuong > 0))
                    {
                        var chiTiet = new ChiTietDatSan
                        {
                            MaDatSan = datSanMoi.MaDatSan,
                            MaDichVu = item.MaDichVu,
                            SoLuong = item.SoLuong,
                            DonGia = item.DonGia,
                            ThanhTien = item.DonGia * item.SoLuong,
                            GhiChu = "Đăng ký khi đặt sân",
                            TrangThai = "Chờ xử lý"
                        };
                        _context.ChiTietDatSans.Add(chiTiet);
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Đặt sân thành công! Đơn đặt sân của bạn đang ở trạng thái 'Chờ xử lý'.";
                return RedirectToAction(nameof(LichSuDatSan));
            }

            return View(model);
        }

        // ====================================================================
        // 3. THEO DÕI LỊCH SỬ ĐẶT SÂN CỦA KHÁCH HÀNG
        // ====================================================================
        public async Task<IActionResult> LichSuDatSan()
        {
            int? maTaiKhoan = GetMaTaiKhoanHienTai();
            if (maTaiKhoan == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem lịch sử đặt sân!";
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            var khachHang = await GetKhachHangHienTaiAsync();
            if (khachHang == null)
            {
                return View(new List<DatSan>());
            }

            // BẢO BẢO QUYỀN DỮ LIỆU: Chỉ lấy danh sách đơn đặt sân của BẢN THÂN khách hàng đó
            var dsDatSan = await _context.DatSans
                .Include(d => d.SanTheThao)
                    .ThenInclude(s => s.LoaiSan)
                .Where(d => d.MaKhachHang == khachHang.MaKhachHang)
                .OrderByDescending(d => d.MaDatSan)
                .ToListAsync();

            return View(dsDatSan);
        }

        // ====================================================================
        // 4. HỦY ĐƠN ĐẶT SÂN (CHỈ CHO HỦY KHI ĐƠN Ở TRẠNG THÁI 'CHỜ XỬ LÝ')
        // ====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyDatSan(int id)
        {
            int? maTaiKhoan = GetMaTaiKhoanHienTai();
            if (maTaiKhoan == null) return RedirectToAction("DangNhap", "TaiKhoan");

            var khachHang = await GetKhachHangHienTaiAsync();
            if (khachHang == null) return NotFound();

            // Kiểm tra đúng đơn của mình (tránh thay đổi ID trên URL)
            var datSan = await _context.DatSans.FirstOrDefaultAsync(d => d.MaDatSan == id && d.MaKhachHang == khachHang.MaKhachHang);
            if (datSan == null)
            {
                TempData["Error"] = "Không tìm thấy đơn đặt sân hoặc bạn không có quyền thao tác đơn này!";
                return RedirectToAction(nameof(LichSuDatSan));
            }

            // Quy định: Chỉ được hủy đơn ở trạng thái "Chờ xử lý"
            if (datSan.TrangThai != "Chờ xử lý")
            {
                TempData["Error"] = "Đơn đặt sân này đã được nhân viên tiếp nhận hoặc đã hoàn thành, không thể tự hủy!";
                return RedirectToAction(nameof(LichSuDatSan));
            }

            datSan.TrangThai = "Đã hủy";
            _context.Update(datSan);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Hủy đơn đặt sân thành công!";
            return RedirectToAction(nameof(LichSuDatSan));
        }
    }
}
