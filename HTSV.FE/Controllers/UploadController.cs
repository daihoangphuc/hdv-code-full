using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HTSV.FE.Controllers
{
    [Authorize]
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<UploadController> _logger;

        public UploadController(IWebHostEnvironment webHostEnvironment, ILogger<UploadController> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> SaveFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return Json(new { success = false, message = "Không có file được chọn" });
                }

                // Tạo đường dẫn đến thư mục lưu file
                var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "tintuc");

                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Tạo tên file unique bằng timestamp
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var fileName = $"{timestamp}_{file.FileName}";
                var filePath = Path.Combine(uploadPath, fileName);

                // Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Trả về đường dẫn tương đối của file
                return Json(new { 
                    success = true, 
                    filePath = $"/images/tintuc/{fileName}",
                    message = "Upload file thành công"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi upload file");
                return Json(new { success = false, message = "Lỗi khi upload file: " + ex.Message });
            }
        }
    }
} 