using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Ufynd.Core.Models;
using Ufynd.Core.Models.Responses;
using Ufynd.Reporting.Api.Controllers;
using Ufynd.Reporting.Api.Models.Requests;
using Ufynd.Reporting.Api.Services.Interfaces;
using Ufynd.Reporting.Api.Tests.TestSetup;
using Xunit;

namespace Ufynd.Reporting.Api.Tests;

public class ReportsControllerShould : IClassFixture<TestFixture>
{
    private readonly Mock<IReportingService> _reportingServiceMock;

    public ReportsControllerShould()
    {
        _reportingServiceMock = new Mock<IReportingService>();
    }

    private ReportsController GetReportsController() =>
        new ReportsController(_reportingServiceMock.Object);

    [Fact]
    public async Task Return_Bad_Request_Response_When_A_Null_Hotel_Is_Provided_As_Value_In_Request()
    {
        // Arrange
        var reportFileRequest = new ReportFileRequest();

        _reportingServiceMock.Setup(x => x.GenerateExcelReportAsync(It.IsAny<ReportFileRequest>()))
            .ReturnsAsync(new BaseResponse<FileContentResult>
            {
                Code = (int)HttpStatusCode.BadRequest
            });

        var reportsController = GetReportsController();

        // Act
        var response = (ObjectResult)await reportsController.GenerateReport(reportFileRequest);

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        var parsedValue = response.Value as BaseResponse<UfyndHotel>;

        parsedValue?.Code.Should().Be(response.StatusCode);
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

        _reportingServiceMock.Setup(x => x.GenerateExcelReportAsync(It.IsAny<ReportFileRequest>()))
            .ReturnsAsync(new BaseResponse<FileContentResult>
            {
                Code = (int)HttpStatusCode.BadRequest
            });

        var reportsController = GetReportsController();

        // Act
        var response = (ObjectResult)await reportsController.GenerateReport(reportFileRequest);

        // Assert
        response.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        var parsedValue = response.Value as BaseResponse<UfyndHotel>;

        parsedValue?.Code.Should().Be(response.StatusCode);
    }
}