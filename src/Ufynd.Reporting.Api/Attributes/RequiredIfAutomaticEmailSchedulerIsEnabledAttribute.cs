using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace Ufynd.Reporting.Api.Attributes;

public class RequiredIfAutomaticEmailSchedulerIsEnabledAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var configuration = validationContext.GetService<IConfiguration>();
        bool useAutomatedEmailSender = configuration.GetValue<bool>("UseAutomatedEmailSender");

        // Ensure email is provided and value provided is a valid email address
        if (!useAutomatedEmailSender) return ValidationResult.Success;
        
        string recipientEmailAddress = (string) value;

        if (string.IsNullOrEmpty(recipientEmailAddress))
        {
            return new ValidationResult("Provide recipient's email address");
        }
            
        // Check if email is a valid email address
        return IsValidEmailAddress(recipientEmailAddress)
            ? ValidationResult.Success
            : new ValidationResult("Email address is invalid");
    }

    private static bool IsValidEmailAddress(string recipientEmailAddress)
    {
        try
        {
            var _ = new MailAddress(recipientEmailAddress);
        }
        catch (Exception e)
        {
            return false;
        }

        return true;
    }
}