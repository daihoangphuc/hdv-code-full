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
                    _logger.LogInformation("Adding token to request header");
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {loginInfo.Token}");
                    return true;
                }
                else 
                {
                    _logger.LogWarning("No token found in session");
                    return false;
                }
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
                if (!AddAuthenticationHeader(client)) return RedirectToAction("Login", "Account");

                // Cập nhật trạng thái hoạt động trước khi lấy danh sách
                await client.PostAsync("api/HoatDong/update-status", null);

                string url = $"api/HoatDong?PageIndex={page}&PageSize={pageSize}";
                
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    url += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
                }

                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error calling API: {response.StatusCode}");
                    return View(new PaginatedList<HoatDongViewModel>());
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedList<HoatDongViewModel>>>(content, _jsonOptions);

                if (apiResponse?.Success == true)
                {
                    return View(apiResponse.Data);
                }

                _logger.LogError($"API returned error: {apiResponse?.Error}");
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
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return RedirectToAction("Login", "Account");
                }

                var response = await client.GetAsync($"api/HoatDong/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error calling API: {response.StatusCode}");
                    return NotFound();
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<HoatDongViewModel>>(content, _jsonOptions);

                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    // Lấy mã sinh viên từ session
                    var maSinhVien = HttpContext.Session.GetString("MaSinhVien");
                    
                    // Kiểm tra đăng ký của người dùng
                    var dangKyResponse = await client.GetAsync($"api/DangKy/nguoi-dang-ky?maSinhVien={maSinhVien}");
                    var dangKyContent = await dangKyResponse.Content.ReadAsStringAsync();
                    var dangKyApiResponse = JsonSerializer.Deserialize<ApiResponse<List<DangKyDTO>>>(dangKyContent, _jsonOptions);

                    if (dangKyApiResponse?.Success == true && dangKyApiResponse.Data != null)
                    {
                        // Tìm đăng ký cho hoạt động hiện tại
                        var dangKy = dangKyApiResponse.Data.FirstOrDefault(dk => dk.HoatDongId == id);
                        ViewBag.DaDangKy = dangKy != null;
                        ViewBag.DangKyId = dangKy?.Id;
                    }
                    else
                    {
                        ViewBag.DaDangKy = false;
                        ViewBag.DangKyId = null;
                    }

                    return View(apiResponse.Data);
                }

                _logger.LogError($"API returned error: {apiResponse?.Error}");
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Details action");
                return NotFound();
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
                var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Phiên đăng nhập đã hết hạn" });
                }
                
                _logger.LogInformation($"Creating activity: {JsonSerializer.Serialize(model)}");
                var response = await client.PostAsJsonAsync("/api/hoatdong", model);
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Create activity response: {content}");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<HoatDongViewModel>>(content, _jsonOptions);

                    if (result?.Success == true)
                    {
                        return Json(new { success = true, message = "Tạo hoạt động thành công", data = result.Data });
                    }
                    return Json(new { success = false, message = result?.Message ?? "Không thể tạo hoạt động" });
                }
                
                return Json(new { success = false, message = $"Lỗi {response.StatusCode}: {content}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating activity");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo hoạt động" });
            }
        }

        [HttpPut]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateHoatDongModel model)
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                AddAuthenticationHeader(client);
                
                _logger.LogInformation($"Updating activity {id}: {JsonSerializer.Serialize(model)}");
                var response = await client.PutAsJsonAsync($"/api/hoatdong/{id}", model);
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Update activity response: {content}");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<HoatDongViewModel>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (result?.Success == true)
                    {
                        return Json(new { success = true, message = "Cập nhật hoạt động thành công", data = result.Data });
                    }
                    return Json(new { success = false, message = result?.Message ?? "Không thể cập nhật hoạt động" });
                }
                
                return Json(new { success = false, message = $"Lỗi {response.StatusCode}: {content}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating activity");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật hoạt động" });
            }
        }

        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus([FromBody] CapNhatTrangThaiModel model)
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                AddAuthenticationHeader(client);
                
                var response = await client.PutAsJsonAsync($"/api/hoatdong/{model.HoatDongId}/trang-thai", model);
                var content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<bool>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (result?.Success == true)
                    {
                        return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
                    }
                }
                
                return Json(new { success = false, message = "Không thể cập nhật trạng thái" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating activity status");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái" });
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
                var client = _clientFactory.CreateClient("BE");
                AddAuthenticationHeader(client);
                
                var response = await client.DeleteAsync($"/api/hoatdong/{id}");
                var content = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<bool>>(content,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (result?.Success == true)
                    {
                        return Json(new { success = true, message = "Xóa hoạt động thành công" });
                    }
                }
                
                return Json(new { success = false, message = "Không thể xóa hoạt động" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting activity");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa hoạt động" });
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Export()
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client)) return RedirectToAction("Login", "Account");

                var response = await client.GetAsync("api/HoatDong/export");
                if (!response.IsSuccessStatusCode)
                {
                    TempData["Error"] = "Có lỗi xảy ra khi xuất file Excel";
                    return RedirectToAction(nameof(Index));
                }

                var content = await response.Content.ReadAsByteArrayAsync();
                return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DanhSachHoatDong.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Export action");
                TempData["Error"] = "Có lỗi xảy ra khi xuất file Excel";
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