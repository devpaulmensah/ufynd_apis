using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using Newtonsoft.Json;
using Ufynd.EmailSender.Job.Configurations;
using Ufynd.EmailSender.Job.Services.Interfaces;

namespace Ufynd.EmailSender.Job.Services.Providers;

public class EmailService : IEmailService
{
    private readonly EmailConfiguration _emailConfiguration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailConfiguration> emailConfiguration,
        ILogger<EmailService> logger)
    {
        _emailConfiguration = emailConfiguration.Value;
        _logger = logger;
    }

    public async Task<(bool, string)> SendEmailWithAttachmentAsync(string email, string subject, byte[] bytes, string filename)
    {
        try
        {
            // Ensure file (byte array) is not empty
            if (!bytes.Any())
            {
                return (false, "File must not be empty");
            }
            
            (bool messageCreatedSuccessfully, MimeMessage emailMessage) = CreateMessage(email, subject, bytes, filename);

            if (!messageCreatedSuccessfully)
            {
                _logger.LogError("An error occured creating creating email message\n{emailMessage}", 
                    JsonConvert.SerializeObject(new { email, subject, filename }, Formatting.Indented));

                return (false, "An error occured creating email content");
            }
            
            bool isMailSent = await SendMail(emailMessage);

            return (isMailSent, isMailSent ? 
                "Email sent successfully" 
                : "An error occured sending email");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured sending email to {email} with subject, {subject}",
                email, subject);

            return (false, e.Message);
        }
    }
    
    private async Task<bool> SendMail(MimeMessage message)
    {
        bool isSuccessful;
        
        using (var smtpClient = new SmtpClient())
        {
            try
            {
                await smtpClient.ConnectAsync(_emailConfiguration.Host, _emailConfiguration.Port, true);
                smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");
                await smtpClient.AuthenticateAsync(_emailConfiguration.From, _emailConfiguration.Password);

                string response = await smtpClient.SendAsync(message);
                    
                _logger.LogInformation("Response from sending email\n{response}", response);
                isSuccessful = true;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occured sending email");
                isSuccessful = false;
            }
            finally
            {
                await smtpClient.DisconnectAsync(true);
                smtpClient.Dispose();
            }
        }

        return isSuccessful;
    }
    
    private (bool successful, MimeMessage mail) CreateMessage(string email, string subject, byte[] bytes, string filename)
    {
        try
        {
            var mail = new MimeMessage();
            mail.Subject = subject;
            mail.To.Add(new MailboxAddress("", email));
            mail.From.Add(new MailboxAddress("Ufynd Reporting", _emailConfiguration.From));

            var builder = new BodyBuilder
            {
                HtmlBody = "Kindly find attached the report for your hotel"
            };

            builder.Attachments.Add(filename, bytes);

            mail.Body = builder.ToMessageBody();

            return (true, mail);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured creating mail");

            return (false, null);
        }
    }
}