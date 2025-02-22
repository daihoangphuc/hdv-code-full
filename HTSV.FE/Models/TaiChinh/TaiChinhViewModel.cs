namespace HTSV.FE.Models.TaiChinh
{
    public class TaiChinhViewModel
    {
        public int Id { get; set; }
        public byte LoaiGiaoDich { get; set; }  // 1: Thu, 2: Chi
        public decimal SoTien { get; set; }
        public DateTime NgayThucHien { get; set; }
        public int NguoiThucHienId { get; set; }
        public string TenNguoiThucHien { get; set; } = string.Empty;
        public int? NguoiDongTienId { get; set; }
        public string TenNguoiDongTien { get; set; } = string.Empty;
        public int? HoatDongId { get; set; }
        public string TenHoatDong { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime? NgaySua { get; set; }
    }

    public class CreateTaiChinhModel
    {
        public byte LoaiGiaoDich { get; set; }
        public decimal SoTien { get; set; }
        public DateTime NgayThucHien { get; set; }
        public int? NguoiDongTienId { get; set; }
        public int? HoatDongId { get; set; }
        public string MoTa { get; set; } = string.Empty;
        public string? GhiChu { get; set; }
    }

    public class UpdateTaiChinhModel
    {
        public decimal? SoTien { get; set; }
        public DateTime? NgayThucHien { get; set; }
        public int? NguoiDongTienId { get; set; }
        public int? HoatDongId { get; set; }
        public string? MoTa { get; set; }
        public string? GhiChu { get; set; }
    }

    public class ThongKeTaiChinhModel
    {
        public decimal TongThu { get; set; }
        public decimal TongChi { get; set; }
        public decimal SoDu { get; set; }
        public DateTime TuNgay { get; set; }
        public DateTime DenNgay { get; set; }
    }
} 