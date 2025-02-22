using HTSV.FE.Models.Enums;

namespace HTSV.FE.Extensions
{
    public static class HoatDongExtensions
    {
        public static string GetTrangThaiText(this byte trangThai)
        {
            return trangThai switch
            {
                1 => "Sắp diễn ra",
                2 => "Đang diễn ra",
                3 => "Đã kết thúc",
                4 => "Đã hủy",
                _ => "Không xác định"
            };
        }

        public static string GetTrangThaiClass(this byte trangThai)
        {
            return trangThai switch
            {
                1 => "bg-blue-100 text-blue-800",
                2 => "bg-green-100 text-green-800",
                3 => "bg-gray-100 text-gray-800",
                4 => "bg-red-100 text-red-800",
                _ => "bg-gray-100 text-gray-800"
            };
        }
    }
} 