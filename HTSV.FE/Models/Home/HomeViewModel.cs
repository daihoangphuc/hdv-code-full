using HTSV.FE.Models.TinTuc;
using HTSV.FE.Models.HoatDong;
using HTSV.FE.Models.NguoiDung;

namespace HTSV.FE.Models.Home
{
    public class HomeViewModel
    {
        public List<TinTucViewModel> LatestNews { get; set; } = new();
        public List<HoatDongViewModel> UpcomingActivities { get; set; } = new();
        public List<NguoiDungViewModel> ManagementBoard { get; set; } = new();
    }
} 