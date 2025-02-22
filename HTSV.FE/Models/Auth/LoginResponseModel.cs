namespace HTSV.FE.Models.Auth
{
    public class LoginResponseModel
    {
        public string Token { get; set; } = string.Empty;
        public DateTime HetHan { get; set; }
        public ThongTinNguoiDung ThongTinNguoiDung { get; set; } = new();
    }

    public class ThongTinNguoiDung
    {
        public string MaSoSinhVien { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public int LopHocId { get; set; }
        public string TenLop { get; set; } = string.Empty;
        public int ChucVuId { get; set; }
        public string TenChucVu { get; set; } = string.Empty;
        public string? AnhDaiDien { get; set; }
        public DateTime LanTruyCapCuoi { get; set; }
        public bool TrangThai { get; set; }
        public List<string> DanhSachQuyen { get; set; } = new();
        public int Id { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgaySua { get; set; }
    }
} 