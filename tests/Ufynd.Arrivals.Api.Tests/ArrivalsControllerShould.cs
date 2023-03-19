using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Ufynd.Arrivals.Api.Controllers;
using Ufynd.Arrivals.Api.Models.Requests;
using Ufynd.Arrivals.Api.Services.Interfaces;
using Ufynd.Arrivals.Api.Tests.TestSetup;
using Ufynd.Core.Models;
using Ufynd.Core.Models.Responses;
using Xunit;

namespace Ufynd.Arrivals.Api.Tests;

public class ArrivalsControllerShould : IClassFixture<TestFixture>
{
    private readonly Mock<IArrivalService> _arrivalServiceMock;
    
    public ArrivalsControllerShould()
    {
        _arrivalServiceMock = new Mock<IArrivalService>();
    }

    private ArrivalsController GetArrivalsController() =>
        new ArrivalsController(_arrivalServiceMock.Object);

    [Fact]
    public async Task Return_Bad_Request_Response_When_A_Null_Hotel_Is_Provided_As_Value_In_Request()
    {
        // Arrange
        var filterArrivalsRequest = new FilterArrivalsRequest();

        _arrivalServiceMock.Setup(x => x.FilterArrivals(It.IsAny<FilterArrivalsRequest>()))
            .ReturnsAsync(new BaseResponse<List<UfyndHotel>>
            {
                Code = (int) HttpStatusCode.BadRequest
            });
        
        var arrivalsController = GetArrivalsController();

        // Act
        var response = (ObjectResult) await arrivalsController.FilterArrivals(filterArrivalsRequest);

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        var parsedValue = response.Value as BaseResponse<List<UfyndHotel>>;

        parsedValue?.Code.Should().Be(response.StatusCode);
    }
    
    [Fact]
    public async Task Return_Bad_Request_Response_When_An_Empty_Hotel_List_Is_Provided_As_Value_In_Request()
    {
        // Arrange
        var filterArrivalsRequest = new FilterArrivalsRequest
        {
            Hotels = new List<UfyndHotel>()
        };

        _arrivalServiceMock.Setup(x => x.FilterArrivals(It.IsAny<FilterArrivalsRequest>()))
            .ReturnsAsync(new BaseResponse<List<UfyndHotel>>
            {
                Code = (int) HttpStatusCode.BadRequest
            });
        
        var arrivalsController = GetArrivalsController();

        // Act
        var response = (ObjectResult) await arrivalsController.FilterArrivals(filterArrivalsRequest);

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        var parsedValue = response.Value as BaseResponse<List<UfyndHotel>>;

        parsedValue?.Code.Should().Be(response.StatusCode);
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

        _arrivalServiceMock.Setup(x => x.FilterArrivals(It.IsAny<FilterArrivalsRequest>()))
            .ReturnsAsync(new BaseResponse<List<UfyndHotel>>
            {
                Code = (int) HttpStatusCode.BadRequest
            });
        
        var arrivalsController = GetArrivalsController();

        // Act
        var response = (ObjectResult) await arrivalsController.FilterArrivals(filterArrivalsRequest);

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        var parsedValue = response.Value as BaseResponse<List<UfyndHotel>>;

        parsedValue?.Code.Should().Be(response.StatusCode);
    }
    
    [Fact]
    public async Task Return_OK_Response_With_Same_Hotels_List_When_No_HotelId_Or_ArrivalDate_Is_Passed()
    {
        // Arrange
        var randomHotel = UfyndHotelFactory.GenerateUfyndHotels(2);

        var filterArrivalsRequest = new FilterArrivalsRequest
        {
            Hotels = randomHotel
        };

        _arrivalServiceMock.Setup(x => x.FilterArrivals(It.IsAny<FilterArrivalsRequest>()))
            .ReturnsAsync(new BaseResponse<List<UfyndHotel>>
            {
                Code = (int) HttpStatusCode.OK,
                Data = randomHotel
            });
        
        var arrivalsController = GetArrivalsController();

        // Act
        var response = (ObjectResult) await arrivalsController.FilterArrivals(filterArrivalsRequest);

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.OK);
        var parsedValue = response.Value as BaseResponse<List<UfyndHotel>>;

        parsedValue?.Code.Should().Be(response.StatusCode);
        parsedValue?.Data.Should().BeSameAs(randomHotel);
    }
}