using AwsFileManagement.Api.Models;

namespace AwsFileManagement.Api.Services;

public interface IFileService
{
    Task<List<S3FileDto>> GetFilesAsync();
    Task UploadFileAsync(IFormFile file);
    Task DeleteFileAsync(string key);
    Task<string> GetDownloadUrlAsync(string key);
    Task SaveContentPackageAsync(
    ContentPackageDto content);
    Task<List<string>> GetContentTopicsAsync();
    Task<ContentPackageDto?> GetContentPackageAsync(string topic);
    Task DeleteContentPackageAsync(string topic);
}