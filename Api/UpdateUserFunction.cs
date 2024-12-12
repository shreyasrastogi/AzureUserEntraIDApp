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
    public class UpdateUserFunction
    {
        private readonly ILogger _logger;
        private static readonly HttpClient httpClient = new HttpClient();

        public UpdateUserFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<UpdateUserFunction>();
        }

        [Function("UpdateUser")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var incomingUser = JsonConvert.DeserializeObject<User>(requestBody);

            if (incomingUser == null)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid user data.");
                return badResponse;
            }

            try
            {
                var token = await GetAccessTokenAsync();

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Fetch the existing user data
                var existingUserResponse = await httpClient.GetAsync($"https://graph.microsoft.com/v1.0/users/{incomingUser.UserPrincipalName}");
                if (!existingUserResponse.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error fetching existing user: {existingUserResponse.ReasonPhrase}");
                    var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                    await errorResponse.WriteStringAsync("Error fetching existing user.");
                    return errorResponse;
                }

                var existingUserJson = await existingUserResponse.Content.ReadAsStringAsync();
                var existingUser = JsonConvert.DeserializeObject<User>(existingUserJson);

                // Generate the patch payload with only the changed fields
                var patchPayload = GeneratePatchPayload(incomingUser, existingUser);

                var content = new StringContent(patchPayload, Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"https://graph.microsoft.com/v1.0/users/{incomingUser.UserPrincipalName}")
                {
                    Content = content
                };

                var response = await httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var successResponse = req.CreateResponse(HttpStatusCode.OK);
                    await successResponse.WriteStringAsync("User updated successfully.");
                    return successResponse;
                }
                else
                {
                    _logger.LogError($"Error updating user: {response.ReasonPhrase}");
                    var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                    await errorResponse.WriteStringAsync("Error updating user.");
                    return errorResponse;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exception: {ex.Message}");
                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteStringAsync("Error updating user.");
                return errorResponse;
            }
        }

        private static string GeneratePatchPayload(User incomingUser, User existingUser)
        {
            var patchObject = new JObject();

            if (!string.Equals(incomingUser.FirstName, existingUser.FirstName))
            {
                patchObject["givenName"] = incomingUser.FirstName;
            }

            if (!string.Equals(incomingUser.LastName, existingUser.LastName))
            {
                patchObject["surname"] = incomingUser.LastName;
            }

            if (!string.Equals(incomingUser.UserPrincipalName, existingUser.UserPrincipalName))
            {
                patchObject["userPrincipalName"] = incomingUser.UserPrincipalName;
            }

            if (!string.Equals(incomingUser.MailNickname, existingUser.MailNickname))
            {
                patchObject["mailNickname"] = incomingUser.MailNickname;
            }

            if (!string.Equals(incomingUser.DisplayName, existingUser.DisplayName))
            {
                patchObject["displayName"] = incomingUser.DisplayName;
            }

            if (!string.Equals(incomingUser.Department, existingUser.Department))
            {
                patchObject["department"] = incomingUser.Department;
            }

            return patchObject.ToString(Formatting.None);
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
            public string MailNickname { get; set; }
            public string DisplayName { get; set; }
            public string Department { get; set; }
        }
    }
}