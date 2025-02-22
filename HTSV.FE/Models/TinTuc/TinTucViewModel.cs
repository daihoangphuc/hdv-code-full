using System;

namespace HTSV.FE.Models.TinTuc
{
    public class TinTucViewModel
    {
        public int Id { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string TomTat { get; set; } = string.Empty;
        public string AnhBia { get; set; } = string.Empty;
        public DateTime NgayDang { get; set; }
        public int NguoiDangTinId { get; set; }
        public string TenNguoiDangTin { get; set; } = string.Empty;
        public string? FileDinhKem { get; set; }
        public bool TrangThai { get; set; }
    }

    public class CreateTinTucModel
    {
        public string TieuDe { get; set; } = string.Empty;
        public string NoiDung { get; set; } = string.Empty;
        public string? FileDinhKem { get; set; }
        public bool TrangThai { get; set; }
    }

    public class UpdateTinTucModel
    {
        public string? TieuDe { get; set; }
        public string? NoiDung { get; set; }
        public string? FileDinhKem { get; set; }
        public bool TrangThai { get; set; }
    }
} 