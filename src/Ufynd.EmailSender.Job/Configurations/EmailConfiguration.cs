namespace Ufynd.EmailSender.Job.Configurations;

public class EmailConfiguration
{
    public string From { get; set; }
    public string Password { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
    public int IntervalToRetrySendingEmailInSeconds { get; set; }
    public int NumberOfTimesToRetrySendingEmailAfterFailure { get; set; }
}