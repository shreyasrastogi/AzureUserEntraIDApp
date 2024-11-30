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
    public class CreateUserFunction
    {
        private readonly ILogger _logger;
        private static readonly HttpClient httpClient = new HttpClient();

        public CreateUserFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<CreateUserFunction>();
        }

        [Function("CreateUser")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<NewUser>(requestBody);

            if (data == null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid user data.");
                return badResponse;
            }

            var token = await GetAccessTokenAsync();

            var user = new
            {
                accountEnabled = true,
                displayName = data.DisplayName,
                mailNickname = data.MailNickname,
                userPrincipalName = data.UserPrincipalName,
                passwordProfile = new
                {
                    forceChangePasswordNextSignIn = true,
                    password = data.Password
                }
            };

            var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.PostAsync("https://graph.microsoft.com/v1.0/users", content);

            if (response.IsSuccessStatusCode)
            {
                var successResponse = req.CreateResponse(HttpStatusCode.OK);
                await successResponse.WriteStringAsync("User created successfully.");
                return successResponse;
            }
            else
            {
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Error creating user.");
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

        public class NewUser
        {
            public string DisplayName { get; set; }
            public string MailNickname { get; set; }
            public string UserPrincipalName { get; set; }
            public string Password { get; set; }
        }
    }
}
