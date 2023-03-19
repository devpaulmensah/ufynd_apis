namespace Ufynd.Reporting.Api.Actors.Messages;

public struct UploadFileMessage
{
    public byte[] FileBytes { get; set; }
    public string Filename { get; set; }
    public DateTime ScheduledTime { get; set; }
    public string RecipientEmailAddress { get; set; }
    public string EmailSubject { get; set; }

    public UploadFileMessage(byte[] file, string filename, DateTime scheduledTime, string recipientEmailAddress, string emailSubject)
    {
        FileBytes = file;
        Filename = filename;
        ScheduledTime = scheduledTime;
        RecipientEmailAddress = recipientEmailAddress;
        EmailSubject = emailSubject;
    }
}