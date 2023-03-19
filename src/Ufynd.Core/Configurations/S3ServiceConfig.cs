namespace Ufynd.Core.Configurations;

public class S3ServiceConfig
{
    public string AccessKey { get; set; }
    public string Secret { get; set; }
    public string AwsUrl { get; set; }
    public string BucketName { get; set; }
    public string FolderName { get; set; }
}