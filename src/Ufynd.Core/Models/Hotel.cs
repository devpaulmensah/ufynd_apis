using Newtonsoft.Json;

namespace Ufynd.Core.Models;

public class Hotel
{
    public int Classification { get; set; }
    [JsonProperty("hotelID")]
    public int HotelId { get; set; }
    public string Name { get; set; }
    public double ReviewScore { get; set; }
}