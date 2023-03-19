using Newtonsoft.Json;

namespace Ufynd.Core.Models;

public class HotelRate
{
    public int Adults { get; set; }
    public int Los { get; set; }
    public HotelRatePrice Price { get; set; }
    public string RateDescription { get; set; }
    [JsonProperty("rateID")]
    public string RateId { get; set; }
    public string RateName { get; set; }
    public List<HotelRateTag> RateTags { get; set; } = new List<HotelRateTag>() { };
    public DateTime TargetDay { get; set; }
}