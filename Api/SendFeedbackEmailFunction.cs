using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Azure.Communication.Email;
using Azure.Core.Pipeline;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace API
{
    public class SendFeedbackEmailFunction
    {
        private readonly ILogger _logger;
        private readonly EmailClient _emailClient;

        public SendFeedbackEmailFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SendFeedbackEmailFunction>();
            var connectionString = Environment.GetEnvironmentVariable("AzureCommunicationServicesConnectionString");
            var emailClientOptions = new EmailClientOptions
            {
                Transport = new HttpClientTransport(new HttpClient
                {
                    Timeout = TimeSpan.FromMinutes(5) // Set custom timeout to 5 minutes
                })
            };
            _emailClient = new EmailClient(connectionString, emailClientOptions);
        }

        [Function("SendFeedbackEmail")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request to send feedback email.");

            try
            {
                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var emailRequest = JsonConvert.DeserializeObject<EmailRequest>(requestBody);

                var emailContent = $"Hi,\n\n{emailRequest.FeedbackResponse}\n\nFeedback Entered: {emailRequest.UserText}\n\nThanks,\nProgramming Parrot";

                var emailMessage = new EmailMessage(
                    senderAddress: "DoNotReply@c9c38a17-ff68-42d4-826e-4df5942cef1b.azurecomm.net", // Use the verified sender email
                    content: new EmailContent("User Feedback")
                    {
                        PlainText = emailContent
                    },
                    recipients: new EmailRecipients(new List<EmailAddress>
                    {
                        new EmailAddress(emailRequest.RecipientEmail)
                    })
                );

                // Fire and forget the email sending task
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailClient.SendAsync(Azure.WaitUntil.Completed, emailMessage);
                        _logger.LogInformation("Email sent successfully.");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Exception while sending email: {ex.Message}");
                    }
                });

                var successResponse = req.CreateResponse(HttpStatusCode.OK);
                await successResponse.WriteStringAsync("Email request received.");
                return successResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Error processing email request.");
                return errorResponse;
            }
        }

        public class EmailRequest
        {
            public string RecipientEmail { get; set; }
            public string UserText { get; set; }
            public string FeedbackResponse { get; set; }
        }
    }
}