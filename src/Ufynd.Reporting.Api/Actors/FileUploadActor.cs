using Akka.Actor;
using Newtonsoft.Json;
using Ufynd.FileUpload.Sdk.Models;
using Ufynd.FileUpload.Sdk.Services.Interfaces;
using Ufynd.Reporting.Api.Actors.Messages;

namespace Ufynd.Reporting.Api.Actors;

public class FileUploadActor : ReceiveActor
{
    
    private readonly IUploadService _uploadService;
    private readonly ILogger<FileUploadActor> _logger;

    public FileUploadActor(IUploadService uploadService,
        ILogger<FileUploadActor> logger)
    {
        _uploadService = uploadService;
        _logger = logger;

        ReceiveAsync<UploadFileMessage>(DoUploadFile);
    }

    private async Task DoUploadFile(UploadFileMessage message)
    {
        try
        {
            // Upload file to AWS S3 to retrieve later and send as attachment to email when time is up
            // to send it
            SdkResponse<string> uploadResponse;
            using (MemoryStream stream = new MemoryStream())
            {
                await stream.WriteAsync(message.FileBytes, 0, message.FileBytes.Length);
                stream.Position = 0;
                
                uploadResponse = await _uploadService.UploadFileAsync(stream, message.Filename);
            }
            
            if (uploadResponse.IsSuccessful)
            {
                ParentActor.CacheActor.Tell(new CacheMessage(message.Filename, message.ScheduledTime, message.RecipientEmailAddress, message.EmailSubject));
                return;
            }
            
            _logger.LogError("An error occured uploading file to aws\n{response}", 
                 JsonConvert.SerializeObject(uploadResponse));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured uploading file for email to be sent");
        }
    }
    
}