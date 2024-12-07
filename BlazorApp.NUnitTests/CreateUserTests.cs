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
    public class CreateUserTests : Bunit.TestContext
    {
        private CustomHttpMessageHandler customHttpMessageHandler = null!;
        private HttpClient httpClient = null!;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            customHttpMessageHandler = new CustomHttpMessageHandler();
            httpClient = new HttpClient(customHttpMessageHandler)
            {
                BaseAddress = new System.Uri("https://programmingparrotcorp.com")
            };
            Services.AddSingleton(httpClient);
        }

        [SetUp]
        public void Setup()
        {
            // Mock the PostAsJsonAsync method to return a successful response
            customHttpMessageHandler.SendAsyncFunc = (request, cancellationToken) =>
            {
                if (request.Method == HttpMethod.Post && request.RequestUri != null && request.RequestUri.ToString().Contains("/api/CreateUser"))
                {
                    return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
                }

                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
            };
        }

        [Test]
        public void CreateUserComponent_ShouldRenderCorrectly()
        {
            // Act
            var cut = RenderComponent<CreateUser>();

            // Assert
            cut.Markup.Should().Contain("Create User");
            cut.Markup.Should().Contain("First Name");
            cut.Markup.Should().Contain("Last Name");
            cut.Markup.Should().Contain("User Principal Name");
            cut.Markup.Should().Contain("Nick Name");
            cut.Markup.Should().Contain("Display Name");
            cut.Markup.Should().Contain("Department");
            cut.Markup.Should().Contain("Other Email");
            cut.Markup.Should().Contain("Password");
        }

        [Test]
        public void CreateUserComponent_ShouldSubmitForm()
        {
            // Act
            var cut = RenderComponent<CreateUser>();

            cut.Find("#firstName").Change("John");
            cut.Find("#lastName").Change("Doe");
            cut.Find("#userPrincipalName").Change("John.Doe");
            cut.Find("#nickName").Change("JD");
            cut.Find("#displayName").Change("John Doe");
            cut.Find("#department").Change("IT");
            cut.Find("#email").Change("john.doe@example.com");
            cut.Find("#password").Change("Password123");

            cut.Find("form").Submit();

            // Assert
            customHttpMessageHandler.SendAsyncFunc = (request, cancellationToken) =>
            {
                if (request.Method == HttpMethod.Post && request.RequestUri != null && request.RequestUri.ToString().Contains("/api/CreateUser"))
                {
                    var content = request.Content?.ReadAsStringAsync().Result;
                    content.Should().Contain("John.Doe@shreyasrastogigmail.onmicrosoft.com");
                    return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
                }

                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
            };
        }
    }
}

