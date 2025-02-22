namespace HTSV.FE.Models.LopHoc
{
    public class LopHocViewModel
    {
        public int Id { get; set; }
        public string MaLop { get; set; } = string.Empty;
        public string TenLop { get; set; } = string.Empty;
        public int KhoaTruongId { get; set; }
        public string? TenKhoaTruong { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime? NgaySua { get; set; }
    }

    public class CreateLopHocModel
    {
        public string MaLop { get; set; } = string.Empty;
        public string TenLop { get; set; } = string.Empty;
        public int KhoaTruongId { get; set; }
    }

    public class UpdateLopHocModel
    {
        public string? MaLop { get; set; }
        public string? TenLop { get; set; }
        public int? KhoaTruongId { get; set; }
    }
} 