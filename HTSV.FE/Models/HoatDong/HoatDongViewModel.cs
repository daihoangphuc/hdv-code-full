using System;
using HTSV.FE.Models.Enums;

namespace HTSV.FE.Models.HoatDong
{
    public class HoatDongViewModel
    {
        public int Id { get; set; }
        public string TenHoatDong { get; set; }
        public string MoTa { get; set; }
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        public string DiaDiem { get; set; }
        public int SoLuongToiDa { get; set; }
        public string ToaDo { get; set; }
        public byte HocKy { get; set; }
        public string NamHoc { get; set; }
        public byte TrangThai { get; set; }
        public bool CongKhai { get; set; }
        public string FileMinhChung { get; set; }
        public int SoLuongDangKy { get; set; }
        public int SoLuongThamGia { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime? NgaySua { get; set; }
    }

    public class CreateHoatDongModel
    {
        public string TenHoatDong { get; set; }
        public string MoTa { get; set; }
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        public string DiaDiem { get; set; }
        public int SoLuongToiDa { get; set; }
        public string ToaDo { get; set; }
        public byte HocKy { get; set; }
        public string NamHoc { get; set; }
        public byte TrangThai { get; set; } = TrangThaiHoatDong.SapDienRa;
        public bool CongKhai { get; set; }
    }

    public class UpdateHoatDongModel
    {
        public string TenHoatDong { get; set; }
        public string MoTa { get; set; }
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        public string DiaDiem { get; set; }
        public int SoLuongToiDa { get; set; }
        public string ToaDo { get; set; }
        public byte HocKy { get; set; }
        public string NamHoc { get; set; }
        public byte TrangThai { get; set; }
        public bool CongKhai { get; set; }
    }

    public class CapNhatTrangThaiModel
    {
        public int HoatDongId { get; set; }
        public byte TrangThai { get; set; }
    }

    public class UploadMinhChungModel
    {
        public int HoatDongId { get; set; }
        public string FileMinhChung { get; set; } = string.Empty;
    }
} 