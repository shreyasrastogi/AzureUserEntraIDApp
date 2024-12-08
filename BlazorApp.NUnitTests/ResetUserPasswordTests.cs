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
    public class ResetUserPasswordTests : Bunit.TestContext
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
            // Mock the PostAsJsonAsync method to return a successful response
            customHttpMessageHandler.SendAsyncFunc = (request, cancellationToken) =>
            {
                if (request.Method == HttpMethod.Post && request.RequestUri != null && request.RequestUri.ToString().Contains("api/ResetUserPassword"))
                {
                    return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
                }

                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
            };
        }

        [Test]
        public void ResetUserPasswordComponent_ShouldRenderCorrectly()
        {
            // Arrange
            var userPrincipalName = "john.doe@example.com";

            // Act
            var cut = RenderComponent<ResetUserPassword>(parameters => parameters.Add(p => p.UserPrincipalName, userPrincipalName));

            // Assert
            cut.Markup.Should().Contain("Reset User Password");
            cut.Find("#userPrincipalName").GetAttribute("value").Should().Be(userPrincipalName);
        }

        [Test]
        public void ResetUserPasswordComponent_ShouldShowSuccessMessage_OnSuccessfulPasswordReset()
        {
            // Arrange
            var userPrincipalName = "john.doe@example.com";
            var newPassword = "NewSecurePassword123!";

            // Act
            var cut = RenderComponent<ResetUserPassword>(parameters => parameters.Add(p => p.UserPrincipalName, userPrincipalName));

            cut.Find("#newPassword").Change(newPassword);
            cut.Find("form").Submit();

            // Assert
            cut.Markup.Should().Contain("User password reset successfully.");
        }

        [Test]
        public void ResetUserPasswordComponent_ShouldShowErrorMessage_WhenFieldsAreEmpty()
        {
            // Arrange
            var userPrincipalName = "john.doe@example.com";

            // Act
            var cut = RenderComponent<ResetUserPassword>(parameters => parameters.Add(p => p.UserPrincipalName, userPrincipalName));

            cut.Find("form").Submit();

            // Assert
            cut.Markup.Should().Contain("Please enter all required information.");
        }

        [Test]
        public void ResetUserPasswordComponent_ShouldShowErrorMessage_OnFailedPasswordReset()
        {
            // Arrange
            customHttpMessageHandler.SendAsyncFunc = (request, cancellationToken) =>
            {
                if (request.Method == HttpMethod.Post && request.RequestUri != null && request.RequestUri.ToString().Contains("api/ResetUserPassword"))
                {
                    return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError));
                }

                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
            };

            var userPrincipalName = "john.doe@example.com";
            var newPassword = "NewSecurePassword123!";

            // Act
            var cut = RenderComponent<ResetUserPassword>(parameters => parameters.Add(p => p.UserPrincipalName, userPrincipalName));

            cut.Find("#newPassword").Change(newPassword);
            cut.Find("form").Submit();

            // Assert
            cut.Markup.Should().Contain("Error resetting user password.");
        }
    }
}