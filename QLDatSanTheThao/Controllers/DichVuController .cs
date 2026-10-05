// Họ và tên: Tạ Đình Kim 
// Mã sinh viên: 23103100315
// Nội dung thực hiện: Controller DichVu - CRUD dịch vụ (nước uống, thuê vợt, bóng...),
// tìm kiếm/sắp xếp/phân trang, thêm dịch vụ vào đơn đặt sân và tự động tính tổng tiền.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLDatSanTheThao.Data;
using QLDatSanTheThao.Models.Entities;
using QLDatSanTheThao.Models.ViewModels;
using QLDatSanTheThao.Services;

namespace QLDatSanTheThao.Controllers
{
    public class DichVuController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Các giá trị trạng thái - phải thống nhất với các thành viên khác trong nhóm
        private const string DON_DANG_XU_LY = "Đang xử lý";
        private const int KICH_THUOC_TRANG = 8;

        public DichVuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ================== PHÂN QUYỀN (kiểm tra tại Controller) ==================
        private string? VaiTro => HttpContext.Session.GetString("VaiTro");
        private bool LaAdmin() => VaiTro == "Admin";
        private bool LaQuanLy() => VaiTro == "Admin" || VaiTro == "NhanVien";

        private IActionResult TuChoi()
        {
            if (string.IsNullOrEmpty(VaiTro))
                return RedirectToAction("DangNhap", "TaiKhoan");   // chưa đăng nhập
            TempData["Loi"] = "Bạn không có quyền thực hiện chức năng này.";
            return RedirectToAction("Index", "Home");
        }

        // ================== DANH SÁCH: tìm kiếm + lọc + sắp xếp + phân trang ==================
        public async Task<IActionResult> Index(string? tuKhoa, bool? trangThai, string? sapXep, int trang = 1)
        {
            if (!LaQuanLy()) return TuChoi();

            var query = _context.DichVus.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
                query = query.Where(d => d.TenDichVu.Contains(tuKhoa.Trim()));
            if (trangThai.HasValue)   // true = đang hoạt động, false = ngừng
                query = query.Where(d => d.TrangThai == trangThai.Value);

            query = sapXep switch
            {
                "ten_giam" => query.OrderByDescending(d => d.TenDichVu),
                "gia_tang" => query.OrderBy(d => d.DonGia),
                "gia_giam" => query.OrderByDescending(d => d.DonGia),
                _ => query.OrderBy(d => d.TenDichVu)
            };

            int tongSo = await query.CountAsync();
            int tongTrang = Math.Max((int)Math.Ceiling(tongSo / (double)KICH_THUOC_TRANG), 1);
            trang = Math.Clamp(trang, 1, tongTrang);

            var ds = await query.Skip((trang - 1) * KICH_THUOC_TRANG)
                                .Take(KICH_THUOC_TRANG)
                                .ToListAsync();

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.TrangThai = trangThai;
            ViewBag.SapXep = sapXep;
            ViewBag.Trang = trang;
            ViewBag.TongTrang = tongTrang;
            ViewBag.TongSo = tongSo;
            ViewBag.LaAdmin = LaAdmin();
            return View(ds);
        }

        // ================== CHI TIẾT ==================
        public async Task<IActionResult> Details(int? id)
        {
            if (!LaQuanLy()) return TuChoi();
            if (id == null) return NotFound();

            var dichVu = await _context.DichVus.AsNoTracking().FirstOrDefaultAsync(d => d.MaDichVu == id);
            if (dichVu == null) return NotFound();

            // Tổng số lượng đã được sử dụng trong các đơn đặt sân
            ViewBag.DaSuDung = await _context.ChiTietDatSans
                .Where(c => c.MaDichVu == id).SumAsync(c => (int?)c.SoLuong) ?? 0;
            return View(dichVu);
        }

