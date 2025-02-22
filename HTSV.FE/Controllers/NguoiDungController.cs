using HTSV.FE.Models.Common;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using HTSV.FE.Models.NguoiDung;
using HTSV.FE.Extensions;
using HTSV.FE.Models.Auth;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HTSV.FE.Controllers
{
    [Authorize(Roles="Admin")]
    public class NguoiDungController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<NguoiDungController> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IWebHostEnvironment _environment;
        private readonly string _apiBaseUrl;

        public NguoiDungController(
            IHttpClientFactory clientFactory, 
            ILogger<NguoiDungController> logger,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _environment = environment;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
                WriteIndented = true
            };
            _apiBaseUrl = configuration["ApiSettings:BaseUrl"];
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
                client.BaseAddress = new Uri(_apiBaseUrl);
                
                if (!AddAuthenticationHeader(client))
                {
/*                    TempData["ErrorMessage"] = "Vui lòng đăng nhập lại để tiếp tục.";*/
                    return RedirectToAction("Login", "Account");
                }

                string url = $"api/NguoiDung?PageIndex={page}&PageSize={pageSize}";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    url += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
                }

                _logger.LogInformation($"Calling API with URL: {_apiBaseUrl}{url}");
                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();
                
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        TempData["ErrorMessage"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
                        return RedirectToAction("Login", "Account");
                    }
                    
                    _logger.LogError($"Error calling API: {response.StatusCode}, Content: {content}");
                    TempData["ErrorMessage"] = "Không thể tải danh sách người dùng. Vui lòng thử lại sau.";
                    return View(new PaginatedList<NguoiDungViewModel>());
                }

                try 
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedList<NguoiDungViewModel>>>(content, _jsonOptions);
                    _logger.LogInformation($"Successfully deserialized API response: {JsonSerializer.Serialize(apiResponse, _jsonOptions)}");

                    if (apiResponse?.Success == true)
                    {
                        if (apiResponse.Data?.Items == null || !apiResponse.Data.Items.Any())
                        {
                            TempData["InfoMessage"] = "Không có dữ liệu người dùng.";
                        }
                        return View(apiResponse.Data);
                    }

                    _logger.LogError($"API returned error: {apiResponse?.Error}");
                    TempData["ErrorMessage"] = apiResponse?.Error ?? "Có lỗi xảy ra khi tải dữ liệu.";
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, $"Error deserializing API response. Content: {content}");
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý dữ liệu từ server.";
                }
                
                return View(new PaginatedList<NguoiDungViewModel>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index action");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi trong quá trình xử lý. Vui lòng thử lại sau.";
                return View(new PaginatedList<NguoiDungViewModel>());
            }
        }

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

                var response = await client.GetAsync($"api/NguoiDung/{id}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<NguoiDungViewModel>>(content, _jsonOptions);
                    return Json(result);
                }

                return Json(new { success = false, message = "Không thể lấy thông tin người dùng" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by id");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi lấy thông tin người dùng" });
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateNguoiDungViewModel model)
        {
            if (ModelState.IsValid)
            {
                string uniqueFileName = null;
                if (model.AnhDaiDienFile != null && model.AnhDaiDienFile.Length > 0)
                {
                    try
                    {
                        // Kiểm tra định dạng file
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                        var fileExtension = Path.GetExtension(model.AnhDaiDienFile.FileName).ToLowerInvariant();
                        if (!allowedExtensions.Contains(fileExtension))
                        {
                            ModelState.AddModelError("AnhDaiDienFile", "Chỉ chấp nhận file ảnh .jpg, .jpeg, .png");
                            return View(model);
                        }

                        // Kiểm tra kích thước file (tối đa 2MB)
                        if (model.AnhDaiDienFile.Length > 2 * 1024 * 1024)
                        {
                            ModelState.AddModelError("AnhDaiDienFile", "Kích thước file không được vượt quá 2MB");
                            return View(model);
                        }

                        string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "avatar");
                        Directory.CreateDirectory(uploadsFolder);
                        
                        uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.AnhDaiDienFile.CopyToAsync(fileStream);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error uploading avatar");
                        ModelState.AddModelError("AnhDaiDienFile", $"Lỗi khi upload ảnh: {ex.Message}");
                        return View(model);
                    }
                }

                try
                {
                    using var client = _clientFactory.CreateClient("BE");
                    if (!AddAuthenticationHeader(client))
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    var createData = new
                    {
                        model.MaSoSinhVien,
                        model.Email,
                        model.MatKhau,
                        model.HoTen,
                        model.SoDienThoai,
                        model.LopHocId,
                        model.ChucVuId,
                        AnhDaiDien = uniqueFileName != null ? $"/images/avatar/{uniqueFileName}" : null
                    };

                    _logger.LogInformation("Creating user with data: {@createData}", createData);
                    var response = await client.PostAsJsonAsync("api/NguoiDung", createData);
                    var content = await response.Content.ReadAsStringAsync();
                    
                    if (response.IsSuccessStatusCode)
                    {
                        var result = JsonSerializer.Deserialize<ApiResponse<NguoiDungViewModel>>(content, _jsonOptions);
                        if (result?.Success == true)
                        {
                            TempData["SuccessMessage"] = "Tạo người dùng thành công!";
                            return RedirectToAction(nameof(Index));
                        }
                    }

                    // Nếu có lỗi từ API, parse và hiển thị lỗi
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content);
                        if (errorResponse?.Errors != null)
                        {
                            foreach (var error in errorResponse.Errors)
                            {
                                foreach (var message in error.Value)
                                {
                                    ModelState.AddModelError(error.Key, message);
                                }
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", "Không thể tạo người dùng. Vui lòng thử lại sau.");
                        }
                    }
                    catch
                    {
                        ModelState.AddModelError("", "Có lỗi xảy ra khi tạo người dùng");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating user");
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi tạo người dùng. Vui lòng thử lại sau.");
                }
            }

            // Nếu có lỗi, load lại danh sách lớp và chức vụ
            return View(model);
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateNguoiDungModel model)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                client.BaseAddress = new Uri(_apiBaseUrl);
                
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                _logger.LogInformation($"Updating user {id} with data: {JsonSerializer.Serialize(model)}");

                var response = await client.PutAsJsonAsync($"api/NguoiDung/{id}", model);
                var content = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation($"Update response: {content}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<NguoiDungViewModel>>(content, _jsonOptions);
                    if (result?.Success == true)
                    {
                        return Json(new { success = true, data = result.Data });
                    }
                }

                // Try to parse error response
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, _jsonOptions);
                    return Json(new { success = false, message = errorResponse?.Error ?? "Không thể cập nhật người dùng" });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing error response");
                    return Json(new { success = false, message = "Không thể cập nhật người dùng" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật người dùng" });
            }
        }

        [HttpPut]
        [Route("{id}/mat-khau")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateMatKhau(int id, [FromBody] UpdateMatKhauModel model)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                client.BaseAddress = new Uri(_apiBaseUrl);
                
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.PutAsJsonAsync($"api/NguoiDung/{id}/mat-khau", model);
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Update password response: {content}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<bool>>(content, _jsonOptions);
                    if (result?.Success == true)
                    {
                        return Json(new { success = true, message = "Đổi mật khẩu thành công" });
                    }
                }

                var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, _jsonOptions);
                return Json(new { success = false, message = errorResponse?.Message ?? errorResponse?.Error ?? "Không thể đổi mật khẩu" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating password");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi đổi mật khẩu" });
            }
        }

        [HttpPut]
        [Route("{id}/trang-thai")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTrangThai(int id, [FromBody] bool trangThai)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                client.BaseAddress = new Uri(_apiBaseUrl);
                
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.PutAsJsonAsync($"api/NguoiDung/{id}/trang-thai", trangThai);
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Update status response: {content}");

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<bool>>(content, _jsonOptions);
                    if (result?.Success == true)
                    {
                        return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
                    }
                }

                var errorResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, _jsonOptions);
                return Json(new { success = false, message = errorResponse?.Message ?? errorResponse?.Error ?? "Không thể cập nhật trạng thái" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi cập nhật trạng thái" });
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                // Lấy thông tin người dùng để xóa ảnh đại diện
                var getUserResponse = await client.GetAsync($"api/NguoiDung/{id}");
                if (getUserResponse.IsSuccessStatusCode)
                {
                    var content = await getUserResponse.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<ApiResponse<NguoiDungViewModel>>(content, _jsonOptions);
                    if (result?.Success == true && !string.IsNullOrEmpty(result.Data?.AnhDaiDien))
                    {
                        var avatarPath = Path.Combine(_environment.WebRootPath, result.Data.AnhDaiDien.TrimStart('/'));
                        if (System.IO.File.Exists(avatarPath))
                        {
                            System.IO.File.Delete(avatarPath);
                        }
                    }
                }

                var response = await client.DeleteAsync($"api/NguoiDung/{id}");
                var deleteContent = await response.Content.ReadAsStringAsync();
                var deleteResult = JsonSerializer.Deserialize<ApiResponse<bool>>(deleteContent, _jsonOptions);

                if (response.IsSuccessStatusCode && deleteResult?.Success == true)
                {
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = deleteResult?.Message ?? "Không thể xóa người dùng" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi xóa người dùng" });
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

                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return Json(new { success = false, message = "Chỉ chấp nhận file ảnh .jpg, .jpeg, .png" });
                }

                // Kiểm tra kích thước file (tối đa 2MB)
                if (file.Length > 2 * 1024 * 1024)
                {
                    return Json(new { success = false, message = "Kích thước file không được vượt quá 2MB" });
                }

                // Tạo thư mục nếu chưa tồn tại
                var uploadPath = Path.Combine(_environment.WebRootPath, "images", "avatar");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Tạo tên file ngẫu nhiên để tránh trùng lặp
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadPath, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Trả về đường dẫn tương đối của file
                var relativePath = $"/images/avatar/{fileName}";
                return Json(new { success = true, data = relativePath });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading avatar");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi upload ảnh đại diện" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetByLopHoc(int lopHocId)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.GetAsync($"api/NguoiDung/lop-hoc/{lopHocId}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<List<NguoiDungViewModel>>>(content, _jsonOptions);
                    return Json(new { success = true, data = result?.Data });
                }

                return Json(new { success = false, message = "Không thể lấy danh sách người dùng theo lớp học" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting nguoi dung by lop hoc");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi lấy danh sách người dùng theo lớp học" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetByChucVu(int chucVuId)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.GetAsync($"api/NguoiDung/chuc-vu/{chucVuId}");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<List<NguoiDungViewModel>>>(content, _jsonOptions);
                    return Json(new { success = true, data = result?.Data });
                }

                return Json(new { success = false, message = "Không thể lấy danh sách người dùng theo chức vụ" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting nguoi dung by chuc vu");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi lấy danh sách người dùng theo chức vụ" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetBanChuNhiem()
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                var response = await client.GetAsync("api/NguoiDung/ban-chu-nhiem");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<List<NguoiDungViewModel>>>(content, _jsonOptions);
                    return Json(new { success = true, data = result?.Data });
                }

                return Json(new { success = false, message = "Không thể lấy danh sách ban chủ nhiệm" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ban chu nhiem");
                return Json(new { success = false, message = "Đã xảy ra lỗi khi lấy danh sách ban chủ nhiệm" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                using var client = _clientFactory.CreateClient("BE");
                if (!AddAuthenticationHeader(client))
                {
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập lại để tiếp tục.";
                    return RedirectToAction("Login", "Account");
                }

                var response = await client.GetAsync($"api/NguoiDung/{id}");
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Không thể lấy thông tin người dùng.";
                    return RedirectToAction(nameof(Index));
                }

                var result = JsonSerializer.Deserialize<ApiResponse<NguoiDungViewModel>>(content, _jsonOptions);
                if (result?.Success != true || result.Data == null)
                {
                    TempData["ErrorMessage"] = result?.Error ?? "Không thể lấy thông tin người dùng.";
                    return RedirectToAction(nameof(Index));
                }

                var model = new UpdateNguoiDungModel
                {
                    Id = result.Data.Id,
                    HoTen = result.Data.HoTen,
                    SoDienThoai = result.Data.SoDienThoai,
                    LopHocId = result.Data.LopHocId,
                    ChucVuId = result.Data.ChucVuId,
                    AnhDaiDien = result.Data.AnhDaiDien,
                    TrangThai = result.Data.TrangThai
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Edit GET action");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi lấy thông tin người dùng.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateNguoiDungModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string uniqueFileName = null;
                    if (model.AnhDaiDienFile != null && model.AnhDaiDienFile.Length > 0)
                    {
                        try
                        {
                            // Kiểm tra định dạng file
                            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                            var fileExtension = Path.GetExtension(model.AnhDaiDienFile.FileName).ToLowerInvariant();
                            if (!allowedExtensions.Contains(fileExtension))
                            {
                                ModelState.AddModelError("AnhDaiDienFile", "Chỉ chấp nhận file ảnh .jpg, .jpeg, .png");
                                return View(model);
                            }

                            // Kiểm tra kích thước file (tối đa 2MB)
                            if (model.AnhDaiDienFile.Length > 2 * 1024 * 1024)
                            {
                                ModelState.AddModelError("AnhDaiDienFile", "Kích thước file không được vượt quá 2MB");
                                return View(model);
                            }

                            string uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "avatar");
                            Directory.CreateDirectory(uploadsFolder);
                            
                            uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            // Xóa ảnh cũ nếu có
                            if (!string.IsNullOrEmpty(model.AnhDaiDien))
                            {
                                var oldAvatarPath = Path.Combine(_environment.WebRootPath, model.AnhDaiDien.TrimStart('/'));
                                if (System.IO.File.Exists(oldAvatarPath))
                                {
                                    System.IO.File.Delete(oldAvatarPath);
                                }
                            }

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await model.AnhDaiDienFile.CopyToAsync(fileStream);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error uploading avatar");
                            ModelState.AddModelError("AnhDaiDienFile", $"Lỗi khi upload ảnh: {ex.Message}");
                            return View(model);
                        }
                    }

                    using var client = _clientFactory.CreateClient("BE");
                    client.BaseAddress = new Uri(_apiBaseUrl);
                    
                    if (!AddAuthenticationHeader(client))
                    {
                        TempData["ErrorMessage"] = "Vui lòng đăng nhập lại để tiếp tục.";
                        return RedirectToAction("Login", "Account");
                    }

                    var updateData = new
                    {
                        hoTen = model.HoTen,
                        soDienThoai = model.SoDienThoai,
                        lopHocId = model.LopHocId,
                        chucVuId = model.ChucVuId,
                        anhDaiDien = uniqueFileName != null ? $"/images/avatar/{uniqueFileName}" : model.AnhDaiDien,
                        trangThai = model.TrangThai
                    };

                    _logger.LogInformation($"Updating user {id} with data: {JsonSerializer.Serialize(updateData)}");
                    var response = await client.PutAsJsonAsync($"api/NguoiDung/{id}", updateData);
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"Update response: {content}");

                    if (response.IsSuccessStatusCode)
                    {
                        var result = JsonSerializer.Deserialize<ApiResponse<NguoiDungViewModel>>(content, _jsonOptions);
                        if (result?.Success == true)
                        {
                            TempData["SuccessMessage"] = "Cập nhật người dùng thành công!";
                            return RedirectToAction(nameof(Index));
                        }
                    }

                    // Parse error response
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions);
                        if (errorResponse?.Errors != null)
                        {
                            foreach (var error in errorResponse.Errors)
                            {
                                foreach (var message in error.Value)
                                {
                                    ModelState.AddModelError(error.Key, message);
                                }
                            }
                        }
                        else
                        {
                            var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, _jsonOptions);
                            ModelState.AddModelError("", apiResponse?.Error ?? "Không thể cập nhật người dùng. Vui lòng thử lại sau.");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error parsing error response");
                        ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật người dùng");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Edit POST action");
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật người dùng. Vui lòng thử lại sau.");
                }
            }

            return View(model);
        }

        public class ErrorResponse
        {
            public Dictionary<string, string[]> Errors { get; set; }
        }
    }
} 