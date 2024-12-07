using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AzureUserEntraIDApp.Pages;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

public class CustomHttpMessageHandler : HttpMessageHandler
{
    public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> SendAsyncFunc { get; set; } = (request, cancellationToken) => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return SendAsyncFunc(request, cancellationToken);
    }
}

namespace BlazorApp.NUnitTests
{
    [TestFixture]
    public class DisableUserTests : Bunit.TestContext
    {
        private CustomHttpMessageHandler customHttpMessageHandler = null!;
        private HttpClient httpClient = null!;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            customHttpMessageHandler = new CustomHttpMessageHandler();
            httpClient = new HttpClient(customHttpMessageHandler);
            httpClient = new HttpClient(customHttpMessageHandler)
            {
                BaseAddress = new System.Uri("https://programmingparrotcorp.com")
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

                if (request.Method == HttpMethod.Put && request.RequestUri != null && request.RequestUri.ToString().Contains("/api/DisableUser"))
                {
                    return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
                }

                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
            };
        }

        [Test]
        public void DisableUserComponent_ShouldRenderCorrectly()
        {
            // Act
            var cut = RenderComponent<DisableUser>(parameters => parameters.Add(p => p.userPrincipalName, "john.doe@example.com"));

            // Assert
            cut.Markup.Should().Contain("Disable User");
            cut.Markup.Should().Contain("First Name");
            cut.Markup.Should().Contain("Last Name");
            cut.Markup.Should().Contain("User Principal Name");
            cut.Markup.Should().Contain("Department");
        }

        [Test]
        public void DisableUserComponent_ShouldDisableUser()
        {
            // Act
            var cut = RenderComponent<DisableUser>(parameters => parameters.Add(p => p.userPrincipalName, "johndoe@shreyasrastogigmail.onmicrosoft.com"));

            cut.Find("button").Click();

            // Assert
            customHttpMessageHandler.SendAsyncFunc = (request, cancellationToken) =>
            {
                if (request.Method == HttpMethod.Put && request.RequestUri != null && request.RequestUri.ToString().Contains("/api/DisableUser"))
                {
                    var content = request.Content?.ReadAsStringAsync().Result;
                    content.Should().Contain("johndoe@shreyasrastogigmail.onmicrosoft.com");
                    return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
                }

                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
            };
        }
    }
}