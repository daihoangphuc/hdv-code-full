using HTSV.FE.Models.Common;
using HTSV.FE.Models.TinTuc;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using HTSV.FE.Extensions;
using HTSV.FE.Models.Auth;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;

namespace HTSV.FE.Controllers
{

    public class TinTucController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<TinTucController> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public TinTucController(IHttpClientFactory clientFactory, ILogger<TinTucController> logger)
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

                string url = $"api/TinTuc?PageIndex={page}&PageSize={pageSize}";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    url += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
                }

                _logger.LogInformation($"Calling API with URL: {url}");
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error calling API: {response.StatusCode}");
                    return View(new PaginatedList<TinTucViewModel>());
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedList<TinTucViewModel>>>(content, _jsonOptions);

                if (apiResponse?.Success == true)
                {
                    return View(apiResponse.Data);
                }

                _logger.LogError($"API returned error: {apiResponse?.Error}");
                return View(new PaginatedList<TinTucViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index action");
                return View(new PaginatedList<TinTucViewModel>());
            }
        }

        //Viet phuong thuc GetById
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.GetAsync($"api/TinTuc/{id}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<TinTucViewModel>>(content, _jsonOptions);
                    return Json(new { success = true, data = result?.Data });
                }

                return Json(new { success = false, message = "Không thể lấy thông tin tin tức" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tin tuc by id");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi lấy thông tin tin tức" });
            }
        }


        [HttpPost]
        [Authorize(Roles = "Admin,CanBo")]
        public async Task<IActionResult> Create([FromBody] CreateTinTucModel model)
        {
            try
            {
                if (model == null)
                {
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
                }

                if (string.IsNullOrWhiteSpace(model.TieuDe))
                {
                    return Json(new { success = false, message = "Tiêu đề không được để trống" });
                }

                if (string.IsNullOrWhiteSpace(model.NoiDung))
                {
                    return Json(new { success = false, message = "Nội dung không được để trống" });
                }

                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.PostAsJsonAsync("api/TinTuc", model);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<TinTucViewModel>>(content, _jsonOptions);
                    if (result?.Success == true)
                    {
                        return Json(new { success = true, data = result.Data });
                    }
                    return Json(new { success = false, message = result?.Message ?? "Không thể tạo tin tức" });
                }

                _logger.LogError($"API Error: {response.StatusCode}, Content: {content}");
                var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, _jsonOptions);
                return Json(new { success = false, message = errorResponse?.Message ?? "Không thể tạo tin tức" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tin tuc");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi tạo tin tức" });
            }
        }

        [HttpPut]
        [Authorize(Roles = "Admin,CanBo")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateTinTucModel model)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.PutAsJsonAsync($"api/TinTuc/{id}", model);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<TinTucViewModel>>(content, _jsonOptions);
                    return Json(new { success = true, data = result?.Data });
                }

                return Json(new { success = false, message = "Không thể cập nhật tin tức" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tin tuc");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật tin tức" });
            }
        }

        [HttpPut]
        [Authorize(Roles = "Admin,CanBo")]
        public async Task<IActionResult> UpdateTrangThai(int id, [FromBody] bool trangThai)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.PutAsJsonAsync($"api/TinTuc/{id}/trang-thai", trangThai);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<bool>>(content, _jsonOptions);
                    return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
                }

                return Json(new { success = false, message = "Không thể cập nhật trạng thái tin tức" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tin tuc status");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật trạng thái tin tức" });
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

                // Lấy thông tin tin tức trước khi xóa
                var getResponse = await client.GetAsync($"api/TinTuc/{id}");
                if (getResponse.IsSuccessStatusCode)
                {
                    var content = await getResponse.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<TinTucViewModel>>(content, _jsonOptions);
                    if (result?.Success == true && !string.IsNullOrEmpty(result.Data?.FileDinhKem))
                    {
                        // Xóa file đính kèm
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", result.Data.FileDinhKem.TrimStart('/'));
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                }

                // Xóa tin tức
                var response = await client.DeleteAsync($"api/TinTuc/{id}");
                var deleteContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "Xóa tin tức thành công" });
                }

                return Json(new { success = false, message = "Không thể xóa tin tức" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tin tuc");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa tin tức" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPublished()
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.GetAsync("api/TinTuc/da-xuat-ban");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<List<TinTucViewModel>>>(content, _jsonOptions);
                    return Json(new { success = true, data = result?.Data });
                }

                return Json(new { success = false, message = "Không thể lấy danh sách tin tức đã xuất bản" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting published tin tuc");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi lấy danh sách tin tức đã xuất bản" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,CanBo")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return Json(new { success = false, message = "Vui lòng chọn file" });

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                    return Json(new { success = false, message = "Định dạng file không được hỗ trợ" });

                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "tintuc");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return Json(new { success = true, data = $"/images/tintuc/{fileName}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return Json(new { success = false, message = "Có lỗi xảy ra khi upload file" });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                // Không cần check authentication
                var response = await client.GetAsync($"api/TinTuc/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error calling API: {response.StatusCode}");
                    return NotFound();
                }

                var content = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<TinTucViewModel>>(content, _jsonOptions);

                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
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
    }
} 