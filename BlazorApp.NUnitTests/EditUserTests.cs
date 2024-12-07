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
    public class EditUserTests : Bunit.TestContext
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
                        Content = new StringContent("{\"givenName\":\"John\",\"surname\":\"Doe\",\"UserPrincipalName\":\"john.doe@example.com\",\"MailNickname\":\"jdoe\",\"DisplayName\":\"John Doe\",\"Department\":\"IT\"}")
                    });
                }

                if (request.Method == HttpMethod.Put && request.RequestUri != null && request.RequestUri.ToString().Contains("/api/UpdateUser"))
                {
                    return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
                }

                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
            };
        }

        [Test]
        public void EditUserComponent_ShouldRenderCorrectly()
        {
            // Act
            var cut = RenderComponent<EditUser>(parameters => parameters.Add(p => p.userPrincipalName, "john.doe@example.com"));

            // Assert
            cut.Markup.Should().Contain("Edit User");
            cut.Markup.Should().Contain("First Name");
            cut.Markup.Should().Contain("Last Name");
            cut.Markup.Should().Contain("User Principal Name");
            cut.Markup.Should().Contain("Nick Name");
            cut.Markup.Should().Contain("Display Name");
            cut.Markup.Should().Contain("Department");
        }

        [Test]
        public void EditUserComponent_ShouldUpdateUser()
        {
            // Act
            var cut = RenderComponent<EditUser>(parameters => parameters.Add(p => p.userPrincipalName, "john.doe@example.com"));

            cut.Find("#firstName").Change("John");
            cut.Find("#lastName").Change("Doe");
            cut.Find("#userPrincipalName").Change("john.doe@example.com");
            cut.Find("#nickName").Change("jdoe");
            cut.Find("#displayName").Change("John Doe");
            cut.Find("#department").Change("IT");

            cut.Find("form").Submit();

            // Assert
            customHttpMessageHandler.SendAsyncFunc = (request, cancellationToken) =>
            {
                if (request.Method == HttpMethod.Put && request.RequestUri != null && request.RequestUri.ToString().Contains("/api/UpdateUser"))
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

