// Họ và tên: [Điền họ tên SV 4]
// Mã sinh viên: [Điền mã SV 4]
// Nội dung thực hiện: ViewModel DatSanXuLyListViewModel - Phục vụ danh sách đặt sân cần xử lý
// (Tìm kiếm, Lọc, Sắp xếp, Phân trang) - Module 4

using QLDatSanTheThao.Models.Entities;

namespace QLDatSanTheThao.Models.ViewModels
{
    public class DatSanXuLyListViewModel
    {
        // Danh sách đặt sân của trang hiện tại
        public List<DatSan> Items { get; set; } = new List<DatSan>();

        // Dữ liệu nền cho dropdown lọc
        public List<LoaiSan> LoaiSans { get; set; } = new List<LoaiSan>();
        public string[] TrangThais { get; set; } = Array.Empty<string>();

        // Điều kiện tìm kiếm / lọc / sắp xếp
        public string? SearchString { get; set; }
        public int? MaLoaiSan { get; set; }
        public string? TrangThai { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }
        public string? SortOrder { get; set; }

        // Phân trang
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        // Số lượng đơn theo từng trạng thái (toàn hệ thống) để hiển thị tổng quan
        public Dictionary<string, int> ThongKeTrangThai { get; set; } = new Dictionary<string, int>();

        // Cảnh báo về điều kiện lọc (ví dụ: từ ngày > đến ngày)
        public string? CanhBaoLoc { get; set; }
    }
}