using System.Net;

namespace Ufynd.Core.Models.Responses;

public static class CommonResponses
{
    private const string InternalServerErrorResponseStatus = "Something bad happened, try again later.";
    private const string FailedDependencyErrorResponseStatus = "An error occured, try again later";
    private const string DefaultOkResponseStatus = "Retrieved successfully";

    public static class ErrorResponse
    {
        public static BaseResponse<T> InternalServerErrorResponse<T>() =>
            new BaseResponse<T>
            {
                Code = (int)HttpStatusCode.InternalServerError,
                Status = InternalServerErrorResponseStatus
            };
        
        public static BaseResponse<T> BadRequestResponse<T>(string message) =>
            new BaseResponse<T>
            {
                Code = (int)HttpStatusCode.BadRequest,
                Status = message
            };

        public static BaseResponse<T> FailedDependencyErrorResponse<T>() =>
            new BaseResponse<T>
            {
                Code = (int)HttpStatusCode.FailedDependency,
                Status = FailedDependencyErrorResponseStatus
            };
    }

    public static class SuccessResponse
    {
        public static BaseResponse<T> OkResponse<T>(T data, string status = null) =>
            new BaseResponse<T>
            {
                Code = (int)HttpStatusCode.OK,
                Status = status?? DefaultOkResponseStatus,
                Data = data
            };
    }
}