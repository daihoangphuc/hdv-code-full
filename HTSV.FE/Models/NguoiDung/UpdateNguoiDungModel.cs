using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HTSV.FE.Models.NguoiDung
{
    public class UpdateNguoiDungModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [Display(Name = "Họ tên")]
        public string HoTen { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Lớp học là bắt buộc")]
        [Display(Name = "Lớp học")]
        public int LopHocId { get; set; }

        [Required(ErrorMessage = "Chức vụ là bắt buộc")]
        [Display(Name = "Chức vụ")]
        public int ChucVuId { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public string? AnhDaiDien { get; set; }

        [Display(Name = "File ảnh đại diện")]
        public IFormFile? AnhDaiDienFile { get; set; }

        public bool TrangThai { get; set; }
    }
} 