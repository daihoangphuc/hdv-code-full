using System;

namespace HTSV.FE.Models.ThamGia
{
    public class ThamGiaDTO
    {
        public int Id { get; set; }
        public int NguoiThamGiaId { get; set; }
        public string TenNguoiThamGia { get; set; }
        public int HoatDongId { get; set; }
        public string TenHoatDong { get; set; }
        public byte TrangThai { get; set; }
        public DateTime? ThoiGianDiemDanh { get; set; }
        public string HinhThucDiemDanh { get; set; }
        public string GhiChu { get; set; }
        public double? KinhDo { get; set; }
        public double? ViDo { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime? NgaySua { get; set; }
    }

    public class DiemDanhDTO
    {
        public int HoatDongId { get; set; }
        public string MaSinhVien { get; set; }
    }

    public class DiemDanhNhomDTO
    {
        public int HoatDongId { get; set; }
        public List<string> DanhSachMaSinhVien { get; set; }
    }

    public class DiemDanhGPSDTO
    {
        public int HoatDongId { get; set; }
        public string MaSinhVien { get; set; }
        public double KinhDo { get; set; }
        public double ViDo { get; set; }
    }

    public class KiemTraThamGiaDTO
    {
        public int HoatDongId { get; set; }
        public string MaSinhVien { get; set; }
    }

    public class CapNhatTrangThaiDTO
    {
        public int TrangThai { get; set; }
        public string GhiChu { get; set; }
    }
} 