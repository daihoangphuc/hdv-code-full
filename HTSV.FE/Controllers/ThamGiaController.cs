using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HTSV.FE.Models.ThamGia;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using HTSV.FE.Models.Common;
using Microsoft.Extensions.Logging;
using HTSV.FE.Models.HoatDong;

namespace HTSV.FE.Controllers
{
    [Authorize]
    public class ThamGiaController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly ILogger<ThamGiaController> _logger;

        public ThamGiaController(IHttpClientFactory clientFactory, ILogger<ThamGiaController> logger)
        {
            _clientFactory = clientFactory;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            _logger = logger;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string searchTerm = "")
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                var token = HttpContext.Session.GetString("TokenUser");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                string url = $"api/ThamGia?PageIndex={page}&PageSize={pageSize}";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    url += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
                }

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedList<ThamGiaDTO>>>(content, _jsonOptions);

                    if (apiResponse?.Success == true && apiResponse.Data != null)
                    {
                        return View(apiResponse.Data);
                    }
                }

                return View(new PaginatedList<ThamGiaDTO>());
            }
            catch (Exception)
            {
                return View(new PaginatedList<ThamGiaDTO>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try 
            {
                var client = _clientFactory.CreateClient("BE");
                var token = HttpContext.Session.GetString("TokenUser");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"api/ThamGia/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ThamGiaDTO>>(content, _jsonOptions);
                    
                    if (apiResponse?.Success == true && apiResponse.Data != null)
                    {
                        ViewBag.MaSinhVien = HttpContext.Session.GetString("MaSinhVien");
                        return View(apiResponse.Data);
                    }
                }

                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Details action");
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> DiemDanh(DiemDanhDTO model)
        {
            var client = _clientFactory.CreateClient();
            var token = HttpContext.Session.GetString("TokenUser");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/ThamGia/diem-danh", content);

            if (response.IsSuccessStatusCode)
                return Json(new { success = true });

            return Json(new { success = false, message = "Điểm danh thất bại" });
        }

        [HttpPost]
        public async Task<IActionResult> DiemDanhNhom(DiemDanhNhomDTO model)
        {
            var client = _clientFactory.CreateClient();
            var token = HttpContext.Session.GetString("TokenUser");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/ThamGia/diem-danh-nhom", content);

            if (response.IsSuccessStatusCode)
                return Json(new { success = true });

            return Json(new { success = false, message = "Điểm danh nhóm thất bại" });
        }

        [HttpPost]
        public async Task<IActionResult> DiemDanhGPS(DiemDanhGPSDTO model)
        {
            var client = _clientFactory.CreateClient();
            var token = HttpContext.Session.GetString("TokenUser");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/ThamGia/diem-danh-gps", content);

            if (response.IsSuccessStatusCode)
                return Json(new { success = true });

            return Json(new { success = false, message = "Điểm danh GPS thất bại" });
        }

        [HttpPost]
        public async Task<IActionResult> KiemTraThamGia(KiemTraThamGiaDTO model)
        {
            var client = _clientFactory.CreateClient();
            var token = HttpContext.Session.GetString("TokenUser");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/ThamGia/kiem-tra-tham-gia", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return Json(new { success = true, data = result });
            }

            return Json(new { success = false, message = "Kiểm tra thất bại" });
        }

        [HttpPut]
        public async Task<IActionResult> CapNhatTrangThai(int id, CapNhatTrangThaiDTO model)
        {
            var client = _clientFactory.CreateClient();
            var token = HttpContext.Session.GetString("TokenUser");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"api/ThamGia/{id}/trang-thai", content);

            if (response.IsSuccessStatusCode)
                return Json(new { success = true });

            return Json(new { success = false, message = "Cập nhật trạng thái thất bại" });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _clientFactory.CreateClient();
            var token = HttpContext.Session.GetString("TokenUser");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.DeleteAsync($"api/ThamGia/{id}");
            if (response.IsSuccessStatusCode)
                return Json(new { success = true });

            return Json(new { success = false, message = "Xóa thất bại" });
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Export(string? maSinhVien = null, int? hoatDongId = null)
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                var token = HttpContext.Session.GetString("TokenUser");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                string url = $"api/ThamGia/export";
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
                    string fileName = "DanhSachThamGia.xlsx";
                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting ThamGia to Excel");
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