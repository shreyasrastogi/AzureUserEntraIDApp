using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AzureUserEntraIDApp.Pages;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Collections.Generic;
using System.Net.Http.Json;

namespace BlazorApp.NUnitTests
{
    [TestFixture]
    public class AssignLicenseTests : Bunit.TestContext
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
            // Mock the GetFromJsonAsync and PostAsJsonAsync methods
            customHttpMessageHandler.SendAsyncFunc = (request, cancellationToken) =>
            {
                if (request.Method == HttpMethod.Get && request.RequestUri != null && request.RequestUri.ToString().Contains("/api/GetUsers"))
                {
                    var users = new List<AssignLicense.User>
                    {
                        new AssignLicense.User { DisplayName = "John Doe", UserPrincipalName = "john.doe@example.com" }
                    };
                    return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(users)
                    });
                }
                else if (request.Method == HttpMethod.Post && request.RequestUri != null && request.RequestUri.ToString().Contains("/api/AssignLicense"))
                {
                    return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
                }

                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
            };
        }

        [Test]
        public void AssignLicenseComponent_ShouldRenderCorrectly()
        {
            // Act
            var cut = RenderComponent<AssignLicense>();

            // Assert
            cut.Markup.Should().Contain("Assign License");
            cut.Markup.Should().Contain("Select User");
            cut.Markup.Should().Contain("License Type");
        }

        [Test]
        public void AssignLicenseComponent_ShouldSubmitForm()
        {
            // Act
            var cut = RenderComponent<AssignLicense>();

            cut.Find("#userPrincipalName").Change("john.doe@example.com");
            cut.Find("#licenseType").Change("M365_F1_COMM");

            cut.Find("form").Submit();

            // Assert
            customHttpMessageHandler.SendAsyncFunc = (request, cancellationToken) =>
            {
                if (request.Method == HttpMethod.Post && request.RequestUri != null && request.RequestUri.ToString().Contains("/api/AssignLicense"))
                {
                    var content = request.Content?.ReadAsStringAsync().Result;
                    content.Should().Contain("john.doe@example.com");
                    content.Should().Contain("M365_F1_COMM");
                    return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
                }

                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
            };
        }
    }
}