namespace HTSV.FE.Models.ChucVu
{
    public class ChucVuViewModel
    {
        public int Id { get; set; }
        public string TenChucVu { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public string NhiemKy { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; }
        public DateTime? NgaySua { get; set; }
    }

    public class CreateChucVuModel
    {
        public string TenChucVu { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public string NhiemKy { get; set; } = string.Empty;
    }

    public class UpdateChucVuModel
    {
        public string? TenChucVu { get; set; }
        public string? MoTa { get; set; }
        public string? NhiemKy { get; set; }
    }
} 