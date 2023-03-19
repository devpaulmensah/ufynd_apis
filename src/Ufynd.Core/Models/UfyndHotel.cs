namespace Ufynd.Core.Models;

public class UfyndHotel
{
    public Hotel Hotel { get; set; }
    public List<HotelRate> HotelRates { get; set; } = new List<HotelRate> { };
}