using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using HTSV.FE.Models.Common;
using HTSV.FE.Models.NguoiDung;
using HTSV.FE.Extensions;
using HTSV.FE.Models.Auth;

namespace HTSV.FE.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<ProfileController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProfileController(
            IHttpClientFactory clientFactory,
            ILogger<ProfileController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
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
                if (!AddAuthenticationHeader(client))
                {
                    return RedirectToAction("Login", "Account");
                }

                var userId = User.GetUserId();
                var response = await client.GetAsync($"api/NguoiDung/{userId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<NguoiDungViewModel>>(content, _jsonOptions);
                    if (result?.Success == true)
                    {
                        return View(result.Data);
                    }
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user profile");
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "Không có file được chọn" });
                }

                var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "avatar");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var fileName = $"{timestamp}_{file.FileName}";
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var userId = User.GetUserId();
                var client = _clientFactory.CreateClient("BE");
                var token = HttpContext.Session.GetString("TokenUser");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var updateData = new { anhDaiDien = $"/images/avatar/{fileName}" };
                var response = await client.PutAsJsonAsync($"api/NguoiDung/{userId}", updateData);

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, filePath = $"/images/avatar/{fileName}" });
                }

                return Json(new { success = false, message = "Không thể cập nhật ảnh đại diện" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading avatar");
                return Json(new { success = false, message = "Lỗi khi upload ảnh đại diện" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromBody] UpdateMatKhauModel model)
        {
            try
            {
                var userId = User.GetUserId();
                var client = _clientFactory.CreateClient("BE");
                var token = HttpContext.Session.GetString("TokenUser");
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await client.PutAsJsonAsync($"api/NguoiDung/{userId}/mat-khau", model);
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ApiResponse<bool>>(content, _jsonOptions);

                return Json(new { success = response.IsSuccessStatusCode, message = result?.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return Json(new { success = false, message = "Lỗi khi đổi mật khẩu" });
            }
        }
    }
} 