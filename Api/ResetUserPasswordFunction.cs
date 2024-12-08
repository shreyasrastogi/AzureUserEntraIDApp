using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json;

namespace API
{
    public class ResetUserPasswordFunction
    {
        private readonly ILogger _logger;
        private static readonly HttpClient httpClient = new HttpClient();

        public ResetUserPasswordFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ResetUserPasswordFunction>();
        }

        [Function("ResetUserPassword")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "ResetUserPassword/{userPrincipalName}")] HttpRequestData req, string userPrincipalName)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request to reset user password.");

            try
            {
                var token = await GetAccessTokenAsync();

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var passwordResetRequest = JsonConvert.DeserializeObject<PasswordResetRequest>(requestBody);

                var content = new StringContent(JsonConvert.SerializeObject(new
                {
                    passwordProfile = new
                    {
                        forceChangePasswordNextSignIn= true,
                        password = passwordResetRequest.NewPassword
                    }
                }), Encoding.UTF8, "application/json");

                var requestMessage = new HttpRequestMessage(new HttpMethod("PATCH"), $"https://graph.microsoft.com/v1.0/users/{userPrincipalName}")
                {
                    Content = content
                };

                var response = await httpClient.SendAsync(requestMessage);

                if (response.IsSuccessStatusCode)
                {
                    var successResponse = req.CreateResponse(HttpStatusCode.OK);
                    await successResponse.WriteStringAsync("User password reset successfully.");
                    return successResponse;
                }
                else
                {
                    _logger.LogError($"Error resetting user password: {response.ReasonPhrase}");
                    var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                    await errorResponse.WriteStringAsync("Error resetting user password.");
                    return errorResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Error resetting user password.");
                return errorResponse;
            }
        }

        private static async Task<string> GetAccessTokenAsync()
        {
            var clientId = Environment.GetEnvironmentVariable("ClientId");
            var tenantId = Environment.GetEnvironmentVariable("TenantId");
            var clientSecret = Environment.GetEnvironmentVariable("ClientSecret");

            var confidentialClient = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithClientSecret(clientSecret)
                .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
                .Build();

            var authResult = await confidentialClient.AcquireTokenForClient(new[] { "https://graph.microsoft.com/.default" }).ExecuteAsync();
            return authResult.AccessToken;
        }

        public class PasswordResetRequest
        {
            public string NewPassword { get; set; }
        }
    }
}
