using HTSV.FE.Extensions;
using HTSV.FE.Models.Common;
using HTSV.FE.Models.HoatDong;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace HTSV.FE.Controllers
{
    [Authorize]
    public class HoatDongController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<HoatDongController> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public HoatDongController(IHttpClientFactory clientFactory, ILogger<HoatDongController> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        private bool AddAuthenticationHeader(HttpClient client)
        {
            try
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                
                var token = HttpContext.Session.GetString("TokenUser");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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

        public async Task<IActionResult> Index(int page = 1, int pageSize = 5, string? searchTerm = null)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return RedirectToAction("Login", "Account");
                }

                var url = $"api/HoatDong?pageIndex={page}&pageSize={pageSize}";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    url += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
                }

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<PaginatedList<HoatDongViewModel>>>(content, _jsonOptions);

                    if (result?.Success == true)
                    {
                        return View(result.Data);
                    }
                }

                return View(new PaginatedList<HoatDongViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index action");
                return View(new PaginatedList<HoatDongViewModel>());
            }
        }

        [Authorize(Roles = "Member")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Lấy thông tin hoạt động
                var response = await client.GetAsync($"api/HoatDong/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<HoatDongViewModel>>(content, _jsonOptions);

                if (result?.Success != true || result.Data == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                // Kiểm tra trạng thái đăng ký của người dùng
                var maSinhVien = User.GetMaSinhVien();
                var dangKyResponse = await client.GetAsync($"api/DangKy/nguoi-dang-ky?maSinhVien={maSinhVien}");
                
                if (dangKyResponse.IsSuccessStatusCode)
                {
                    var dangKyContent = await dangKyResponse.Content.ReadAsStringAsync();
                    var dangKyResult = JsonSerializer.Deserialize<ApiResponse<List<DangKyDTO>>>(dangKyContent, _jsonOptions);

                    if (dangKyResult?.Success == true && dangKyResult.Data != null)
                    {
                        var dangKy = dangKyResult.Data.FirstOrDefault(dk => dk.HoatDongId == id);
                        ViewBag.DaDangKy = dangKy != null;
                        ViewBag.DangKyId = dangKy?.Id;
                    }
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Details action");
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> UpcomingActivities(int limit = 5)
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                AddAuthenticationHeader(client);
                
                var response = await client.GetAsync($"/api/hoatdong/sap-dien-ra?limit={limit}");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<List<HoatDongViewModel>>>(
                        await response.Content.ReadAsStringAsync(),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );

                    if (result?.Success == true)
                    {
                        return PartialView("_UpcomingActivities", result.Data);
                    }
                }
                
                return PartialView("_UpcomingActivities", new List<HoatDongViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading upcoming activities");
                return PartialView("_UpcomingActivities", new List<HoatDongViewModel>());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateHoatDongModel model)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.PostAsJsonAsync("api/HoatDong", model);
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<HoatDongViewModel>>(content, _jsonOptions);

                return Json(new { success = response.IsSuccessStatusCode, message = result?.Message, data = result?.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating activity");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tạo hoạt động" });
            }
        }

        [HttpPut]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateHoatDongModel model)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.PutAsJsonAsync($"api/HoatDong/{id}", model);
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<HoatDongViewModel>>(content, _jsonOptions);

                return Json(new { success = response.IsSuccessStatusCode, message = result?.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating activity");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật hoạt động" });
            }
        }

        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus([FromBody] CapNhatTrangThaiModel model)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.PutAsJsonAsync($"api/HoatDong/{model.TrangThai}/trang-thai", model);
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<bool>>(content, _jsonOptions);

                return Json(new { success = response.IsSuccessStatusCode, message = result?.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating activity status");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật trạng thái" });
            }
        }

        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateMinhChung([FromBody] UploadMinhChungModel model)
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                AddAuthenticationHeader(client);
                
                var response = await client.PutAsJsonAsync($"/api/hoatdong/{model.HoatDongId}/minh-chung", model.FileMinhChung);
                var content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<bool>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (result?.Success == true)
                    {
                        return Json(new { success = true, message = "Cập nhật minh chứng thành công" });
                    }
                }
                
                return Json(new { success = false, message = "Không thể cập nhật minh chứng" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating activity proof");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật minh chứng" });
            }
        }

        [HttpDelete]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.DeleteAsync($"api/HoatDong/{id}");
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<bool>>(content, _jsonOptions);

                return Json(new { success = response.IsSuccessStatusCode, message = result?.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting activity");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa hoạt động" });
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Export()
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return RedirectToAction("Login", "Account");
                }

                var response = await client.GetAsync("api/HoatDong/export");
                if (response.IsSuccessStatusCode)
                {
                    var fileBytes = await response.Content.ReadAsByteArrayAsync();
                    return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DanhSachHoatDong.xlsx");
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting activities");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> DangKy([FromBody] int hoatDongId)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var loginInfo = HttpContext.Session.Get<LoginResponseModel>("LoginResponse");
                var data = new
                {
                    MaSinhVien = loginInfo.ThongTinNguoiDung.MaSoSinhVien,
                    HoatDongId = hoatDongId
                };

                var response = await client.PostAsJsonAsync("api/DangKy", data);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<object>>(content, _jsonOptions);
                    return Json(new { success = true, message = "Đăng ký thành công" });
                }

                return Json(new { success = false, message = "Không thể đăng ký hoạt động" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering activity");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi đăng ký hoạt động" });
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> HuyDangKy(int id)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.DeleteAsync($"api/DangKy/{id}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Hủy đăng ký thành công" });
                }

                return Json(new { success = false, message = "Không thể hủy đăng ký hoạt động" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error canceling registration");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi hủy đăng ký hoạt động" });
            }
        }
    }
} 