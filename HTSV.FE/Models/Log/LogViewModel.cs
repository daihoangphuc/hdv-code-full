namespace HTSV.FE.Models.Log
{
    public class LogViewModel
    {
        public int Id { get; set; }
        public string? IP { get; set; }
        public int? NguoiDungId { get; set; }
        public string? TenNguoiDung { get; set; }
        public string? HanhDong { get; set; }
        public DateTime ThoiGian { get; set; }
        public string? KetQua { get; set; }
        public string? MoTa { get; set; }
        public string? GhiChu { get; set; }
    }
} 