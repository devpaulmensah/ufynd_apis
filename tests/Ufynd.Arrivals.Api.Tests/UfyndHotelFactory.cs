using Bogus;
using Ufynd.Core.Models;

namespace Ufynd.Arrivals.Api.Tests;

public static class UfyndHotelFactory
{
    private static readonly Faker Faker = new Faker();

    public static List<UfyndHotel> GenerateUfyndHotels(int count)
    {
        List<UfyndHotel> hotels = new List<UfyndHotel>();

        for (int i = 0; i < count; i++)
        {
            hotels.Add(new UfyndHotel
            {
                Hotel = new Hotel
                {
                    Name = Faker.Company.CompanyName(),
                    Classification = Faker.Random.Int(1, 5),
                    HotelId = Faker.Random.Int(1, 1000),
                    ReviewScore = Faker.Random.Double(1, 10)
                },
                HotelRates = GenerateHotelRates(Faker.Random.Int(1, 110))
            });
        }
        
        return hotels;
    }

    private static List<HotelRate> GenerateHotelRates(int count)
    {
        List<HotelRate> hotelRates = new List<HotelRate>();
        
        for (int i = 0; i < count; i++)
        {
            decimal numericFloat = Faker.Random.Decimal(75, 500);
            
            hotelRates.Add(new HotelRate
            {
                Adults = Faker.Random.Int(1, 5),
                Los = Faker.Random.Int(1, 3),
                Price = new HotelRatePrice
                {
                    Currency = "EUR",
                    NumericFloat = numericFloat,
                    NumericInteger = int.Parse(numericFloat.ToString("#####"))
                },
                RateDescription = Faker.Company.CatchPhrase(),
                RateId = Faker.Random.Int(0, 10000).ToString(),
                RateName = Faker.Random.Words(4),
                TargetDay = Faker.Date.Between(DateTime.UtcNow.AddDays(-5), DateTime.UtcNow),
                RateTags = new List<HotelRateTag>
                {
                    new HotelRateTag
                    {
                        Name = "breakfast",
                        Shape = Faker.Random.Bool()
                    }
                }
            });
        }
        
        return hotelRates;
    }
}