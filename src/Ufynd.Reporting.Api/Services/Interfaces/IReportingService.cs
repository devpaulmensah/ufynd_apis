using Microsoft.AspNetCore.Mvc;
using Ufynd.Core.Models.Responses;
using Ufynd.Reporting.Api.Models.Requests;

namespace Ufynd.Reporting.Api.Services.Interfaces;

public interface IReportingService
{
    Task<BaseResponse<FileContentResult>> GenerateExcelReportAsync(ReportFileRequest request);
}