namespace Ufynd.Reporting.Api.Actors.Messages;

public struct CacheMessage
{
    public string Filename { get; set; }
    public DateTime ScheduledTime { get; set; }
    public string RecipientEmailAddress { get; set; }
    public string EmailSubject { get; set; }

    public CacheMessage(string filename, DateTime scheduledTime, string recipientEmailAddress, string emailSubject)
    {
        Filename = filename;
        ScheduledTime = scheduledTime;
        RecipientEmailAddress = recipientEmailAddress;
        EmailSubject = emailSubject;
    }
}