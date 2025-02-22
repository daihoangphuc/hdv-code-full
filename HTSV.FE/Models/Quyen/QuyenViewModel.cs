namespace HTSV.FE.Models.Quyen
{
    public class QuyenViewModel
    {
        public int Id { get; set; }
        public string TenQuyen { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; }
        public DateTime? NgaySua { get; set; }
    }

    public class CreateQuyenModel
    {
        public string TenQuyen { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
    }

    public class UpdateQuyenModel
    {
        public string? TenQuyen { get; set; }
        public string? MoTa { get; set; }
    }

    public class PhanQuyenModel
    {
        public int NguoiDungId { get; set; }
        public int[] QuyenIds { get; set; } = Array.Empty<int>();
    }
} 