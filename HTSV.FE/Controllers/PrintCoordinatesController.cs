using Microsoft.AspNetCore.Mvc;

namespace HTSV.FE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PrintCoordinatesController : ControllerBase
    {
        private readonly ILogger<PrintCoordinatesController> _logger;

        public PrintCoordinatesController(ILogger<PrintCoordinatesController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult PrintToTerminal([FromBody] CoordinatesModel coordinates)
        {
            var message = $"\n=== TỌA ĐỘ GPS ===\n" +
                         $"Thời gian: {coordinates.Timestamp}\n" +
                         $"Vĩ độ (latitude): {coordinates.Latitude}\n" +
                         $"Kinh độ (longitude): {coordinates.Longitude}\n" +
                         $"==================\n";

            // In ra terminal
            Console.WriteLine(message);

            return Ok();
        }
    }

    public class CoordinatesModel
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Timestamp { get; set; }
    }
} 