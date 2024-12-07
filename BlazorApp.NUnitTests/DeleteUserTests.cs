using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AzureUserEntraIDApp.Pages;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;


namespace BlazorApp.NUnitTests
{
    [TestFixture]
    public class DeleteUserTests : Bunit.TestContext
    {
        private CustomHttpMessageHandler customHttpMessageHandler = null!;
        private HttpClient httpClient = null!;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            customHttpMessageHandler = new CustomHttpMessageHandler();
            var baseAddress = Environment.GetEnvironmentVariable("BASE_ADDRESS") ?? "http://localhost:7203";
            httpClient = new HttpClient(customHttpMessageHandler)
            {
                BaseAddress = new System.Uri(baseAddress)
            };
            Services.AddSingleton(httpClient);
        }

        [SetUp]
        public void Setup()
        {
            // Mock the GetFromJsonAsync method to return a user
            customHttpMessageHandler.SendAsyncFunc = (request, cancellationToken) =>
            {
                if (request.Method == HttpMethod.Get && request.RequestUri != null && request.RequestUri.ToString().Contains("/api/GetUser/"))
                {
                    return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new StringContent("{\"givenName\":\"John\",\"surname\":\"Doe\",\"UserPrincipalName\":\"john.doe@example.com\",\"Department\":\"IT\"}")
                    });
                }

                if (request.Method == HttpMethod.Delete && request.RequestUri != null && request.RequestUri.ToString().Contains("/api/DeleteUser"))
                {
                    return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
                }

                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
            };
        }

        [Test]
        public void DeleteUserComponent_ShouldRenderCorrectly()
        {
            // Act
            var cut = RenderComponent<DeleteUser>(parameters => parameters.Add(p => p.userPrincipalName, "john.doe@example.com"));

            // Assert
            cut.Markup.Should().Contain("Delete User");
            cut.Markup.Should().Contain("First Name");
            cut.Markup.Should().Contain("Last Name");
            cut.Markup.Should().Contain("User Principal Name");
            cut.Markup.Should().Contain("Department");
        }

        [Test]
        public void DeleteUserComponent_ShouldDeleteUser()
        {
            // Act
            var cut = RenderComponent<DeleteUser>(parameters => parameters.Add(p => p.userPrincipalName, "john.doe@example.com"));

            cut.Find("button").Click();

            // Assert
            customHttpMessageHandler.SendAsyncFunc = (request, cancellationToken) =>
            {
                if (request.Method == HttpMethod.Delete && request.RequestUri != null && request.RequestUri.ToString().Contains("/api/DeleteUser"))
                {
                    var content = request.Content?.ReadAsStringAsync().Result;
                    content.Should().Contain("john.doe@example.com");
                    return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
                }

                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
            };
        }
    }
}