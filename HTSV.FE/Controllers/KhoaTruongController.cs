using HTSV.FE.Models.Common;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using HTSV.FE.Models.KhoaTruong;
using HTSV.FE.Extensions;
using HTSV.FE.Models.Auth;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;

namespace HTSV.FE.Controllers
{
    [Authorize]
    public class KhoaTruongController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<KhoaTruongController> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public KhoaTruongController(IHttpClientFactory clientFactory, ILogger<KhoaTruongController> logger)
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

                string url = $"api/KhoaTruong?PageIndex={page}&PageSize={pageSize}";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    url += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
                }

                _logger.LogInformation($"Calling API with URL: {url}");
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error calling API: {response.StatusCode}");
                    return View(new PaginatedList<KhoaTruongViewModel>());
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedList<KhoaTruongViewModel>>>(content, _jsonOptions);

                if (apiResponse?.Success == true)
                {
                    return View(apiResponse.Data);
                }

                _logger.LogError($"API returned error: {apiResponse?.Error}");
                return View(new PaginatedList<KhoaTruongViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index action");
                return View(new PaginatedList<KhoaTruongViewModel>());
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateKhoaTruongModel model)
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.PostAsJsonAsync("/api/KhoaTruong", model);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<KhoaTruongViewModel>>(content, _jsonOptions);
                    return Json(new { success = true, data = result?.Data });
                }

                return Json(new { success = false, message = "Không thể tạo khoa/trường" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating khoa truong");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tạo khoa/trường" });
            }
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateKhoaTruongModel model)
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.PutAsJsonAsync($"/api/KhoaTruong/{id}", model);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<KhoaTruongViewModel>>(content, _jsonOptions);
                    return Json(new { success = true, data = result?.Data });
                }

                return Json(new { success = false, message = "Không thể cập nhật khoa/trường" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating khoa truong");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật khoa/trường" });
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.DeleteAsync($"/api/KhoaTruong/{id}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Không thể xóa khoa/trường" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting khoa truong");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa khoa/trường" });
            }
        }
    }
} 