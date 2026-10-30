using AwsFileManagement.Api.Models;
using AwsFileManagement.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AwsFileManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FilesController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFiles()
        {
            var result = await _fileService.GetFilesAsync();

            return Ok(result);
        }
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            await _fileService.UploadFileAsync(file);

            return Ok(new
            {
                Message = "File uploaded successfully."
            });
        }


        [HttpDelete("{*key}")]
        public async Task<IActionResult> Delete(string key)
        {
            key = WebUtility.UrlDecode(key);

            Console.WriteLine($"Deleting: {key}");

            await _fileService.DeleteFileAsync(key);

            return Ok(new
            {
                Message = "File deleted successfully."
            });
        }
        [HttpGet("download/{*key}")]
        public async Task<IActionResult> Download(string key)
        {
            key = WebUtility.UrlDecode(key);

            var url = await _fileService.GetDownloadUrlAsync(key);

            return Ok(new
            {
                DownloadUrl = url
            });
        }
        [HttpPost("save")]
        public async Task<IActionResult> SaveContent(
            ContentPackageDto content)
        {
            if (content == null)
            {
                return BadRequest("Request is required.");
            }

            if (string.IsNullOrWhiteSpace(content.Topic))
            {
                return BadRequest("Topic is required.");
            }

            await _fileService.SaveContentPackageAsync(content);

            return Ok(new
            {
                Message = "Content package saved."
            });
        }
    }
}
