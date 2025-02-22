namespace HTSV.FE.Models.Admin
{
    public class ThongKeViewModel
    {
        public int TongHoatDong { get; set; }
        public int HoatDongDangDienRa { get; set; }
        public int TongLuotDangKy { get; set; }
        public int TongLuotThamGia { get; set; }
        public List<ThongKeTheoThangViewModel> ThongKeTheoThang { get; set; } = new();
        public List<ThongKeTrangThaiViewModel> ThongKeTrangThai { get; set; } = new();
    }

    public class ThongKeTheoThangViewModel
    {
        public int Thang { get; set; }
        public int Nam { get; set; }
        public int SoHoatDong { get; set; }
        public int SoLuotDangKy { get; set; }
        public int SoLuotThamGia { get; set; }
    }

    public class ThongKeTrangThaiViewModel
    {
        public string TrangThai { get; set; } = string.Empty;
        public int SoLuong { get; set; }
    }
} 