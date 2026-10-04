// Họ và tên: [Điền tên SV 2]
// Mã sinh viên: [Điền mã SV 2]
// Nội dung thực hiện: Controller SanTheThao - Quản lý sân thể thao, Tìm kiếm, Lọc, Sắp xếp và Phân trang (Module 2)

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QLDatSanTheThao.Data;
using QLDatSanTheThao.Models.Entities;
using QLDatSanTheThao.Models.ViewModels;

namespace QLDatSanTheThao.Controllers
{
    public class SanTheThaoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SanTheThaoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Helper: Kiểm tra quyền Admin / Nhân viên
        private bool KiemTraQuyenQuanLy()
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            return vaiTro == "Admin" || vaiTro == "NhanVien";
        }

        // ====================================================================
        // 1. TRANG DANH SÁCH KHÁCH HÀNG (TÌM KIẾM + LỌC + SẮP XẾP + PHÂN TRANG)
        // ====================================================================
        public async Task<IActionResult> Index(
            string? searchString,
            int? maLoaiSan,
            decimal? minPrice,
            decimal? maxPrice,
            string? sortOrder,
            int pageIndex = 1)
        {
            int pageSize = 6; // Hiển thị 6 sân trên 1 trang

            // Bắt đầu truy vấn IQueryable với Include loại sân
            var query = _context.SanTheThaos
                .Include(s => s.LoaiSan)
                .Where(s => s.TrangThai == true) // Chỉ lấy các sân đang hoạt động
                .AsQueryable();

            // 1. TÌM KIẾM (Theo tên sân, tên loại sân hoặc địa chỉ)
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                searchString = searchString.Trim();
                query = query.Where(s => s.TenSan.Contains(searchString) ||
                                         s.DiaChi.Contains(searchString) ||
                                         s.LoaiSan.TenLoai.Contains(searchString));
            }

            // 2. LỌC
            // Lọc theo Loại sân
            if (maLoaiSan.HasValue && maLoaiSan.Value > 0)
            {
                query = query.Where(s => s.MaLoaiSan == maLoaiSan.Value);
            }

            // Lọc theo Khoảng giá
            if (minPrice.HasValue)
            {
                query = query.Where(s => s.DonGia >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(s => s.DonGia <= maxPrice.Value);
            }

            // 3. SẮP XẾP (LINQ OrderBy / OrderByDescending)
            switch (sortOrder)
            {
                case "ten_desc":
                    query = query.OrderByDescending(s => s.TenSan);
                    break;
                case "gia_asc":
                    query = query.OrderBy(s => s.DonGia);
                    break;
                case "gia_desc":
                    query = query.OrderByDescending(s => s.DonGia);
                    break;
                case "gio_asc":
                    query = query.OrderBy(s => s.GioMoCua);
                    break;
                case "gio_desc":
                    query = query.OrderByDescending(s => s.GioMoCua);
                    break;
                default: // Default: Tên sân A-Z
                    query = query.OrderBy(s => s.TenSan);
                    break;
            }

            // 4. PHÂN TRANG (LINQ CountAsync, Skip, Take)
            int totalItems = await query.CountAsync();
            pageIndex = Math.Max(1, pageIndex);

            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Lấy danh sách loại sân cho Dropdown Lọc
            var dsLoaiSan = await _context.LoaiSans.Where(l => l.TrangThai).ToListAsync();

            var viewModel = new SanTheThaoListViewModel
            {
                Items = items,
                LoaiSans = dsLoaiSan,
                SearchString = searchString,
                MaLoaiSan = maLoaiSan,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortOrder = sortOrder,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return View(viewModel);
        }

        // ====================================================================
        // 2. CHI TIẾT SÂN THỂ THAO
        // ====================================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var sanTheThao = await _context.SanTheThaos
                .Include(s => s.LoaiSan)
                .FirstOrDefaultAsync(m => m.MaSan == id);

            if (sanTheThao == null) return NotFound();

            return View(sanTheThao);
        }

        // ====================================================================
        // 3. TRANG QUẢN TRỊ ADMIN / NHÂN VIÊN (DANH SÁCH BẢNG)
        // ====================================================================
        public async Task<IActionResult> AdminIndex(string? searchString, int? maLoaiSan, int pageIndex = 1)
        {
            if (!KiemTraQuyenQuanLy())
            {
                TempData["Error"] = "Bạn không có quyền truy cập trang quản trị sân!";
                return RedirectToAction("DangNhap", "TaiKhoan");
            }

            int pageSize = 10;
            var query = _context.SanTheThaos
                .Include(s => s.LoaiSan)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                query = query.Where(s => s.TenSan.Contains(searchString) || s.DiaChi.Contains(searchString));
            }

            if (maLoaiSan.HasValue && maLoaiSan > 0)
            {
                query = query.Where(s => s.MaLoaiSan == maLoaiSan);
            }

            int totalItems = await query.CountAsync();
            pageIndex = Math.Max(1, pageIndex);

            var items = await query
                .OrderByDescending(s => s.MaSan)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.LoaiSans = new SelectList(await _context.LoaiSans.ToListAsync(), "MaLoaiSan", "TenLoai", maLoaiSan);
            ViewBag.SearchString = searchString;
            ViewBag.MaLoaiSan = maLoaiSan;
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            return View(items);
        }

        // ====================================================================
        // 4. THÊM MỚI SÂN (ADMIN / NHÂN VIÊN)
        // ====================================================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!KiemTraQuyenQuanLy()) return RedirectToAction("DangNhap", "TaiKhoan");

            ViewBag.MaLoaiSan = new SelectList(await _context.LoaiSans.Where(l => l.TrangThai).ToListAsync(), "MaLoaiSan", "TenLoai");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SanTheThao sanTheThao)
        {
            if (!KiemTraQuyenQuanLy()) return RedirectToAction("DangNhap", "TaiKhoan");

            // Kiểm tra trùng tên sân
            if (await _context.SanTheThaos.AnyAsync(s => s.TenSan.ToLower() == sanTheThao.TenSan.ToLower()))
            {
                ModelState.AddModelError("TenSan", "Tên sân này đã tồn tại trong hệ thống!");
            }

            // Kiểm tra giờ mở/đóng cửa hợp lệ
            if (sanTheThao.GioDongCua <= sanTheThao.GioMoCua)
            {
                ModelState.AddModelError("GioDongCua", "Giờ đóng cửa phải sau giờ mở cửa!");
            }

            if (ModelState.IsValid)
            {
                _context.SanTheThaos.Add(sanTheThao);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm mới sân thể thao thành công!";
                return RedirectToAction(nameof(AdminIndex));
            }

            ViewBag.MaLoaiSan = new SelectList(await _context.LoaiSans.Where(l => l.TrangThai).ToListAsync(), "MaLoaiSan", "TenLoai", sanTheThao.MaLoaiSan);
            return View(sanTheThao);
        }

        // ====================================================================
        // 5. CẬP NHẬT SÂN (ADMIN / NHÂN VIÊN)
        // ====================================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!KiemTraQuyenQuanLy()) return RedirectToAction("DangNhap", "TaiKhoan");
            if (id == null) return NotFound();

            var sanTheThao = await _context.SanTheThaos.FindAsync(id);
            if (sanTheThao == null) return NotFound();

            ViewBag.MaLoaiSan = new SelectList(await _context.LoaiSans.ToListAsync(), "MaLoaiSan", "TenLoai", sanTheThao.MaLoaiSan);
            return View(sanTheThao);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SanTheThao sanTheThao)
        {
            if (!KiemTraQuyenQuanLy()) return RedirectToAction("DangNhap", "TaiKhoan");
            if (id != sanTheThao.MaSan) return NotFound();

            // Kiểm tra trùng tên với sân khác
            if (await _context.SanTheThaos.AnyAsync(s => s.TenSan.ToLower() == sanTheThao.TenSan.ToLower() && s.MaSan != id))
            {
                ModelState.AddModelError("TenSan", "Tên sân này đã trùng với một sân khác!");
            }

            if (sanTheThao.GioDongCua <= sanTheThao.GioMoCua)
            {
                ModelState.AddModelError("GioDongCua", "Giờ đóng cửa phải sau giờ mở cửa!");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sanTheThao);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật thông tin sân thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.SanTheThaos.Any(e => e.MaSan == sanTheThao.MaSan)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(AdminIndex));
            }

            ViewBag.MaLoaiSan = new SelectList(await _context.LoaiSans.ToListAsync(), "MaLoaiSan", "TenLoai", sanTheThao.MaLoaiSan);
            return View(sanTheThao);
        }

        // ====================================================================
        // 6. XÓA SÂN THỂ THAO (KIỂM TRA THAM CHIẾU VỚI BẢNG ĐẶT SÂN)
        // ====================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!KiemTraQuyenQuanLy()) return RedirectToAction("DangNhap", "TaiKhoan");

            var sanTheThao = await _context.SanTheThaos.FindAsync(id);
            if (sanTheThao == null) return NotFound();

            // Kiểm tra xem sân đã phát sinh lịch đặt trong bảng DatSan chưa
            bool daCoLichDat = await _context.DatSans.AnyAsync(d => d.MaSan == id);
            if (daCoLichDat)
            {
                TempData["Error"] = "Không thể xóa sân này vì đã phát sinh lịch sử đặt sân trong hệ thống!";
                return RedirectToAction(nameof(AdminIndex));
            }

            _context.SanTheThaos.Remove(sanTheThao);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa sân thể thao thành công!";
            return RedirectToAction(nameof(AdminIndex));
        }
    }
}
