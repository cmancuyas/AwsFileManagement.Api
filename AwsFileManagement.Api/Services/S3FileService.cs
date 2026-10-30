using Amazon.S3;
using Amazon.S3.Model;
using AwsFileManagement.Api.Models;

namespace AwsFileManagement.Api.Services;

public class S3FileService : IFileService
{
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<S3FileService> _logger;
    public S3FileService(
        IAmazonS3 s3Client,
        IConfiguration configuration,
        ILogger<S3FileService> logger)
    {
        _s3Client = s3Client;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<List<S3FileDto>> GetFilesAsync()
    {
        var bucketName =
            _configuration["S3Settings:BucketName"];

        var response =
            await _s3Client.ListObjectsV2Async(
                new ListObjectsV2Request
                {
                    BucketName = bucketName
                });

        return response.S3Objects
            .Select(x => new S3FileDto
            {
                Key = x.Key,
                Size = x.Size ?? 0,
                LastModified = x.LastModified?.ToLocalTime() ?? DateTime.MinValue
            })
            .ToList();
    }
    public async Task UploadFileAsync(IFormFile file)
    {
        var bucketName =
            _configuration["S3Settings:BucketName"];

        using var stream = file.OpenReadStream();

        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = $"uploads/{file.FileName}",
            InputStream = stream,
            ContentType = file.ContentType
        };

        _logger.LogInformation("uploading file {FileName}", file.FileName);

        await _s3Client.PutObjectAsync(request);
    }
    public async Task DeleteFileAsync(string key)
    {
        var bucketName =
            _configuration["S3Settings:BucketName"];

        _logger.LogInformation("deleting file {Key}", key);

        await _s3Client.DeleteObjectAsync(
            new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = key
            });
    }
    public Task<string> GetDownloadUrlAsync(string key)
    {
        var bucketName =
            _configuration["S3Settings:BucketName"];

        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(5)
        };

        _logger.LogInformation("generating download URL for {Key}", key);

        var url = _s3Client.GetPreSignedURL(request);

        return Task.FromResult(url);
    }
    public async Task SaveContentPackageAsync(
    ContentPackageDto content)
    {
        var bucketName =
            _configuration["S3Settings:BucketName"];

        var folder =
            $"youtube-content/{content.Topic.Replace(" ", "-")}/";

        _logger.LogInformation("saving content package for topic {Topic}", content.Topic);

        await SaveTextFile(
            bucketName,
            $"{folder}script.txt",
            content.Script);

        await SaveTextFile(
            bucketName,
            $"{folder}seo-report.txt",
            content.SeoReport);

        await SaveTextFile(
            bucketName,
            $"{folder}thumbnail-prompt.txt",
            content.ThumbnailPrompt);

        await SaveTextFile(
            bucketName,
            $"{folder}shorts.txt",
            content.ShortsContent);

        await SaveTextFile(
            bucketName,
            $"{folder}manifest.json",
            content.ProductionManifest);
    }
    private async Task SaveTextFile(
    string bucketName,
    string key,
    string content)
    {
        await _s3Client.PutObjectAsync(
            new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                ContentBody = content
            });
    }
    public async Task<List<string>> GetContentTopicsAsync()
    {
        try
        {
            var bucketName =
                _configuration["S3Settings:BucketName"];

            if (string.IsNullOrWhiteSpace(bucketName))
            {
                throw new InvalidOperationException(
                    "Bucket name not configured.");
            }

            var response =
                await _s3Client.ListObjectsV2Async(
                    new ListObjectsV2Request
                    {
                        BucketName = bucketName,
                        Prefix = "youtube-content/"
                    });

            _logger.LogInformation("retrieved {Count} objects from bucket {BucketName}", response.S3Objects.Count, bucketName);

            return response.S3Objects?
                       .Select(x => x.Key?.Split('/'))
                       .Where(parts => parts is { Length: > 1 })
                       .Select(parts => parts![1])
                       .Distinct()
                       .ToList()
                   ?? new List<string>();
        }
        catch (AmazonS3Exception ex)
        {
            throw new ApplicationException(
                $"S3 error: {ex.Message}",
                ex);
        }
    }
    private async Task<string> ReadTextFile(
    string bucketName,
    string key)
    {
        var response =
            await _s3Client.GetObjectAsync(
                bucketName,
                key);

        using var reader =
            new StreamReader(response.ResponseStream);

        return await reader.ReadToEndAsync();
    }
    public async Task<ContentPackageDto?> GetContentPackageAsync(string topic)
    {
        var bucketName =
            _configuration["S3Settings:BucketName"];

        var folder =
            $"youtube-content/{topic}/";

        _logger.LogInformation("retrieving content package for topic {Topic}", topic);

        return new ContentPackageDto
        {
            Topic = topic,

            Script = await ReadTextFile(
                bucketName,
                $"{folder}script.txt"),

            SeoReport = await ReadTextFile(
                bucketName,
                $"{folder}seo-report.txt"),

            ThumbnailPrompt = await ReadTextFile(
                bucketName,
                $"{folder}thumbnail-prompt.txt"),

            ShortsContent = await ReadTextFile(
                bucketName,
                $"{folder}shorts.txt"),

            ProductionManifest = await ReadTextFile(
                bucketName,
                $"{folder}manifest.json")
        };
    }
    public async Task DeleteContentPackageAsync(string topic)
    {
        var bucketName =
            _configuration["S3Settings:BucketName"];

        var prefix =
            $"youtube-content/{topic}/";

        _logger.LogInformation("deleting content package for topic {Topic}", topic);

        var response =
            await _s3Client.ListObjectsV2Async(
                new ListObjectsV2Request
                {
                    BucketName = bucketName,
                    Prefix = prefix
                });

        foreach (var obj in response.S3Objects)
        {
            await _s3Client.DeleteObjectAsync(
                new DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = obj.Key
                });
        }
    }
}