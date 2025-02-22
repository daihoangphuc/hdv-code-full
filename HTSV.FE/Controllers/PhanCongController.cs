using HTSV.FE.Models.Common;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using HTSV.FE.Models.PhanCong;
using HTSV.FE.Extensions;
using HTSV.FE.Models.Auth;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;

namespace HTSV.FE.Controllers
{
    [Authorize]
    public class PhanCongController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<PhanCongController> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public PhanCongController(IHttpClientFactory clientFactory, ILogger<PhanCongController> logger)
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

                string url = $"api/PhanCong?PageIndex={page}&PageSize={pageSize}&includeDetails=true";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    url += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
                }

                _logger.LogInformation($"Calling API with URL: {url}");
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error calling API: {response.StatusCode}");
                    return View(new PaginatedList<PhanCongViewModel>());
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedList<PhanCongViewModel>>>(content, _jsonOptions);

                if (apiResponse?.Success == true)
                {
                    return View(apiResponse.Data);
                }

                _logger.LogError($"API returned error: {apiResponse?.Error}");
                return View(new PaginatedList<PhanCongViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index action");
                return View(new PaginatedList<PhanCongViewModel>());
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,CanBo")]
        public async Task<IActionResult> Create([FromBody] CreatePhanCongModel model)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.PostAsJsonAsync("api/PhanCong", model);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<PhanCongViewModel>>(content, _jsonOptions);
                    return Json(new { success = true, data = result?.Data });
                }

                return Json(new { success = false, message = "Không thể tạo phân công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating phan cong");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tạo phân công" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,CanBo")]
        public async Task<IActionResult> PhanCongNhom([FromBody] PhanCongNhomModel model)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.PostAsJsonAsync("api/PhanCong/phan-cong-nhom", model);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<bool>>(content, _jsonOptions);
                    return Json(new { success = true, message = "Phân công nhóm thành công" });
                }

                return Json(new { success = false, message = "Không thể phân công nhóm" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating phan cong nhom");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi phân công nhóm" });
            }
        }

        [HttpPut]
        [Authorize(Roles = "Admin,CanBo")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePhanCongModel model)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.PutAsJsonAsync($"api/PhanCong/{id}", model);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<PhanCongViewModel>>(content, _jsonOptions);
                    return Json(new { success = true, data = result?.Data });
                }

                return Json(new { success = false, message = "Không thể cập nhật phân công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating phan cong");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật phân công" });
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

                var response = await client.DeleteAsync($"api/PhanCong/{id}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Xóa phân công thành công" });
                }

                return Json(new { success = false, message = "Không thể xóa phân công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting phan cong");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa phân công" });
            }
        }
    }
} 