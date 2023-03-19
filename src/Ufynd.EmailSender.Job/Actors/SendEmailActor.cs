using Akka.Actor;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using Ufynd.Core.Constants;
using Ufynd.EmailSender.Job.Actors.Messages;
using Ufynd.EmailSender.Job.Configurations;
using Ufynd.EmailSender.Job.Services.Interfaces;
using Ufynd.FileUpload.Sdk.Services.Interfaces;
using Ufynd.Redis.Sdk.Services.Interfaces;

namespace Ufynd.EmailSender.Job.Actors;

public class SendEmailActor : ReceiveActor
{
    private readonly ILogger<SendEmailActor> _logger;
    private readonly IEmailService _emailService;
    private readonly IRedisService _redisService;
    private readonly IUploadService _uploadService;
    private readonly EmailConfiguration _emailConfiguration;

    public SendEmailActor(ILogger<SendEmailActor> logger,
        IEmailService emailService,
        IRedisService redisService,
        IUploadService uploadService,
        IOptions<EmailConfiguration> emailConfiguration)
    {
        _logger = logger;
        _emailService = emailService;
        _redisService = redisService;
        _uploadService = uploadService;
        _emailConfiguration = emailConfiguration.Value;

        ReceiveAsync<CacheMessage>(SendEmail);
    }

    private async Task SendEmail(CacheMessage message)
    {
        try
        {
            /*
             * Steps to process message and send email
             * 1. Remove message from redis to prevent reading again to trigger sending email when time to send is not up
             * 2. Download file to be added as attachment to email
             * 3. Send email with attachment using a retry policy since the email record has been deleted from redis.
             * 4. Delete file from AWS to save up space and cost
             */
            
            // Remove from redis
            bool isRemoved = await _redisService.GetRedisClient()
                .HashDeleteAsync(RedisConstants.ScheduledEmailsKey, message.Filename);
            
            if (!isRemoved)
            {
                _logger.LogInformation("An error occured removing email details from redis\n{emailDetails}", 
                    JsonConvert.SerializeObject(message));
                
                return;
            }

            AsyncRetryPolicy<bool> retrySendingEmailPolicy = GetRetryPolicy();

            bool emailSentSuccessfully = await retrySendingEmailPolicy.ExecuteAsync(async () =>
            {
                byte[] fileBytes = await _uploadService.DownloadFileAsync(message.Filename);

                if (!fileBytes.Any())
                {
                    _logger.LogError("An error occured downloading file:{filename}", message.Filename);
                    return false;
                }

                (bool emailSent, string responseMessage) = await _emailService.SendEmailWithAttachmentAsync(
                    message.RecipientEmailAddress, 
                    message.EmailSubject, 
                    fileBytes, 
                    message.Filename);

                if (!emailSent)
                {
                    _logger.LogError("An error occured sending email to {emailRecipient}\nError:{errorResponse}", 
                        message.RecipientEmailAddress, responseMessage);
                }
                
                return emailSent;
            });

            if (emailSentSuccessfully)
            {
                _logger.LogInformation("Report email with subject:{emailSubject} successfully sent to {emailRecipient}", 
                    message.EmailSubject, message.RecipientEmailAddress);
            }
            
            // Delete file from AWS
            await _uploadService.DeleteFileAsync(message.Filename);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occured sending email to {emailRecipient}", message.RecipientEmailAddress);
        }
    }

    /*
     * This method generates a retry policy based on the specified number of times to retry sending an email and
     * the seconds to wait till the next retry
     */
    private AsyncRetryPolicy<bool> GetRetryPolicy()
    {
        int retryIntervalInSeconds = _emailConfiguration.IntervalToRetrySendingEmailInSeconds;
        int maximumRetries = _emailConfiguration.NumberOfTimesToRetrySendingEmailAfterFailure;
            
        var stepDurations = new List<TimeSpan>
        {
            /*
             * Let's start our interval with our retry interval since our request will run
             * before using our retries interval
             */
            TimeSpan.FromSeconds(retryIntervalInSeconds)
        };
            
        /*
         * Here, we get the previous timespan and add our retry intervals to generate our next timespan
         */
            
        int previousTimeSpan = stepDurations.First().Seconds;
        for (int i = 1; i < maximumRetries - 1; i++)
        {
            int timeSpanNow = previousTimeSpan + retryIntervalInSeconds;
            stepDurations.Add(TimeSpan.FromSeconds(timeSpanNow));

            previousTimeSpan = timeSpanNow;
        }

        return Policy
            .HandleResult<bool>(x => x == false)
            .WaitAndRetryAsync(stepDurations);
    }
}