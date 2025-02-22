namespace HTSV.FE.Models.PhanCong
{
    public class PhanCongViewModel
    {
        public int Id { get; set; }
        public int NguoiDuocPhanCongId { get; set; }
        public string TenNguoiDuocPhanCong { get; set; } = string.Empty;
        public int NhiemVuId { get; set; }
        public string TenNhiemVu { get; set; } = string.Empty;
        public DateTime ThoiGianPhanCong { get; set; }
        public string? GhiChu { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime? NgaySua { get; set; }
    }

    public class CreatePhanCongModel
    {
        public int NguoiDuocPhanCongId { get; set; }
        public int NhiemVuId { get; set; }
        public string? GhiChu { get; set; }
    }

    public class UpdatePhanCongModel
    {
        public int? NguoiDuocPhanCongId { get; set; }
        public int? NhiemVuId { get; set; }
        public string? GhiChu { get; set; }
    }

    public class PhanCongNhomModel
    {
        public List<string> MaSinhViens { get; set; } = new List<string>();
        public int NhiemVuId { get; set; }
        public string GhiChu { get; set; } = string.Empty;
    }
} 