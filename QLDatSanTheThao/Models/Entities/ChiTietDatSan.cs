using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDatSanTheThao.Models.Entities
{
    public class ChiTietDatSan
    {
        [Key]
        public int MaChiTiet { get; set; }

        [Required]
        [Display(Name = "Đặt sân")]
        public int MaDatSan { get; set; }

        [ForeignKey("MaDatSan")]
        public DatSan DatSan { get; set; }

        [Required]
        [Display(Name = "Dịch vụ")]
        public int MaDichVu { get; set; }

        [ForeignKey("MaDichVu")]
        public DichVu DichVu { get; set; }

        [Range(1, 1000)]
        [Display(Name = "Số lượng")]
        public int SoLuong { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Đơn giá")]
        public decimal DonGia { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Thành tiền")]
        public decimal ThanhTien { get; set; }

        [StringLength(200)]
        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } = true;
    }
}
