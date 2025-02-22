namespace HTSV.FE.Models.KhoaTruong
{
    public class KhoaTruongViewModel
    {
        public int Id { get; set; }
        public string TenKhoaTruong { get; set; } = string.Empty;
        public string MaKhoaTruong { get; set; } = string.Empty;
        public DateTime NgayTao { get; set; }
        public DateTime? NgaySua { get; set; }
    }

    public class CreateKhoaTruongModel
    {
        public string TenKhoaTruong { get; set; } = string.Empty;
        public string MaKhoaTruong { get; set; } = string.Empty;
    }

    public class UpdateKhoaTruongModel
    {
        public string? TenKhoaTruong { get; set; }
        public string? MaKhoaTruong { get; set; }
    }
} 