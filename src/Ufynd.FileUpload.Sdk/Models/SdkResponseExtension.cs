namespace Ufynd.FileUpload.Sdk.Models;

public static class SdkResponseExtension
{
    public static SdkResponse<T> ErrorResponse<T>(string message) =>
        new SdkResponse<T>
        {
            IsSuccessful = false,
            Message = message
        };

    public static SdkResponse<T> SuccessResponse<T>(T value) =>
        new SdkResponse<T>
        {
            IsSuccessful = true,
            Value = value
        };
}