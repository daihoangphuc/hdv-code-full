using System;

namespace HTSV.FE.Models.HoatDong
{
    public class DangKyDTO
    {
        public int Id { get; set; }
        public int NguoiDangKyId { get; set; }
        public string TenNguoiDangKy { get; set; }
        public int HoatDongId { get; set; }
        public string TenHoatDong { get; set; }
        public DateTime ThoiGianDangKy { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime? NgaySua { get; set; }
    }
} 