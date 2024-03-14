using Api.Interfaces;
using Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BestStoriesController : ControllerBase
    {
        private readonly IBestStoriesService _bestStoriesService;
        private readonly ILogger _logger;

        public BestStoriesController(IBestStoriesService bestStoriesService, ILogger<BestStoriesController> logger)
        {
            _bestStoriesService = bestStoriesService;
            _logger = logger;
        }

        [HttpGet("")]
        public async Task<ActionResult<IEnumerable<BestStoryDto>>> GetNBestStoriesAsync([FromQuery] int n)
        {
            try
            {
                return await _bestStoriesService.GetNBestStoriesAsync(n);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting {n} best stories", n);
                return StatusCode(500, "An error occured while processing your request.");
            }
        }
    }
}
