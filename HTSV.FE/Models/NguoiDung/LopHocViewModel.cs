namespace HTSV.FE.Models.NguoiDung
{
    public class LopHocViewModel
    {
        public int Id { get; set; }
        public string TenLop { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public bool TrangThai { get; set; }
    }
} 