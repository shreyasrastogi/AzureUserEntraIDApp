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
    public class AssignLicenseFunction
    {
        private readonly ILogger _logger;
        private static readonly HttpClient httpClient = new HttpClient();

        public AssignLicenseFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AssignLicenseFunction>();
        }

        [Function("AssignLicense")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var requestData = JsonConvert.DeserializeObject<LicenseRequest>(requestBody);

            if (requestData == null || string.IsNullOrEmpty(requestData.UserPrincipalName) || string.IsNullOrEmpty(requestData.LicenseType))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid request data.");
                return badRequestResponse;
            }

            var token = await GetAccessTokenAsync();

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var skuId = await GetSkuIdAsync(requestData.LicenseType, token);
            if (string.IsNullOrEmpty(skuId))
            {
                var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteStringAsync("Invalid license type.");
                return badRequestResponse;
            }

            var content = new StringContent(JsonConvert.SerializeObject(new
            {
                addLicenses = new[]
                {
                    new
                    {
                        skuId = skuId
                    }
                },
                removeLicenses = new string[] { }
            }), Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync($"https://graph.microsoft.com/v1.0/users/{requestData.UserPrincipalName}/assignLicense", content);

            if (response.IsSuccessStatusCode)
            {
                var successResponse = req.CreateResponse(HttpStatusCode.OK);
                await successResponse.WriteStringAsync("License assigned successfully.");
                return successResponse;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error assigning license: {response.ReasonPhrase}, Details: {errorContent}");
                var errorResponse = req.CreateResponse(response.StatusCode);
                await errorResponse.WriteStringAsync($"Error assigning license: {response.ReasonPhrase}, Details: {errorContent}");
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

        private static async Task<string> GetSkuIdAsync(string licenseType, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.GetAsync("https://graph.microsoft.com/v1.0/subscribedSkus");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var skus = JsonConvert.DeserializeObject<SubscribedSkusResponse>(content);

                foreach (var sku in skus.Value)
                {
                    if (sku.SkuPartNumber.Equals(licenseType, StringComparison.OrdinalIgnoreCase))
                    {
                        return sku.SkuId;
                    }
                }
            }

            return null;
        }

        public class LicenseRequest
        {
            public string UserPrincipalName { get; set; }
            public string LicenseType { get; set; }
        }

        public class SubscribedSkusResponse
        {
            [JsonProperty("value")]
            public List<SubscribedSku> Value { get; set; }
        }

        public class SubscribedSku
        {
            [JsonProperty("skuId")]
            public string SkuId { get; set; }

            [JsonProperty("skuPartNumber")]
            public string SkuPartNumber { get; set; }
        }
    }
}
