using System;
using System.Collections.Generic;

namespace HTSV.FE.Models.NguoiDung
{
    public class NguoiDungViewModel
    {
        public int Id { get; set; }
        public string MaSoSinhVien { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public DateTime? NgaySinh { get; set; }
        public string GioiTinh { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public int LopHocId { get; set; }
        public string TenLop { get; set; } = string.Empty;
        public int ChucVuId { get; set; }
        public string TenChucVu { get; set; } = string.Empty;
        public string ChucVu { get; set; } = string.Empty;
        public string? AnhDaiDien { get; set; }
        public DateTime? LanTruyCapCuoi { get; set; }
        public bool TrangThai { get; set; }
        public List<string> DanhSachQuyen { get; set; } = new();
        public DateTime NgayTao { get; set; }
        public DateTime? NgaySua { get; set; }
    }

    public class CreateNguoiDungModel
    {
        public string MaSoSinhVien { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public int LopHocId { get; set; }
        public int ChucVuId { get; set; }
        public string MatKhau { get; set; } = string.Empty;
        public string XacNhanMatKhau { get; set; } = string.Empty;
    }

    public class UpdateMatKhauModel
    {
        public string MatKhauCu { get; set; } = string.Empty;
        public string MatKhauMoi { get; set; } = string.Empty;
        public string XacNhanMatKhau { get; set; } = string.Empty;
    }
} 