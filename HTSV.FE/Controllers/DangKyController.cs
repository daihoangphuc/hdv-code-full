using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using HTSV.FE.Models.Common;
using HTSV.FE.Models.HoatDong;

namespace HTSV.FE.Controllers
{
    [Authorize]
    public class DangKyController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<DangKyController> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public DangKyController(IHttpClientFactory clientFactory, ILogger<DangKyController> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string searchTerm = "")
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                var token = HttpContext.Session.GetString("TokenUser");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                string url = $"api/DangKy?PageIndex={page}&PageSize={pageSize}";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    url += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
                }

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedList<DangKyDTO>>>(content, _jsonOptions);

                    if (apiResponse?.Success == true && apiResponse.Data != null)
                    {
                        return View(apiResponse.Data);
                    }
                }

                return View(new PaginatedList<DangKyDTO>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index action");
                return View(new PaginatedList<DangKyDTO>());
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Member")]
        public async Task<IActionResult> Export(string? maSinhVien = null, int? hoatDongId = null)
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                var token = HttpContext.Session.GetString("TokenUser");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                string url = $"api/DangKy/export";
                if (!string.IsNullOrEmpty(maSinhVien))
                {
                    url += $"?maSinhVien={Uri.EscapeDataString(maSinhVien)}";
                }
                if (hoatDongId.HasValue)
                {
                    url += url.Contains("?") ? "&" : "?";
                    url += $"hoatDongId={hoatDongId}";
                }

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var fileBytes = await response.Content.ReadAsByteArrayAsync();
                    string fileName = "DanhSachDangKy.xlsx";
                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting DangKy to Excel");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetHoatDongs()
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                var token = HttpContext.Session.GetString("TokenUser");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync("api/HoatDong?PageSize=100");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedList<HoatDongViewModel>>>(content, _jsonOptions);

                    if (apiResponse?.Success == true && apiResponse.Data != null)
                    {
                        return Json(new { success = true, data = apiResponse.Data.Items });
                    }
                }

                return Json(new { success = false, message = "Không thể lấy danh sách hoạt động" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hoat dong list");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi lấy danh sách hoạt động" });
            }
        }
    }
} 