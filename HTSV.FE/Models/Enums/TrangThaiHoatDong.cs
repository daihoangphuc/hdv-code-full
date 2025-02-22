namespace HTSV.FE.Models.Enums
{
    public static class TrangThaiHoatDong
    {
        public const byte SapDienRa = 1;
        public const byte DangDienRa = 2;
        public const byte DaKetThuc = 3;
        public const byte DaBiHuy = 4;

        public static string GetDescription(byte trangThai) => trangThai switch
        {
            SapDienRa => "Sắp diễn ra",
            DangDienRa => "Đang diễn ra",
            DaKetThuc => "Đã kết thúc",
            DaBiHuy => "Đã bị hủy",
            _ => "Không xác định"
        };

        public static string GetTrangThaiClass(byte trangThai) => trangThai switch
        {
            SapDienRa => "bg-warning",
            DangDienRa => "bg-primary",
            DaKetThuc => "bg-success",
            DaBiHuy => "bg-danger",
            _ => "bg-secondary"
        };
    }

    public static class TrangThaiDangKy
    {
        public const byte DaHuy = 0;
        public const byte DaDangKy = 1;

        public static string GetDescription(byte trangThai) => trangThai switch
        {
            DaHuy => "Đã hủy đăng ký",
            DaDangKy => "Đã đăng ký",
            _ => "Không xác định"
        };

        public static string GetTrangThaiClass(byte trangThai) => trangThai switch
        {
            DaHuy => "bg-danger",
            DaDangKy => "bg-success",
            _ => "bg-secondary"
        };
    }

    public static class TrangThaiThamGia
    {
        public const byte VangMat = 0;
        public const byte DaThamGia = 1;

        public static string GetDescription(byte trangThai) => trangThai switch
        {
            VangMat => "Vắng mặt",
            DaThamGia => "Đã tham gia",
            _ => "Không xác định"
        };

        public static string GetTrangThaiClass(byte trangThai) => trangThai switch
        {
            VangMat => "bg-danger",
            DaThamGia => "bg-success",
            _ => "bg-secondary"
        };
    }
} 