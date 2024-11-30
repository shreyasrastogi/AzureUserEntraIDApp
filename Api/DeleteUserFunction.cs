using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Newtonsoft.Json;

namespace API
{
    public class DeleteUserFunction
    {
        private readonly ILogger _logger;
        private static readonly HttpClient httpClient = new HttpClient();

        public DeleteUserFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DeleteUserFunction>();
        }

        [Function("DeleteUser")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "DeleteUser/{userPrincipalName}")] HttpRequestData req, string userPrincipalName)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                var token = await GetAccessTokenAsync();

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.DeleteAsync($"https://graph.microsoft.com/v1.0/users/{userPrincipalName}");

                if (response.IsSuccessStatusCode)
                {
                    var successResponse = req.CreateResponse(HttpStatusCode.OK);
                    await successResponse.WriteStringAsync("User deleted successfully.");
                    return successResponse;
                }
                else
                {
                    _logger.LogError($"Error deleting user: {response.ReasonPhrase}");
                    var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                    await errorResponse.WriteStringAsync("Error deleting user.");
                    return errorResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Error deleting user.");
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

        public class User
        {
            public string DisplayName { get; set; }
            public string MailNickname { get; set; }
            public string UserPrincipalName { get; set; }
        }
    }
}
