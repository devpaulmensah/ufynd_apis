using System.Net;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ufynd.Arrivals.Api.Models.Requests;
using Ufynd.Arrivals.Api.Services.Providers;
using Ufynd.Arrivals.Api.Tests.TestSetup;
using Ufynd.Core.Models;
using Xunit;

namespace Ufynd.Arrivals.Api.Tests;

public class ArrivalServiceShould : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;

    public ArrivalServiceShould(TestFixture fixture)
    {
        _fixture = fixture;
    }

    private ArrivalService GetArrivalService()
    {
        var logger = _fixture.ServiceProvider.GetService<ILogger<ArrivalService>>();
        return new ArrivalService(logger);
    }
    
    [Fact]
    public async Task Return_Bad_Request_Response_When_A_Null_Hotel_Is_Provided_As_Value_In_Request()
    {
        // Arrange
        var filterArrivalsRequest = new FilterArrivalsRequest();
        var arrivalService = GetArrivalService();

        // Act
        var response = await arrivalService.FilterArrivals(filterArrivalsRequest);

        // Assert
        response.Code.Should().Be((int)HttpStatusCode.BadRequest);
        response.Status.Should().NotBeNullOrEmpty();
        response.Status.Should().Be("Provide at least a hotel with at least one of their respective rates");
    }
    
    [Fact]
    public async Task Return_Bad_Request_Response_When_An_Empty_Hotel_Is_Provided_As_Value_In_Request()
    {
        // Arrange
        var filterArrivalsRequest = new FilterArrivalsRequest
        {
            Hotels = new List<UfyndHotel>()
        };

        var arrivalService = GetArrivalService();

        // Act
        var response = await arrivalService.FilterArrivals(filterArrivalsRequest);
        
        // Assert
        response.Code.Should().Be((int)HttpStatusCode.BadRequest);
        response.Status.Should().NotBeNullOrEmpty();
        response.Status.Should().Be("Provide at least a hotel with at least one of their respective rates");
    }
    
    [Fact]
    public async Task Return_Bad_Request_Response_When_A_Hotel_With_No_Rates_Is_Provided_As_Value_In_Request()
    {
        // Arrange
        var randomHotel = UfyndHotelFactory.GenerateUfyndHotels(1);
        randomHotel.FirstOrDefault()!.HotelRates = new List<HotelRate>();
            
        var filterArrivalsRequest = new FilterArrivalsRequest
        {
            Hotels = randomHotel
        };

        var arrivalService = GetArrivalService();

        // Act
        var response = await arrivalService.FilterArrivals(filterArrivalsRequest);
        
        // Assert
        response.Code.Should().Be((int)HttpStatusCode.BadRequest);
        response.Status.Should().NotBeNullOrEmpty();
        response.Status.Should().Be("Provide at least a hotel with at least one of their respective rates");
    }
    
    [Fact]
    public async Task Return_OK_Response_With_Same_Hotels_List_When_No_HotelId_Or_ArrivalDate_Is_Passed()
    {
        // Arrange
        var randomHotel = UfyndHotelFactory.GenerateUfyndHotels(4);

        var filterArrivalsRequest = new FilterArrivalsRequest
        {
            Hotels = randomHotel
        };

        var arrivalService = GetArrivalService();

        // Act
        var response = await arrivalService.FilterArrivals(filterArrivalsRequest);
        
        // Assert
        response.Code.Should().Be((int)HttpStatusCode.OK);
        response.Status.Should().NotBeNullOrEmpty();
        response.Status.Should().Be("Retrieved successfully");
        response.Data.Should().NotBeEmpty();
        response.Data.Should().BeSameAs(randomHotel);
    }
    
    [Fact]
    public async Task Return_OK_Response_With_Filtered_Hotels_List_When_A_HotelId_Is_Passed_As_Filter()
    {
        // Arrange
        var randomHotel = UfyndHotelFactory.GenerateUfyndHotels(4);
        int? randomHotelId = randomHotel.FirstOrDefault()?.Hotel.HotelId;
        
        var filterArrivalsRequest = new FilterArrivalsRequest
        {
            Hotels = randomHotel,
            HotelId = randomHotelId
        };

        var arrivalService = GetArrivalService();

        // Act
        var response = await arrivalService.FilterArrivals(filterArrivalsRequest);
        
        // Assert
        response.Code.Should().Be((int)HttpStatusCode.OK);
        response.Status.Should().NotBeNullOrEmpty();
        response.Status.Should().Be("Retrieved successfully");
        response.Data.Should().NotBeEmpty();
        response.Data.ForEach(hotel => hotel.Hotel.HotelId.Should().Be(randomHotelId));
    }
    
    [Fact]
    public async Task Return_OK_Response_With_Filtered_Hotels_List_When_An_ArrivalDate_Is_Passed_As_Filter()
    {
        // Arrange
        var randomHotel = UfyndHotelFactory.GenerateUfyndHotels(4);
        var randomArrivalDate = randomHotel.FirstOrDefault()?.HotelRates.FirstOrDefault()?.TargetDay;
        
        var filterArrivalsRequest = new FilterArrivalsRequest
        {
            Hotels = randomHotel,
            ArrivalDate = randomArrivalDate
        };

        var arrivalService = GetArrivalService();

        // Act
        var response = await arrivalService.FilterArrivals(filterArrivalsRequest);
        
        // Assert
        response.Code.Should().Be((int)HttpStatusCode.OK);
        response.Status.Should().NotBeNullOrEmpty();
        response.Status.Should().Be("Retrieved successfully");
        response.Data.Should().NotBeEmpty();
        response.Data.ForEach(hotel => hotel.HotelRates.All(x => 
            x.TargetDay.ToString("yyyy-MM-dd").Equals(randomArrivalDate.Value.ToString("yyyy-MM-dd"))));
    }
    
    [Fact]
    public async Task Return_OK_Response_With_Filtered_Hotels_List_When_Both_ArrivalDate_And_HotelId_Are_Passed_As_Filters()
    {
        // Arrange
        var randomHotel = UfyndHotelFactory.GenerateUfyndHotels(4);
        var randomArrivalDate = randomHotel.FirstOrDefault()?.HotelRates.FirstOrDefault()?.TargetDay;
        int? randomHotelId = randomHotel.FirstOrDefault()?.Hotel.HotelId;
        
        var filterArrivalsRequest = new FilterArrivalsRequest
        {
            Hotels = randomHotel,
            ArrivalDate = randomArrivalDate,
            HotelId = randomHotelId
        };

        var arrivalService = GetArrivalService();

        // Act
        var response = await arrivalService.FilterArrivals(filterArrivalsRequest);
        
        // Assert
        response.Code.Should().Be((int)HttpStatusCode.OK);
        response.Status.Should().NotBeNullOrEmpty();
        response.Status.Should().Be("Retrieved successfully");
        response.Data.Should().NotBeEmpty();
        response.Data.ForEach(hotel => hotel.Hotel.HotelId.Should().Be(randomHotelId));
        response.Data.ForEach(hotel => hotel.HotelRates.All(x => 
            x.TargetDay.ToString("yyyy-MM-dd").Equals(randomArrivalDate.Value.ToString("yyyy-MM-dd"))));
    }
    
    [Fact] 
    public async Task Return_Internal_Server_Response_When_A_Null_Value_Is_Provided_As_In_Request()
    {
        // Arrange
        var arrivalService = GetArrivalService();

        // Act
        var response = await arrivalService.FilterArrivals(null);

        // Assert
        response.Code.Should().Be((int)HttpStatusCode.InternalServerError);
        response.Status.Should().NotBeNullOrEmpty();
        response.Status.Should().Be("Something bad happened, try again later.");
    }
}