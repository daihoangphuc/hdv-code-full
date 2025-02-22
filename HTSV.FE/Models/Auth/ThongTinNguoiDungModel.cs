using System.Collections.Generic;

namespace HTSV.FE.Models.Auth
{
    public class ThongTinNguoiDungModel
    {
        public int Id { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
        public List<string> DanhSachQuyen { get; set; } = new List<string>();
    }
} 