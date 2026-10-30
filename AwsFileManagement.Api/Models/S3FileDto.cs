namespace AwsFileManagement.Api.Models
{
    public class S3FileDto
    {
        public string Key { get; set; } = string.Empty;

        public long Size { get; set; }

        public DateTime LastModified { get; set; }
    }
}

