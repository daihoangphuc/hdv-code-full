using HTSV.FE.Attributes;
using HTSV.FE.Extensions;
using HTSV.FE.Models.Admin;
using HTSV.FE.Models.Common;
using HTSV.FE.Models.HoatDong;
using HTSV.FE.Models.NguoiDung;
using HTSV.FE.Models.TinTuc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
namespace HTSV.FE.Controllers
{
    [Attributes.Authorize("Admin")]
    public class AdminController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IHttpClientFactory clientFactory, ILogger<AdminController> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
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
        public async Task<IActionResult> Index()
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                var response = await client.GetAsync("/api/thongke");
                if (!AddAuthenticationHeader(client)) return RedirectToAction("Login", "Account");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<ThongKeViewModel>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.Success == true && apiResponse.Data != null)
                    {
                        return View(apiResponse.Data);
                    }
                    
                    TempData["Error"] = apiResponse?.Message ?? "Không thể đọc dữ liệu từ server.";
                    return View(new ThongKeViewModel());
                }

                TempData["Error"] = "Không thể tải dữ liệu thống kê. Vui lòng thử lại sau.";
                return View(new ThongKeViewModel());
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
                return View(new ThongKeViewModel());
            }
        }

        #region Quản lý người dùng
        public async Task<IActionResult> Users(int page = 1, string searchTerm = "", string role = "")
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                var url = $"/api/nguoidung?page={page}&pageSize=10";
                if (!string.IsNullOrEmpty(searchTerm))
                    url += $"&searchTerm={searchTerm}";
                if (!string.IsNullOrEmpty(role))
                    url += $"&role={role}";

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedList<NguoiDungViewModel>>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.Success == true)
                    {
                        return View(apiResponse.Data);
                    }
                    TempData["Error"] = apiResponse?.Message;
                }
                TempData["Error"] = "Không thể tải danh sách người dùng";
                return View(new PaginatedList<NguoiDungViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Users action");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách người dùng";
                return View(new PaginatedList<NguoiDungViewModel>());
            }
        }

        #endregion

        #region Quản lý hoạt động
        public async Task<IActionResult> Activities(int page = 1, string? searchTerm = null, string? status = null)
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                var url = $"/api/hoatdong?page={page}&pageSize=10";
                if (!string.IsNullOrEmpty(searchTerm))
                    url += $"&searchTerm={searchTerm}";
                if (!string.IsNullOrEmpty(status))
                    url += $"&status={status}";

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    using (JsonDocument document = JsonDocument.Parse(content))
                    {
                        var root = document.RootElement;
                        var success = root.GetProperty("success").GetBoolean();
                        var message = root.GetProperty("message").ValueKind == JsonValueKind.Null ? null : root.GetProperty("message").GetString();
                        
                        if (!success)
                        {
                            TempData["Error"] = message ?? "Không thể đọc dữ liệu từ server.";
                            return View(new List<HoatDongViewModel>());
                        }

                        var data = root.GetProperty("data");
                        var items = data.GetProperty("items");
                        
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };

                        var hoatDongList = JsonSerializer.Deserialize<List<HoatDongViewModel>>(items.ToString(), options);

                        ViewBag.PageIndex = data.GetProperty("pageIndex").GetInt32();
                        ViewBag.TotalPages = data.GetProperty("totalPages").GetInt32();
                        ViewBag.HasPreviousPage = data.GetProperty("hasPreviousPage").GetBoolean();
                        ViewBag.HasNextPage = data.GetProperty("hasNextPage").GetBoolean();
                        ViewBag.TotalCount = data.GetProperty("totalCount").GetInt32();
                        ViewBag.PageSize = data.GetProperty("pageSize").GetInt32();
                        ViewBag.SearchTerm = searchTerm;
                        ViewBag.Status = status;

                        return View(hoatDongList ?? new List<HoatDongViewModel>());
                    }
                }

                TempData["Error"] = "Không thể tải danh sách hoạt động.";
                return View(new List<HoatDongViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Activities action");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách hoạt động";
                return View(new List<HoatDongViewModel>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateActivity([FromBody] CreateHoatDongModel model)
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                var response = await client.PostAsJsonAsync("/api/hoatdong", model);
                var content = await response.Content.ReadAsStringAsync();
                
                return Json(new { success = response.IsSuccessStatusCode, content });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating activity");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tạo hoạt động" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetActivity(int id)
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                var response = await client.GetAsync($"/api/hoatdong/{id}");
                var content = await response.Content.ReadAsStringAsync();
                
                return Json(new { success = response.IsSuccessStatusCode, content });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting activity");
                return Json(new { success = false, message = "Có lỗi xảy ra khi lấy thông tin hoạt động" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateActivity(int id, [FromBody] UpdateHoatDongModel model)
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                var response = await client.PutAsJsonAsync($"/api/hoatdong/{id}", model);
                var content = await response.Content.ReadAsStringAsync();
                
                if (string.IsNullOrEmpty(content))
                {
                    return Json(new { 
                        success = false, 
                        message = "Không nhận được phản hồi từ server" 
                    });
                }

                var result = JsonSerializer.Deserialize<ApiResponse<object>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (!response.IsSuccessStatusCode)
                {
                    return Json(new { 
                        success = false, 
                        message = result?.Message ?? $"Lỗi {response.StatusCode}: Không thể cập nhật hoạt động"
                    });
                }

                return Json(new { 
                    success = result?.Success ?? false, 
                    message = result?.Message,
                    data = result?.Data 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating activity");
                return Json(new { 
                    success = false, 
                    message = "Có lỗi xảy ra khi cập nhật hoạt động" 
                });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteActivity(int id)
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                var response = await client.DeleteAsync($"/api/hoatdong/{id}");
                var content = await response.Content.ReadAsStringAsync();
                
                return Json(new { success = response.IsSuccessStatusCode, content });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting activity");
                return Json(new { success = false, message = "Có lỗi xảy ra khi xóa hoạt động" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateActivityStatus([FromBody] CapNhatTrangThaiModel model)
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                var response = await client.PutAsJsonAsync($"/api/hoatdong/{model.HoatDongId}/status", model);
                var content = await response.Content.ReadAsStringAsync();
                
                return Json(new { success = response.IsSuccessStatusCode, content });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating activity status");
                return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật trạng thái hoạt động" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadMinhChung([FromBody] UploadMinhChungModel model)
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                var response = await client.PostAsJsonAsync($"/api/hoatdong/{model.HoatDongId}/minhchung", model);
                var content = await response.Content.ReadAsStringAsync();
                
                return Json(new { success = response.IsSuccessStatusCode, content });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading minh chung");
                return Json(new { success = false, message = "Có lỗi xảy ra khi tải lên minh chứng" });
            }
        }
        #endregion

        #region Quản lý tin tức
        public async Task<IActionResult> News(int page = 1, string searchTerm = "", string category = "")
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                var url = $"/api/tintuc?page={page}&pageSize=10";
                if (!string.IsNullOrEmpty(searchTerm))
                    url += $"&searchTerm={searchTerm}";
                if (!string.IsNullOrEmpty(category))
                    url += $"&category={category}";

                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedList<TinTucViewModel>>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.Success == true)
                    {
                        return View(apiResponse.Data);
                    }
                    TempData["Error"] = apiResponse?.Message;
                }
                TempData["Error"] = "Không thể tải danh sách tin tức";
                return View(new PaginatedList<TinTucViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in News action");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách tin tức";
                return View(new PaginatedList<TinTucViewModel>());
            }
        }
        #endregion
    }
} 