using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDatSanTheThao.Models.Entities
{
    public class SanTheThao
    {
            [Key]
            public int MaSan { get; set; }

            [Required, StringLength(100)]
            [Display(Name = "Tên sân")]
            public string TenSan { get; set; }

            [Required]
            [Display(Name = "Loại sân")]
            public int MaLoaiSan { get; set; }

            [ForeignKey("MaLoaiSan")]
            public LoaiSan LoaiSan { get; set; }

            [StringLength(200)]
            [Display(Name = "Địa chỉ")]
            public string DiaChi { get; set; }

            [StringLength(500)]
            [Display(Name = "Tiện ích")]
            public string TienIch { get; set; }

            [Column(TypeName = "decimal(18,2)")]
            [Display(Name = "Đơn giá")]
            public decimal DonGia { get; set; }

            [Display(Name = "Trạng thái")]
            public bool TrangThai { get; set; } = true;

            [Display(Name = "Giờ mở cửa")]
            public TimeSpan GioMoCua { get; set; }

            [Display(Name = "Giờ đóng cửa")]
            public TimeSpan GioDongCua { get; set; }

            [StringLength(500)]
            [Display(Name = "Ghi chú")]
            public string GhiChu { get; set; }

            [Display(Name = "Ngày bảo trì")]
            public DateTime? NgayBaoTri { get; set; }

            // Navigation
            public ICollection<DatSan> DanhSachDatSan { get; set; }
       
    }
}
