using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HTSV.FE.Models.NguoiDung
{
    public class CreateNguoiDungViewModel
    {
        [Required(ErrorMessage = "Mã số sinh viên là bắt buộc")]
        [Display(Name = "Mã số sinh viên")]
        public string MaSoSinhVien { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }

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
        public IFormFile AnhDaiDienFile { get; set; }
    }
} 