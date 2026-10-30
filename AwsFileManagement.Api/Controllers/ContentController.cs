using AwsFileManagement.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AwsFileManagement.Api.Controllers
{
    [ApiController]
    [Route("api/content")]
    public class ContentController : ControllerBase
    {
        private readonly IFileService _fileService;

        public ContentController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTopics()
        {
            var topics =
                await _fileService.GetContentTopicsAsync();

            return Ok(topics);
        }
        [HttpGet("{topic}")]
        public async Task<IActionResult> GetContent(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                return BadRequest("Topic is required.");
            }

            var result =
                await _fileService.GetContentPackageAsync(topic);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }
        [HttpDelete("{topic}")]
        public async Task<IActionResult> DeleteContent(string topic)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                return BadRequest("Topic is required.");
            }

            await _fileService.DeleteContentPackageAsync(topic);

            return Ok(new
            {
                Message =
                    $"Content package '{topic}' deleted."
            });
        }
    }

}
