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
using Newtonsoft.Json.Linq;

namespace API
{
    public class DisableUserFunction
    {
        private readonly ILogger _logger;
        private static readonly HttpClient httpClient = new HttpClient();

        public DisableUserFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DisableUserFunction>();
        }

        [Function("DisableUser")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var user = JsonConvert.DeserializeObject<User>(requestBody);

            if (user == null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid user data.");
                return badResponse;
            }

            try
            {
                var token = await GetAccessTokenAsync();

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var patchObject = new JObject
                {
                    { "accountEnabled", false }
                };

                var content = new StringContent(patchObject.ToString(Formatting.None), Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"https://graph.microsoft.com/v1.0/users/{user.UserPrincipalName}")
                {
                    Content = content
                };

                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var successResponse = req.CreateResponse(HttpStatusCode.OK);
                    await successResponse.WriteStringAsync("User disabled successfully.");
                    return successResponse;
                }
                else
                {
                    _logger.LogError($"Error disabling user: {response.ReasonPhrase}");
                    var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                    await errorResponse.WriteStringAsync("Error disabling user.");
                    return errorResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Error disabling user.");
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




