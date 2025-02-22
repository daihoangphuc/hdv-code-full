using Microsoft.AspNetCore.Mvc;
using HTSV.FE.Models.NguoiDung;
using HTSV.FE.Models.Common;
using HTSV.FE.Extensions;
using System.Text.Json;

namespace HTSV.FE.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IHttpClientFactory clientFactory, ILogger<AuthController> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
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
                var response = await client.PostAsJsonAsync("/api/auth/login", model);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<NguoiDungViewModel>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (result?.Success == true && result.Data != null)
                    {
                        HttpContext.Session.Set("CurrentUser", result.Data);
                        if (!string.IsNullOrEmpty(returnUrl))
                        {
                            return LocalRedirect(returnUrl);
                        }
                        return RedirectToAction("Index", "Home");
                    }

                    ModelState.AddModelError(string.Empty, result?.Message ?? "Đăng nhập không thành công");
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
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
} 