        // ================== THÊM (Admin) ==================
        public IActionResult Create()
        {
            if (!LaAdmin()) return TuChoi();
            return View(new DichVu { TrangThai = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenDichVu,MoTa,DonGia,TrangThai")] DichVu dichVu)
        {
            if (!LaAdmin()) return TuChoi();

            dichVu.TenDichVu = (dichVu.TenDichVu ?? "").Trim();

            // Nghiệp vụ: tên dịch vụ không được trùng
            if (await _context.DichVus.AnyAsync(d => d.TenDichVu == dichVu.TenDichVu))
                ModelState.AddModelError("TenDichVu", "Tên dịch vụ đã tồn tại.");

            if (!ModelState.IsValid) return View(dichVu);

            _context.Add(dichVu);
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = "Thêm dịch vụ thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ================== SỬA (Admin) ==================
        public async Task<IActionResult> Edit(int? id)
        {
            if (!LaAdmin()) return TuChoi();
            if (id == null) return NotFound();

            var dichVu = await _context.DichVus.FindAsync(id);
            if (dichVu == null) return NotFound();
            return View(dichVu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDichVu,TenDichVu,MoTa,DonGia,TrangThai")] DichVu model)
        {
            if (!LaAdmin()) return TuChoi();
            if (id != model.MaDichVu) return NotFound();

            model.TenDichVu = (model.TenDichVu ?? "").Trim();

            if (await _context.DichVus.AnyAsync(d => d.TenDichVu == model.TenDichVu && d.MaDichVu != id))
                ModelState.AddModelError("TenDichVu", "Tên dịch vụ đã tồn tại.");

            if (!ModelState.IsValid) return View(model);

            var dichVu = await _context.DichVus.FindAsync(id);
            if (dichVu == null) return NotFound();

            // Chỉ cập nhật các trường cho phép (tránh overposting)
            dichVu.TenDichVu = model.TenDichVu;
            dichVu.DonGia = model.DonGia;
            dichVu.MoTa = model.MoTa;
            dichVu.TrangThai = model.TrangThai;

            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = "Cập nhật dịch vụ thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ================== ĐỔI TRẠNG THÁI (Admin) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiTrangThai(int id)
        {
            if (!LaAdmin()) return TuChoi();
            var dichVu = await _context.DichVus.FindAsync(id);
            if (dichVu == null) return NotFound();

            dichVu.TrangThai = !dichVu.TrangThai;
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = $"Đã chuyển \"{dichVu.TenDichVu}\" sang trạng thái {(dichVu.TrangThai ? "Hoạt động" : "Ngừng cung cấp")}.";
            return RedirectToAction(nameof(Index));
        }

        // ================== XÓA (Admin) ==================
        public async Task<IActionResult> Delete(int? id)
        {
            if (!LaAdmin()) return TuChoi();
            if (id == null) return NotFound();

            var dichVu = await _context.DichVus.AsNoTracking().FirstOrDefaultAsync(d => d.MaDichVu == id);
            if (dichVu == null) return NotFound();

            ViewBag.DaSuDung = await _context.ChiTietDatSans.AnyAsync(c => c.MaDichVu == id);
            return View(dichVu);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!LaAdmin()) return TuChoi();

            var dichVu = await _context.DichVus.FindAsync(id);
            if (dichVu == null) return NotFound();

            // Không xóa dịch vụ đã phát sinh lịch sử sử dụng (giữ toàn vẹn dữ liệu)
            if (await _context.ChiTietDatSans.AnyAsync(c => c.MaDichVu == id))
            {
                TempData["Loi"] = "Dịch vụ đã được sử dụng trong đơn đặt sân, không thể xóa. Hãy chuyển sang trạng thái ngừng cung cấp.";
                return RedirectToAction(nameof(Index));
            }

            _context.DichVus.Remove(dichVu);
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = "Đã xóa dịch vụ.";
            return RedirectToAction(nameof(Index));
        }

        // ================== DANH SÁCH ĐƠN ĐANG XỬ LÝ (để chọn đơn thêm dịch vụ) ==================
        public async Task<IActionResult> DonDatSan(string? tuKhoa)
        {
            if (!LaQuanLy()) return TuChoi();

            var query = _context.DatSans.AsNoTracking()
                .Include(d => d.KhachHang)
                .Include(d => d.SanTheThao)
                .Where(d => d.TrangThai == DON_DANG_XU_LY);

            if (!string.IsNullOrWhiteSpace(tuKhoa))
                query = query.Where(d => d.KhachHang!.HoTen.Contains(tuKhoa.Trim())
                                      || d.SanTheThao!.TenSan.Contains(tuKhoa.Trim()));

            var ds = await query.OrderByDescending(d => d.NgayDat)
                                .ThenBy(d => d.GioBatDau)
                                .ToListAsync();
            ViewBag.TuKhoa = tuKhoa;
            return View(ds);
        }

        // ================== CHI TIẾT ĐƠN + DỊCH VỤ ĐÃ DÙNG + TỔNG TIỀN ==================
        public async Task<IActionResult> ChiTietDon(int maDatSan)
        {
            if (!LaQuanLy()) return TuChoi();

            var vm = await TaoViewModelAsync(maDatSan);
            if (vm == null) return NotFound();

            await NapDanhSachDichVuAsync();
            return View(vm);
        }

        // ================== THÊM DỊCH VỤ VÀO ĐƠN (tự cập nhật tổng tiền) ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemVaoDon(int maDatSan, int maDichVu, int soLuong, string? ghiChu)
        {
            if (!LaQuanLy()) return TuChoi();

            var datSan = await _context.DatSans.FirstOrDefaultAsync(d => d.MaDatSan == maDatSan);
            if (datSan == null) return NotFound();

            // 1. Đơn phải đang ở trạng thái cho phép
            if (datSan.TrangThai != DON_DANG_XU_LY)
            {
                TempData["Loi"] = "Chỉ được thêm dịch vụ khi đơn đặt sân ở trạng thái \"Đang xử lý\".";
                return RedirectToAction(nameof(ChiTietDon), new { maDatSan });
            }

            // 2. Số lượng hợp lệ (> 0)
            if (soLuong <= 0 || soLuong > 100)
            {
                TempData["Loi"] = "Số lượng phải từ 1 đến 100.";
                return RedirectToAction(nameof(ChiTietDon), new { maDatSan });
            }

            // 3. Dịch vụ phải tồn tại và đang hoạt động
            var dichVu = await _context.DichVus.FirstOrDefaultAsync(d => d.MaDichVu == maDichVu);
            if (dichVu == null || !dichVu.TrangThai)
            {
                TempData["Loi"] = "Dịch vụ không tồn tại hoặc đã ngừng cung cấp.";
                return RedirectToAction(nameof(ChiTietDon), new { maDatSan });
            }

            // 4. Nếu dịch vụ đã có trong đơn -> cộng dồn số lượng, ngược lại thêm dòng mới
            var chiTiet = await _context.ChiTietDatSans
                .FirstOrDefaultAsync(c => c.MaDatSan == maDatSan && c.MaDichVu == maDichVu);

            if (chiTiet != null)
            {
                if (chiTiet.SoLuong + soLuong > 100)
                {
                    TempData["Loi"] = "Tổng số lượng của một dịch vụ trong đơn không được vượt quá 100.";
                    return RedirectToAction(nameof(ChiTietDon), new { maDatSan });
                }
                chiTiet.SoLuong += soLuong;
                chiTiet.ThanhTien = chiTiet.SoLuong * chiTiet.DonGia;   // ThanhTien = SoLuong * DonGia
            }
            else
            {
                _context.ChiTietDatSans.Add(new ChiTietDatSan
                {
                    MaDatSan = maDatSan,
                    MaDichVu = maDichVu,
                    SoLuong = soLuong,
                    DonGia = dichVu.DonGia,                 // lưu lại giá tại thời điểm dùng
                    ThanhTien = soLuong * dichVu.DonGia,
                    GhiChu = ghiChu,
                    TrangThai = "Chưa thanh toán"
                });
            }

            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = $"Đã thêm {soLuong} x {dichVu.TenDichVu} vào đơn.";
            return RedirectToAction(nameof(ChiTietDon), new { maDatSan });
        }

        // ================== XÓA DỊCH VỤ KHỎI ĐƠN ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaKhoiDon(int maChiTiet)
        {
            if (!LaQuanLy()) return TuChoi();

            var chiTiet = await _context.ChiTietDatSans
                .Include(c => c.DatSan)
                .FirstOrDefaultAsync(c => c.MaChiTiet == maChiTiet);
            if (chiTiet == null) return NotFound();

            int maDatSan = chiTiet.MaDatSan;
            if (chiTiet.DatSan?.TrangThai != DON_DANG_XU_LY)
            {
                TempData["Loi"] = "Đơn đã chốt, không thể xóa dịch vụ.";
                return RedirectToAction(nameof(ChiTietDon), new { maDatSan });
            }

            _context.ChiTietDatSans.Remove(chiTiet);
            await _context.SaveChangesAsync();
            TempData["ThanhCong"] = "Đã xóa dịch vụ khỏi đơn.";
            return RedirectToAction(nameof(ChiTietDon), new { maDatSan });
        }

        // ================== HÀM HỖ TRỢ ==================
        private async Task<ChiTietDonViewModel?> TaoViewModelAsync(int maDatSan)
        {
            var datSan = await _context.DatSans.AsNoTracking()
                .Include(d => d.KhachHang)
                .Include(d => d.SanTheThao)
                .FirstOrDefaultAsync(d => d.MaDatSan == maDatSan);
            if (datSan == null) return null;

            var chiTiets = await _context.ChiTietDatSans.AsNoTracking()
                .Include(c => c.DichVu)
                .Where(c => c.MaDatSan == maDatSan)
                .OrderBy(c => c.MaChiTiet)
                .ToListAsync();

            return new ChiTietDonViewModel
            {
                DatSan = datSan,
                ChiTiets = chiTiets,
                SoGio = TinhTienHelper.SoGio(datSan.GioBatDau, datSan.GioKetThuc),
                TienSan = TinhTienHelper.TienSan(datSan.DonGia, datSan.GioBatDau, datSan.GioKetThuc),
                TienDichVu = chiTiets.Sum(c => c.ThanhTien),
                ChoPhepThem = datSan.TrangThai == DON_DANG_XU_LY
            };
        }

        // Select khóa ngoại lấy từ Database: chỉ dịch vụ đang hoạt động
        private async Task NapDanhSachDichVuAsync()
        {
            var ds = await _context.DichVus.AsNoTracking()
                .Where(d => d.TrangThai)
                .OrderBy(d => d.TenDichVu)
                .Select(d => new SelectListItem
                {
                    Value = d.MaDichVu.ToString(),
                    Text = d.TenDichVu + " - " + d.DonGia.ToString("N0") + "đ"
                })
                .ToListAsync();
            ViewBag.DichVuList = ds;
        }
    }
}
