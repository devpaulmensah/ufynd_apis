using System.Globalization;
using System.Net.Mime;
using Akka.Actor;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Ufynd.Core.Models;
using Ufynd.Core.Models.Responses;
using Ufynd.Reporting.Api.Actors;
using Ufynd.Reporting.Api.Actors.Messages;
using Ufynd.Reporting.Api.Models.Requests;
using Ufynd.Reporting.Api.Services.Interfaces;

namespace Ufynd.Reporting.Api.Services.Providers;

public class ReportingService : IReportingService
{
    private readonly ILogger<ReportingService> _logger;
    private readonly IConfiguration _configuration;

    public ReportingService(ILogger<ReportingService> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }
    
    public async Task<BaseResponse<FileContentResult>> GenerateExcelReportAsync(ReportFileRequest request)
    {
        try
        {
            var validationResult = await new ReportFileRequestValidator().ValidateAsync(request);

            if (!validationResult.IsValid)
            {
                return CommonResponses.ErrorResponse
                    .BadRequestResponse<FileContentResult>("Provide a hotel with at least one rate and a valid email address");
            }
            
            // Create excel file with non commercial license
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var excelPackage = new ExcelPackage())
            {
                // Create worksheet
                var worksheet = excelPackage.Workbook.Worksheets.Add("Hotel Report");
                
                // Set default properties for the worksheet
                worksheet.DefaultRowHeight = 12;
                worksheet.TabColor = System.Drawing.Color.Black;
                
                // Set default properties for headers
                worksheet.Row(1).Height = 20;
                worksheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Row(1).Style.Font.Bold = true;
                
                // Headers for the excel report
                string[] reportHeaders = new string[]
                {
                    "ARRIVAL_DATE", "DEPARTURE_DATE", "PRICE", "CURRENCY", "RATENAME", "ADULTS", "BREAKFAST_INCLUDED"
                };
                
                // Set header for worksheet
                for (int i = 0; i < reportHeaders.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = reportHeaders[i];
                }
                
                /*
                 * Since we have already inserted headers into row one, we will start inserting content
                 * from row 2 using a for each loop and increase for each content
                 */
                int contentIndex = 2;

                foreach (var hotelRate in request.Hotel.HotelRates)
                {
                    worksheet.Cells[contentIndex, 1].Value = FormatReportDate(hotelRate.TargetDay);
                    worksheet.Cells[contentIndex, 2].Value = FormatReportDate(hotelRate.TargetDay.AddDays(1));
                    worksheet.Cells[contentIndex, 3].Value = GetAmountInGermanFormat(hotelRate.Price.NumericFloat);
                    worksheet.Cells[contentIndex, 4].Value = hotelRate.Price.Currency.ToUpper();
                    worksheet.Cells[contentIndex, 5].Value = hotelRate.RateName;
                    worksheet.Cells[contentIndex, 6].Value = hotelRate.Adults;
                    worksheet.Cells[contentIndex, 7].Value = DoesRateIncludeBreakfast(hotelRate.RateTags);
                    
                    contentIndex++;
                }

                // Convert to bytes array to create file to be returned
                byte[] fileBytes = await excelPackage.GetAsByteArrayAsync();

                DateTime fileNameDate = DateTime.UtcNow;
                string fileName = $"{request.Hotel.Hotel.Name.ToLower().Replace(" ", "_")}_{fileNameDate:yyyyMMddHHmmss}.xlsx";

                var fileContentResult = new FileContentResult(fileBytes, MediaTypeNames.Application.Octet);
                fileContentResult.FileDownloadName = fileName;

                // Store file and save time to trigger automated email sender
                bool useAutomatedEmailSender = _configuration.GetValue<bool>("UseAutomatedEmailSender");

                if (useAutomatedEmailSender)
                {
                    // Get time to trigger sending automated email
                    int timeToTriggerAutomatedEmailIntervalInMinutes = _configuration.GetValue<int>("TimeToTriggerAutomatedEmailIntervalInMinutes");
                    DateTime timeToSendEmail = DateTime.UtcNow.AddMinutes(timeToTriggerAutomatedEmailIntervalInMinutes);

                    string emailSubject = $"{request.Hotel.Hotel.Name}: Report for {fileNameDate:yyyy-MM-dd h:mm tt}";
                    ParentActor.UploadActor
                        .Tell(new UploadFileMessage(fileBytes, fileName, timeToSendEmail, request.RecipientEmailAddress, emailSubject), ActorRefs.Nobody);
                }
                
                return CommonResponses.SuccessResponse.OkResponse(fileContentResult);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured generating excel report");
            return CommonResponses.ErrorResponse.InternalServerErrorResponse<FileContentResult>();
        }
    }
    
    private static string FormatReportDate(DateTime dateTime) => dateTime.ToString("dd.MM.yy");

    private static int DoesRateIncludeBreakfast(IEnumerable<HotelRateTag> rateTags)
    {
        // Loop through hotel rates if there is breakfast included and return 1 if true,
        // else 0 if false
        return rateTags.Any(x => "breakfast".Equals(x.Name, StringComparison.OrdinalIgnoreCase) && x.Shape)
            ? 1
            : 0;
    }

    private static string GetAmountInGermanFormat(decimal amount)
    {
        return amount
            .ToString("C", new CultureInfo("de-DE"))
            .Replace("€", "");
    }
        
}