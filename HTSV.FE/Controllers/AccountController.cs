using Microsoft.AspNetCore.Mvc;
using HTSV.FE.Models.NguoiDung;
using HTSV.FE.Extensions;
using HTSV.FE.Models.Common;
using System.Text.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace HTSV.FE.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<AccountController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountController(
            IHttpClientFactory clientFactory, 
            ILogger<AccountController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _clientFactory = clientFactory;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            try
            {
                var client = _clientFactory.CreateClient("BE");
                var response = await client.GetAsync("/api/LopHoc");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedList<LopHocViewModel>>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.Success == true && apiResponse.Data?.Items != null)
                    {
                        ViewBag.Classes = apiResponse.Data.Items;
                        return View();
                    }
                }

                ViewBag.Classes = new List<LopHocViewModel>();
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching class list");
                ViewBag.Classes = new List<LopHocViewModel>();
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Xử lý upload ảnh nếu có
                    if (model.AnhDaiDien != null)
                    {
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "avatar");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.AnhDaiDien.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.AnhDaiDien.CopyToAsync(fileStream);
                        }

                        // Tạo đường dẫn tuyệt đối
                        var request = HttpContext.Request;
                        var domain = $"{request.Scheme}://{request.Host}";
                        model.AnhDaiDienPath = $"{domain}/images/avatar/{uniqueFileName}";
                    }

                    var client = _clientFactory.CreateClient("BE");
                    var response = await client.PostAsJsonAsync("/api/NguoiDung/register", model);
                    var content = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var apiResponse = JsonSerializer.Deserialize<ApiResponse<object>>(content, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (apiResponse?.Success == true)
                        {
                            TempData["Success"] = "Đăng ký thành công. Vui lòng đăng nhập.";
                            return RedirectToAction(nameof(Login));
                        }

                        ModelState.AddModelError(string.Empty, apiResponse?.Message ?? "Đăng ký không thành công");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Đăng ký không thành công");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Register action");
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi đăng ký");
            }

            // Repopulate the class list
            try
            {
                var client = _clientFactory.CreateClient("BE");
                var response = await client.GetAsync("/api/LopHoc");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<PaginatedList<LopHocViewModel>>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (apiResponse?.Success == true && apiResponse.Data?.Items != null)
                    {
                        ViewBag.Classes = apiResponse.Data.Items;
                    }
                }
            }
            catch
            {
                ViewBag.Classes = new List<LopHocViewModel>();
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var client = _clientFactory.CreateClient("BE");
                var response = await client.PostAsJsonAsync("/api/NguoiDung/login", model);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<LoginResponseModel>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result != null)
                    {
                        // Lưu thông tin vào session sau khi đăng nhập thành công
                        HttpContext.Session.Set("LoginResponse", result);
                        HttpContext.Session.Set("CurrentUser", result.ThongTinNguoiDung);
                        HttpContext.Session.SetString("TokenUser", result.Token);
                        HttpContext.Session.SetString("MaSinhVien", result.ThongTinNguoiDung.MaSoSinhVien);
                        HttpContext.Session.SetString("Role", result.ThongTinNguoiDung.DanhSachQuyen.FirstOrDefault());

                        Console.WriteLine("Dang nhap thanh cong voi Token: " + result.Token);
                        Console.WriteLine("Ma sinh vien: " + HttpContext.Session.GetString("MaSinhVien"));
                        // Tạo claims cho người dùng
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, result.ThongTinNguoiDung.HoTen),
                            new Claim(ClaimTypes.Email, result.ThongTinNguoiDung.Email),
                            new Claim(ClaimTypes.Role, result.ThongTinNguoiDung.DanhSachQuyen.FirstOrDefault())
                        };

                        Console.WriteLine("Quyền người dùng: " + result.ThongTinNguoiDung.DanhSachQuyen.FirstOrDefault());
                        // Thêm roles vào claims
                        foreach (var role in result.ThongTinNguoiDung.DanhSachQuyen)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role));
                        }

                        // Tạo identity
                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        // Tạo principal
                        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                        // Đăng nhập và tạo cookie
                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            claimsPrincipal,
                            new AuthenticationProperties
                            {
                                IsPersistent = model.RememberMe,
                                ExpiresUtc = result.HetHan
                            });

                        if (!string.IsNullOrEmpty(returnUrl))
                        {
                            return LocalRedirect(returnUrl);
                        }
                        return RedirectToAction("Index", "Home");
                    }

                    ModelState.AddModelError(string.Empty, "Đăng nhập không thành công");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Đăng nhập không thành công");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Login action");
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi đăng nhập");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Xóa cookie xác thực
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            // Xóa session
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }

    public class LoginResponseModel
    {
        public string Token { get; set; } = string.Empty;
        public DateTime HetHan { get; set; }
        public NguoiDungViewModel ThongTinNguoiDung { get; set; } = new();
    }
} 