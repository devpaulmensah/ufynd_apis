namespace Ufynd.EmailSender.Job.Services.Interfaces;

public interface IEmailService
{
    Task<(bool, string)> SendEmailWithAttachmentAsync(string email, string subject, byte[] bytes, string filename);
}