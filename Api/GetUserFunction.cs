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
    public class GetUserFunction
    {
        private readonly ILogger _logger;
        private static readonly HttpClient httpClient = new HttpClient();

        public GetUserFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GetUserFunction>();
        }

        [Function("GetUser")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "GetUser/{userPrincipalName}")] HttpRequestData req, string userPrincipalName)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                var token = await GetAccessTokenAsync();

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{userPrincipalName}?$select=givenName,surname,userPrincipalName,department");

                if (response.IsSuccessStatusCode)
                {
                    var userJson = await response.Content.ReadAsStringAsync();
                    var user = JsonConvert.DeserializeObject<User>(userJson);

                    var successResponse = req.CreateResponse(HttpStatusCode.OK);
                    successResponse.Headers.Add("Content-Type", "application/json");
                    await successResponse.WriteStringAsync(JsonConvert.SerializeObject(user));
                    return successResponse;
                }
                else
                {
                    _logger.LogError($"Error retrieving user: {response.ReasonPhrase}");
                    var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                    await errorResponse.WriteStringAsync("Error retrieving user.");
                    return errorResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Error retrieving user.");
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
            [JsonProperty("givenName")]
            public string FirstName { get; set; }
            [JsonProperty("surname")]
            public string LastName { get; set; }
            public string UserPrincipalName { get; set; }
            public string Department { get; set; }
        }
    }
}





