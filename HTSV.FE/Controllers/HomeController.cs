using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using HTSV.FE.Models;
using HTSV.FE.Models.Home;
using HTSV.FE.Models.Common;
using HTSV.FE.Models.TinTuc;
using HTSV.FE.Models.HoatDong;
using HTSV.FE.Models.NguoiDung;
using System.Text.Json;
using HTSV.FE.Extensions;
using HTSV.FE.Models.Auth;

namespace HTSV.FE.Controllers;

public class HomeController : Controller
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ILogger<HomeController> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public HomeController(IHttpClientFactory clientFactory, ILogger<HomeController> logger)
    {
        _clientFactory = clientFactory;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    private bool AddAuthenticationHeader(HttpClient client)
    {
        try
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            
            var loginInfo = HttpContext.Session.Get<LoginResponseModel>("LoginResponse");
            if (loginInfo != null && !string.IsNullOrEmpty(loginInfo.Token))
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {loginInfo.Token}");
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding authentication header");
            return false;
        }
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeViewModel();
        using var client = _clientFactory.CreateClient("BE");
        AddAuthenticationHeader(client);

        try
        {
            // Lấy tin tức mới nhất
            var newsResponse = await client.GetAsync("/api/TinTuc/da-xuat-ban");
            if (newsResponse.IsSuccessStatusCode)
            {
                var content = await newsResponse.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<List<TinTucViewModel>>>(content, _jsonOptions);
                if (result?.Success == true)
                {
                    model.LatestNews = result.Data ?? new();
                }
            }

            // Lấy hoạt động sắp diễn ra
            var activitiesResponse = await client.GetAsync("/api/HoatDong/sap-dien-ra?limit=3");
            if (activitiesResponse.IsSuccessStatusCode)
            {
                var content = await activitiesResponse.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<List<HoatDongViewModel>>>(content, _jsonOptions);
                if (result?.Success == true)
                {
                    model.UpcomingActivities = result.Data ?? new();
                }
            }

            // Lấy danh sách ban chủ nhiệm
            var boardResponse = await client.GetAsync("/api/NguoiDung/ban-chu-nhiem");
            if (boardResponse.IsSuccessStatusCode)
            {
                var content = await boardResponse.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<List<NguoiDungViewModel>>>(content, _jsonOptions);
                if (result?.Success == true)
                {
                    model.ManagementBoard = result.Data ?? new();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching data for home page");
        }

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult HuongDan()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
