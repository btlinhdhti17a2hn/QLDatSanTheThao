// Họ và tên: [Điền tên SV 2]
// Mã sinh viên: [Điền mã SV 2]
// Nội dung thực hiện: ViewModel SanTheThaoListViewModel - Phục vụ Tìm kiếm, Lọc, Sắp xếp và Phân trang sân thể thao

using QLDatSanTheThao.Models.Entities;

namespace QLDatSanTheThao.Models.ViewModels
{
    public class SanTheThaoListViewModel
    {
        // Danh sách sân thể thao của trang hiện tại
        public List<SanTheThao> Items { get; set; } = new List<SanTheThao>();

        // Danh sách Loại sân phục vụ cho SelectList / Dropdown lọc
        public List<LoaiSan> LoaiSans { get; set; } = new List<LoaiSan>();

        // Các thuộc tính phục vụ Bộ lọc & Tìm kiếm
        public string? SearchString { get; set; }
        public int? MaLoaiSan { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? SortOrder { get; set; }

        // Các thuộc tính phục vụ Phân trang (Pagination)
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 6; // Hiển thị 6 sân trên 1 trang
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }
}
