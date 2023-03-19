using System.Collections.Generic;
using System.Threading.Tasks;
using Ufynd.Arrivals.Api.Models.Requests;
using Ufynd.Core.Models;
using Ufynd.Core.Models.Responses;

namespace Ufynd.Arrivals.Api.Services.Interfaces;

public interface IArrivalService
{
    Task<BaseResponse<List<UfyndHotel>>> FilterArrivals(FilterArrivalsRequest request);
}