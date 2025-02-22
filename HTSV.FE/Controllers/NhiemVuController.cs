using HTSV.FE.Models.Common;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using HTSV.FE.Models.NhiemVu;
using HTSV.FE.Extensions;
using HTSV.FE.Models.Auth;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;

namespace HTSV.FE.Controllers
{
    [Authorize]
    public class NhiemVuController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<NhiemVuController> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public NhiemVuController(IHttpClientFactory clientFactory, ILogger<NhiemVuController> logger)
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
                if (!AddAuthenticationHeader(client))
                {
                    return RedirectToAction("Login", "Account");
                }

                string url = $"api/NhiemVu?PageIndex={page}&PageSize={pageSize}";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    url += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
                }

                _logger.LogInformation($"Calling API with URL: {url}");
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error calling API: {response.StatusCode}");
                    return View(new PaginatedList<NhiemVuViewModel>());
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedList<NhiemVuViewModel>>>(content, _jsonOptions);

                if (apiResponse?.Success == true)
                {
                    return View(apiResponse.Data);
                }

                _logger.LogError($"API returned error: {apiResponse?.Error}");
                return View(new PaginatedList<NhiemVuViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index action");
                return View(new PaginatedList<NhiemVuViewModel>());
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,CanBo")]
        public async Task<IActionResult> Create([FromBody] CreateNhiemVuModel model)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.PostAsJsonAsync("api/NhiemVu", model);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<NhiemVuViewModel>>(content, _jsonOptions);
                    return Json(new { success = true, data = result?.Data });
                }

                return Json(new { success = false, message = "Không thể tạo nhiệm vụ" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating nhiem vu");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tạo nhiệm vụ" });
            }
        }

        [HttpPut]
        [Authorize(Roles = "Admin,CanBo")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateNhiemVuModel model)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.PutAsJsonAsync($"api/NhiemVu/{id}", model);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<NhiemVuViewModel>>(content, _jsonOptions);
                    return Json(new { success = true, data = result?.Data });
                }

                return Json(new { success = false, message = "Không thể cập nhật nhiệm vụ" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating nhiem vu");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật nhiệm vụ" });
            }
        }

        [HttpPut]
        [Authorize(Roles = "Admin,CanBo")]
        public async Task<IActionResult> UpdateTrangThai(int id, [FromBody] byte trangThai)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.PutAsJsonAsync($"api/NhiemVu/{id}/trang-thai", trangThai);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
                }

                return Json(new { success = false, message = "Không thể cập nhật trạng thái nhiệm vụ" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating nhiem vu status");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật trạng thái nhiệm vụ" });
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Admin,CanBo")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.DeleteAsync($"api/NhiemVu/{id}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Xóa nhiệm vụ thành công" });
                }

                return Json(new { success = false, message = "Không thể xóa nhiệm vụ" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting nhiem vu");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa nhiệm vụ" });
            }
        }
    }
} 