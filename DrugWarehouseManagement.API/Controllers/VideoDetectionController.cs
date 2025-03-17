using DrugWarehouseManagement.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace DrugWarehouseManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideoDetectionController : ControllerBase
    {
        private readonly VideoDetectionService _detectionService;

        public VideoDetectionController(VideoDetectionService detectionService)
        {
            _detectionService = detectionService;
        }

        [HttpPost("/detect")]
        public IActionResult DetectObjects([FromBody] string videoPath)
        {
            try
            {
                var result = _detectionService.ProcessVideo(videoPath);
                return Ok(result);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
