namespace HTSV.FE.Models.NhiemVu
{
    public class NhiemVuViewModel
    {
        public int Id { get; set; }
        public string TenNhiemVu { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public int NguoiTaoId { get; set; }
        public string TenNguoiTao { get; set; } = string.Empty;
        public byte TrangThai { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime? NgaySua { get; set; }
    }

    public class CreateNhiemVuModel
    {
        public string TenNhiemVu { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
    }

    public class UpdateNhiemVuModel
    {
        public string? TenNhiemVu { get; set; }
        public string? MoTa { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public byte? TrangThai { get; set; }
    }
} 