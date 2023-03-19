using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FluentValidation;
using Ufynd.Core.Models;

namespace Ufynd.Arrivals.Api.Models.Requests;

public class FilterArrivalsRequest
{
    [Required]
    [MinLength(1)]
    public List<UfyndHotel> Hotels { get; set; } = new List<UfyndHotel>();
    /*
     * HotelId are not required and will be as filters
     */
    public int? HotelId { get; set; }
    public DateTime? ArrivalDate { get; set; }
}

public class FilterArrivalsRequestValidator : AbstractValidator<FilterArrivalsRequest>
{
    public FilterArrivalsRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x != null);
        
        RuleFor(x => x.Hotels)
            .Must(x =>
                x.Count > 0 &&
                x.TrueForAll(y =>
                    y.HotelRates != null &&
                    y.HotelRates.Count > 0));
    }
}