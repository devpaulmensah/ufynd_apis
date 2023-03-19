using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Options;
using Ufynd.Core.Configurations;
using Ufynd.FileUpload.Sdk.Models;
using Ufynd.FileUpload.Sdk.Services.Interfaces;

namespace Ufynd.FileUpload.Sdk.Services.Providers;

public class UploadService : IUploadService, IDisposable
{
    private readonly S3ServiceConfig _s3ServiceConfig;
    private IAmazonS3 _awsS3Client;
    private ITransferUtility _transferUtility;

    public UploadService(IOptions<S3ServiceConfig> s3ServiceConfig)
    {
        _s3ServiceConfig = s3ServiceConfig.Value;

        SetupAwsTransferUtility();
    }

    private async void SetupAwsTransferUtility()
    {
        BasicAWSCredentials credentials = new BasicAWSCredentials(_s3ServiceConfig.AccessKey, _s3ServiceConfig.Secret);
        AmazonS3Config amazonS3Config = new AmazonS3Config
        {
            ServiceURL = _s3ServiceConfig.AwsUrl,
            ForcePathStyle = true
        };

        _awsS3Client = new AmazonS3Client(credentials, amazonS3Config);
        _transferUtility = new TransferUtility(_awsS3Client);
        
        var buckets = await ListBucketsAsync();

        if (!buckets.Contains(_s3ServiceConfig.BucketName))
        {
            await CreateBucket(_s3ServiceConfig.BucketName);
        }
    }
    
    public async Task<SdkResponse<string>> UploadFileAsync(Stream stream, string filename)
    {
        try
        {
            PutObjectRequest uploadRequest = new PutObjectRequest
            {
                InputStream = stream,
                BucketName = _s3ServiceConfig.BucketName,
                Key = $"{_s3ServiceConfig.FolderName}/{filename}"
            };
                
            uploadRequest.Metadata.Add("type", "file");

            PutObjectResponse uploadResponse = await _awsS3Client.PutObjectAsync(uploadRequest);

            return uploadResponse != null && HttpStatusCode.OK.Equals(uploadResponse.HttpStatusCode)
                ? SdkResponseExtension.SuccessResponse(filename)
                : SdkResponseExtension.ErrorResponse<string>("An error occured uploading file");
        }
        catch (Exception e)
        {
            return SdkResponseExtension.ErrorResponse<string>(e.Message);
        }
    }

    public async Task<SdkResponse<bool>> DeleteFileAsync(string filename)
    {
        try
        {
            DeleteObjectResponse deleteResponse = await _awsS3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _s3ServiceConfig.BucketName,
                Key = $"{_s3ServiceConfig.FolderName}/{filename}"
            });

            return !HttpStatusCode.NoContent.Equals(deleteResponse.HttpStatusCode)
                ? SdkResponseExtension.ErrorResponse<bool>("An error occured deleting file")
                : SdkResponseExtension.SuccessResponse(true);
        }
        catch (Exception e)
        {
            return SdkResponseExtension.ErrorResponse<bool>(e.Message);
        }
    }

    public async Task<byte[]> DownloadFileAsync(string filename)
    {
        try
        {
            using (var stream = new MemoryStream())
            {
                string key = $"{_s3ServiceConfig.FolderName}/{filename}";
                await (await _transferUtility.OpenStreamAsync(_s3ServiceConfig.BucketName, key)).CopyToAsync(stream);
                return stream.ToArray();
            }
        }
        catch (Exception e)
        {
            return new byte[] { };
        }
    }

    private async Task<List<string>> ListBucketsAsync()
    {
        try
        {
            var response = await _awsS3Client.ListBucketsAsync();
            return response.Buckets.Select(x => x.BucketName).ToList();
        }
        catch (Exception e)
        {
            return new List<string>();
        }
    }

    private async Task CreateBucket(string newBucketName)
    {
        var putBucketRequest = new PutBucketRequest { BucketName = newBucketName };
        await _awsS3Client.PutBucketAsync(putBucketRequest);

        // to grant everyone permission
        var grant = new S3Grant
        {
            Grantee = new S3Grantee { CanonicalUser = "Everyone" },
            Permission = new S3Permission("List")
        };

        var grants = new List<S3Grant> { grant };
        putBucketRequest.Grants = grants;
    }
    
    public void Dispose()
    {
        _awsS3Client?.Dispose();
        _transferUtility?.Dispose();
    }
}