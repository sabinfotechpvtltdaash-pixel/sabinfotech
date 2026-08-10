using Microsoft.AspNetCore.Mvc;

namespace Erpweb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        public UploadController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveImage([FromBody] ImagePayload payload)
        {
            if (string.IsNullOrEmpty(payload?.ImageData))
            {
                return BadRequest(new { success = false, message = "No image payload received." });
            }

            try
            {
                string today = DateTime.Now.ToString("yyyy-MM-dd");

                // Folders: wwwroot/uploads/yyyy-MM-dd and project_root/logs
                string uploadDirectory = Path.Combine(_environment.WebRootPath, "uploads", today);
                string logDirectory = Path.Combine(_environment.ContentRootPath, "logs");

                Directory.CreateDirectory(uploadDirectory);
                Directory.CreateDirectory(logDirectory);

                // Save JPG image file
                string fileName = $"img_{DateTime.Now:HHmmss_fff}.jpg";
                string filePath = Path.Combine(uploadDirectory, fileName);
                string relativePath = $"/uploads/{today}/{fileName}";

                string base64Data = payload.ImageData.Contains(",")
                    ? payload.ImageData.Split(',')[1]
                    : payload.ImageData;

                byte[] imageBytes = Convert.FromBase64String(base64Data);
                await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                // Create/Append to day-wise log text file: logs/yyyy-MM-dd.txt
                string logFilePath = Path.Combine(logDirectory, $"{today}.txt");
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Saved Path: {relativePath}{Environment.NewLine}";
                await System.IO.File.AppendAllTextAsync(logFilePath, logEntry);

                return Ok(new { success = true, filePath = relativePath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("images")]
        public IActionResult GetImages([FromQuery] string? date)
        {
            string targetDate = string.IsNullOrWhiteSpace(date)
                ? DateTime.Now.ToString("yyyy-MM-dd")
                : date;

            string targetDirectory = Path.Combine(_environment.WebRootPath, "uploads", targetDate);

            if (!Directory.Exists(targetDirectory))
            {
                return Ok(new { date = targetDate, images = Array.Empty<string>() });
            }

            var imagePaths = Directory.GetFiles(targetDirectory, "*.jpg")
                .Select(file => $"/uploads/{targetDate}/{Path.GetFileName(file)}")
                .ToList();

            return Ok(new { date = targetDate, images = imagePaths });
        }
    }

    public class ImagePayload
    {
        public string ImageData { get; set; } = string.Empty;
    }
}