using HTSV.FE.Models.Common;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using HTSV.FE.Models.ChucVu;
using HTSV.FE.Extensions;
using HTSV.FE.Models.Auth;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;

namespace HTSV.FE.Controllers
{
    [Authorize]
    public class ChucVuController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<ChucVuController> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ChucVuController(IHttpClientFactory clientFactory, ILogger<ChucVuController> logger)
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

                string url = $"api/ChucVu?PageIndex={page}&PageSize={pageSize}";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    url += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
                }

                _logger.LogInformation($"Calling API with URL: {url}");
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error calling API: {response.StatusCode}");
                    return View(new PaginatedList<ChucVuViewModel>());
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedList<ChucVuViewModel>>>(content, _jsonOptions);

                if (apiResponse?.Success == true)
                {
                    return View(apiResponse.Data);
                }

                _logger.LogError($"API returned error: {apiResponse?.Error}");
                return View(new PaginatedList<ChucVuViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index action");
                return View(new PaginatedList<ChucVuViewModel>());
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateChucVuModel model)
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.PostAsJsonAsync("/api/ChucVu", model);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<ChucVuViewModel>>(content, _jsonOptions);
                    return Json(new { success = true, data = result?.Data });
                }

                return Json(new { success = false, message = "Không thể tạo chức vụ" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating chuc vu");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tạo chức vụ" });
            }
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateChucVuModel model)
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.PutAsJsonAsync($"/api/ChucVu/{id}", model);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<ChucVuViewModel>>(content, _jsonOptions);
                    return Json(new { success = true, data = result?.Data });
                }

                return Json(new { success = false, message = "Không thể cập nhật chức vụ" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating chuc vu");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật chức vụ" });
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

                var response = await client.DeleteAsync($"/api/ChucVu/{id}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Không thể xóa chức vụ" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting chuc vu");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa chức vụ" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.GetAsync("api/ChucVu?PageSize=100");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<PaginatedList<ChucVuViewModel>>>(content, _jsonOptions);
                    if (result?.Success == true)
                    {
                        return Json(new { success = true, data = result.Data.Items });
                    }
                }

                return Json(new { success = false, message = "Không thể lấy danh sách chức vụ" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all chuc vu");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi lấy danh sách chức vụ" });
            }
        }
    }
} 