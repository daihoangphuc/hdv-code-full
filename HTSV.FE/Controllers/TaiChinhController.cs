using HTSV.FE.Models.Common;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using HTSV.FE.Models.TaiChinh;
using HTSV.FE.Extensions;
using HTSV.FE.Models.Auth;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;

namespace HTSV.FE.Controllers
{
    [Authorize]
    public class TaiChinhController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<TaiChinhController> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public TaiChinhController(IHttpClientFactory clientFactory, ILogger<TaiChinhController> logger)
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

                string url = $"api/TaiChinh?PageIndex={page}&PageSize={pageSize}";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    url += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
                }

                _logger.LogInformation($"Calling API with URL: {url}");
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error calling API: {response.StatusCode}");
                    return View(new PaginatedList<TaiChinhViewModel>());
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedList<TaiChinhViewModel>>>(content, _jsonOptions);

                if (apiResponse?.Success == true)
                {
                    return View(apiResponse.Data);
                }

                _logger.LogError($"API returned error: {apiResponse?.Error}");
                return View(new PaginatedList<TaiChinhViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index action");
                return View(new PaginatedList<TaiChinhViewModel>());
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,CanBo")]
        public async Task<IActionResult> Create([FromBody] CreateTaiChinhModel model)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.PostAsJsonAsync("api/TaiChinh", model);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<TaiChinhViewModel>>(content, _jsonOptions);
                    return Json(new { success = true, data = result?.Data });
                }

                return Json(new { success = false, message = "Không thể tạo giao dịch" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tai chinh");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tạo giao dịch" });
            }
        }

        [HttpPut]
        [Authorize(Roles = "Admin,CanBo")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTaiChinhModel model)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.PutAsJsonAsync($"api/TaiChinh/{id}", model);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<TaiChinhViewModel>>(content, _jsonOptions);
                    return Json(new { success = true, data = result?.Data });
                }

                return Json(new { success = false, message = "Không thể cập nhật giao dịch" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tai chinh");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật giao dịch" });
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

                var response = await client.DeleteAsync($"api/TaiChinh/{id}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Xóa giao dịch thành công" });
                }

                return Json(new { success = false, message = "Không thể xóa giao dịch" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tai chinh");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa giao dịch" });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,CanBo")]
        public async Task<IActionResult> ThongKe(DateTime? tuNgay = null, DateTime? denNgay = null)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                string url = "api/TaiChinh/thong-ke";
                if (tuNgay.HasValue)
                    url += $"?tuNgay={tuNgay.Value:yyyy-MM-dd}";
                if (denNgay.HasValue)
                    url += $"{(tuNgay.HasValue ? "&" : "?")}denNgay={denNgay.Value:yyyy-MM-dd}";

                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<ThongKeTaiChinhModel>>(content, _jsonOptions);
                    return Json(new { success = true, data = result?.Data });
                }

                return Json(new { success = false, message = "Không thể lấy thống kê" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting thong ke");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi lấy thống kê" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetByNguoiDongTien(string maSinhVien)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.GetAsync($"api/TaiChinh/nguoi-dong-tien?maSinhVien={Uri.EscapeDataString(maSinhVien)}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<List<TaiChinhViewModel>>>(content, _jsonOptions);
                    return Json(new { success = true, data = result?.Data });
                }

                return Json(new { success = false, message = "Không thể lấy danh sách giao dịch" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tai chinh by nguoi dong tien");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi lấy danh sách giao dịch" });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,CanBo")]
        public async Task<IActionResult> GetByNguoiThucHien(string maSinhVien)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.GetAsync($"api/TaiChinh/nguoi-thuc-hien?maSinhVien={Uri.EscapeDataString(maSinhVien)}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<List<TaiChinhViewModel>>>(content, _jsonOptions);
                    return Json(new { success = true, data = result?.Data });
                }

                return Json(new { success = false, message = "Không thể lấy danh sách giao dịch" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tai chinh by nguoi thuc hien");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi lấy danh sách giao dịch" });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,CanBo")]
        public async Task<IActionResult> GetByHoatDong(int hoatDongId)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.GetAsync($"api/TaiChinh/hoat-dong/{hoatDongId}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<List<TaiChinhViewModel>>>(content, _jsonOptions);
                    return Json(new { success = true, data = result?.Data });
                }

                return Json(new { success = false, message = "Không thể lấy danh sách giao dịch" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tai chinh by hoat dong");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi lấy danh sách giao dịch" });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,CanBo")]
        public async Task<IActionResult> GetByLoaiGiaoDich(byte loaiGiaoDich)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.GetAsync($"api/TaiChinh/loai-giao-dich/{loaiGiaoDich}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<List<TaiChinhViewModel>>>(content, _jsonOptions);
                    return Json(new { success = true, data = result?.Data });
                }

                return Json(new { success = false, message = "Không thể lấy danh sách giao dịch" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tai chinh by loai giao dich");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi lấy danh sách giao dịch" });
            }
        }
    }
} 