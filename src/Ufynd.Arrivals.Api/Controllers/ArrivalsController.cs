using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Ufynd.Arrivals.Api.Models.Requests;
using Ufynd.Arrivals.Api.Services.Interfaces;
using Ufynd.Core.Models;
using Ufynd.Core.Models.Responses;

namespace Ufynd.Arrivals.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArrivalsController : ControllerBase
{
    private readonly IArrivalService _arrivalService;

    public ArrivalsController(IArrivalService arrivalService)
    {
        _arrivalService = arrivalService;
    }

    /// <summary>
    /// Filter arrivals of a hotel
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(BaseResponse<EmptyResponse>))]
    [ProducesResponseType(StatusCodes.Status424FailedDependency, Type = typeof(BaseResponse<EmptyResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(BaseResponse<EmptyResponse>))]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BaseResponse<List<UfyndHotel>>))]
    [SwaggerOperation(nameof(FilterArrivals), OperationId = nameof(FilterArrivals))]
    public async Task<IActionResult> FilterArrivals([FromBody] FilterArrivalsRequest request)
    {
        var response = await _arrivalService.FilterArrivals(request);
        return StatusCode(response.Code, response);
    }
}