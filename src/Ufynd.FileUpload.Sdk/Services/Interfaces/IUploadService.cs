using Ufynd.FileUpload.Sdk.Models;

namespace Ufynd.FileUpload.Sdk.Services.Interfaces;

public interface IUploadService
{
    Task<SdkResponse<string>> UploadFileAsync(Stream stream, string filename);
    Task<SdkResponse<bool>> DeleteFileAsync(string filename);
    Task<byte[]> DownloadFileAsync(string filename);
}