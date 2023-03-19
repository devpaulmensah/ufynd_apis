using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ufynd.Arrivals.Api.Models.Requests;
using Ufynd.Arrivals.Api.Services.Interfaces;
using Ufynd.Core.Models;
using Ufynd.Core.Models.Responses;

namespace Ufynd.Arrivals.Api.Services.Providers;

public class ArrivalService : IArrivalService
{
    private readonly ILogger<ArrivalService> _logger;

    public ArrivalService(ILogger<ArrivalService> logger)
    {
        _logger = logger;
    }
    
    public async Task<BaseResponse<List<UfyndHotel>>> FilterArrivals(FilterArrivalsRequest request)
    {
        try
        {
            var validationResult = await new FilterArrivalsRequestValidator().ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return CommonResponses.ErrorResponse
                    .BadRequestResponse<List<UfyndHotel>>("Provide at least a hotel with at least one of their respective rates");
            }

            if (request.HotelId.HasValue)
            {
                request.Hotels = request.Hotels
                    .Where(x => x.Hotel.HotelId.Equals(request.HotelId.Value))
                    .ToList();
            }

            if (request.ArrivalDate.HasValue)
            {
                var arrivalsList = new List<UfyndHotel>();
                
                request.Hotels.ForEach(hotel =>
                {
                    var arrivals = hotel.HotelRates.Where(x =>
                            x.TargetDay.ToString("yyyy-MM-dd")
                                .Equals(request.ArrivalDate.Value.ToString("yyyy-MM-dd")))
                        .ToList();
                    
                    if (arrivals.Any())
                    {
                        arrivalsList.Add(new UfyndHotel
                        {
                            Hotel = hotel.Hotel,
                            HotelRates = arrivals
                        });
                    }
                });

                request.Hotels = arrivalsList;
            }

            await Task.Delay(0);
            return CommonResponses.SuccessResponse.OkResponse(request.Hotels);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured filtering arrivals");
            return CommonResponses.ErrorResponse.InternalServerErrorResponse<List<UfyndHotel>>();
        }
    }
}