using DrugWarehouseManagement.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace DrugWarehouseManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideoDetectionController : ControllerBase
    {
        private readonly VideoDetectionService _videoDetectionService;

        public VideoDetectionController(VideoDetectionService videoDetectionService)
        {
            _videoDetectionService = videoDetectionService;
        }

        [HttpPost("detect")]
        public async Task<IActionResult> DetectProductsInVideo(IFormFile videoFile)
        {
            if (videoFile == null || videoFile.Length == 0)
            {
                return BadRequest("No video file uploaded.");
            }

            try
            {
                using (var stream = videoFile.OpenReadStream())
                {
                    // Chạy ProcessVideo trong một task riêng
                    var result = await Task.Run(() => _videoDetectionService.ProcessVideo(stream));
                    return Ok(new
                    {
                        TotalCount = result.TotalCount,
                        Detections = result.Detections.Take(10).Select(d => new
                        {
                            BoundingBox = new
                            {
                                X = d.BoundingBox.X,
                                Y = d.BoundingBox.Y,
                                Width = d.BoundingBox.Width,
                                Height = d.BoundingBox.Height
                            },
                            Label = d.Label,
                            Confidence = d.Confidence
                        }).ToList()
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
