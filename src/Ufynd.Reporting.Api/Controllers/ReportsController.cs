using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Ufynd.Core.Models.Responses;
using Ufynd.Reporting.Api.Models.Requests;
using Ufynd.Reporting.Api.Services.Interfaces;

namespace Ufynd.Reporting.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportingService _reportingService;

    public ReportsController(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    /// <summary>
    /// Generate report for hotel based on json file
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(BaseResponse<EmptyResponse>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(BaseResponse<EmptyResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BaseResponse<EmptyResponse>))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileContentResult))]
    [SwaggerOperation(nameof(GenerateReport), OperationId = nameof(GenerateReport))]
    public async Task<IActionResult> GenerateReport([FromBody] ReportFileRequest request)
    {
        var response = await _reportingService.GenerateExcelReportAsync(request);
        
        if (!response.IsSuccessful())
        {
            return StatusCode(response.Code, response);
        }

        return response.Data;
    }
}