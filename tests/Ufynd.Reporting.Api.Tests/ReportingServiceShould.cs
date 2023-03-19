using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ufynd.Core.Models;
using Ufynd.Reporting.Api.Models.Requests;
using Ufynd.Reporting.Api.Services.Providers;
using Ufynd.Reporting.Api.Tests.TestSetup;
using Xunit;

namespace Ufynd.Reporting.Api.Tests;

public class ReportingServiceShould : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;

    public ReportingServiceShould(TestFixture fixture)
    {
        _fixture = fixture;
    }

    private ReportingService GetReportingService()
    {
        var logger = _fixture.ServiceProvider.GetService<ILogger<ReportingService>>();
        var configuration = _fixture.ServiceProvider.GetService<IConfiguration>();

        return new ReportingService(logger, configuration);
    }

    [Fact] 
    public async Task Return_Internal_Server_Response_When_A_Null_Hotel_Is_Provided_As_Value_In_Request()
    {
        // Arrange
        var reportFileRequest = new ReportFileRequest();
        var reportingService = GetReportingService();

        // Act
        var response = await reportingService.GenerateExcelReportAsync(reportFileRequest);

        // Assert
        response.Code.Should().Be((int)HttpStatusCode.InternalServerError);
        response.Status.Should().NotBeNullOrEmpty();
        response.Status.Should().Be("Something bad happened, try again later.");
    }
    
    [Fact]
    public async Task Return_Bad_Request_Response_When_A_Hotel_With_No_Rates_Is_Provided_As_Value_In_Request()
    {
        // Arrange
        var randomHotel = UfyndHotelFactory.GenerateUfyndHotel();
        randomHotel.HotelRates = new List<HotelRate>();
            
        var reportFileRequest = new ReportFileRequest
        {
            Hotel = randomHotel
        };
        
        var reportingService = GetReportingService();
    
        // Act
        var response = await reportingService.GenerateExcelReportAsync(reportFileRequest);

        // Assert
        response.Code.Should().Be((int)HttpStatusCode.BadRequest);
        response.Status.Should().NotBeNullOrEmpty();
        response.Status.Should().Be("Provide a hotel with at least one rate and a valid email address");
    }
    
    [Fact]
    public async Task Return_OK_Response_When_A_Hotel_With_Rates_Is_Provided_As_Value_In_Request()
    {
        // Arrange
        var randomHotel = UfyndHotelFactory.GenerateUfyndHotel();
            
        var reportFileRequest = new ReportFileRequest
        {
            Hotel = randomHotel
        };
        
        var reportingService = GetReportingService();
    
        // Act
        var response = await reportingService.GenerateExcelReportAsync(reportFileRequest);

        // Assert
        response.Code.Should().Be((int)HttpStatusCode.OK);
        response.Status.Should().NotBeNullOrEmpty();
        response.Status.Should().Be("Retrieved successfully");
        response.Data.FileContents.Should().NotBeNullOrEmpty();
        response.Data.FileContents.Length.Should().BeGreaterThan(0);
        response.Data.FileDownloadName.Should().NotBeNullOrEmpty();
        response.Data.FileDownloadName.Should().Contain(randomHotel.Hotel.Name.ToLower().Replace(" ", "_"));
        response.Data.FileDownloadName.Should().EndWith(".xlsx");


    }
}