using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using API.Models;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.Extensions.Configuration;

namespace API.Functions
{
    public class UserFeedbackFunction
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _endpoint;

        public UserFeedbackFunction(ILoggerFactory loggerFactory, HttpClient httpClient, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<UserFeedbackFunction>();
            _httpClient = httpClient;
             _apiKey = configuration["SentimentAnalysis:ApiKey"];
             _endpoint = configuration["SentimentAnalysis:Endpoint"];

            //_apiKey = "";
            //_endpoint = "";
        }

        [Function("UserFeedback")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("Processing user feedback request.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var userFeedback = JsonConvert.DeserializeObject<UserFeedback>(requestBody);

                if (userFeedback == null || string.IsNullOrEmpty(userFeedback.Text) || string.IsNullOrEmpty(userFeedback.Email) || string.IsNullOrEmpty(userFeedback.PhoneNumber))
                {
                    var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                    await badResponse.WriteStringAsync("Invalid feedback data.");
                    return badResponse;
                }

                // Perform sentiment analysis
                userFeedback.Sentiment = await AnalyzeSentimentAsync(userFeedback.Text);

                // Log the received feedback and sentiment
                _logger.LogInformation($"Received feedback: Text={userFeedback.Text}, Email={userFeedback.Email}, PhoneNumber={userFeedback.PhoneNumber}, Sentiment={userFeedback.Sentiment}");

                var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
                await response.WriteStringAsync($"Received feedback: Text={userFeedback.Text}, Email={userFeedback.Email}, PhoneNumber={userFeedback.PhoneNumber}, Sentiment={userFeedback.Sentiment}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the feedback request.");
                var errorResponse = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("An error occurred while processing your request.");
                return errorResponse;
            }
        }

        private async Task<string> AnalyzeSentimentAsync(string text)
        {
            var sentimentRequest = new
            {
                documents = new[]
                {
                    new { id = "1", language = "en", text = text }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_endpoint}/text/analytics/v3.0/sentiment")
            {
                Content = JsonContent.Create(sentimentRequest)
            };
            request.Headers.Add("Ocp-Apim-Subscription-Key", _apiKey);

            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var sentimentResult = await response.Content.ReadFromJsonAsync<SentimentResult>();
                return sentimentResult?.documents?.FirstOrDefault()?.sentiment ?? "Unknown";
            }
            else
            {
                return "Error";
            }
        }

        private class SentimentResult
        {
            public Document[] documents { get; set; }
        }

        private class Document
        {
            public string id { get; set; }
            public string sentiment { get; set; }
        }
    }
}
