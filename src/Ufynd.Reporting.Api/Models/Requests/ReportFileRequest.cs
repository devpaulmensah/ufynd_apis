using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using FluentValidation;
using Ufynd.Core.Models;
using Ufynd.Reporting.Api.Attributes;

namespace Ufynd.Reporting.Api.Models.Requests;

public class ReportFileRequest
{
    [Required]
    public UfyndHotel Hotel { get; set; }
    [RequiredIfAutomaticEmailSchedulerIsEnabled]
    public string RecipientEmailAddress { get; set; }
}

public class ReportFileRequestValidator : AbstractValidator<ReportFileRequest>
{
    public ReportFileRequestValidator()
    {
        RuleFor(x => x)
            .Must(x => x != null);

        RuleFor(x => x.Hotel)
            .Must(x =>
                x.HotelRates != null &&
                x.HotelRates.Count > 0);

        RuleFor(x => x.RecipientEmailAddress)
            .Must(x =>
            {
                if (string.IsNullOrEmpty(x)) return true;

                try
                {
                    var emailAddress = new MailAddress(x);
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }
            });
    }
}