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
    public class FeedbackTests : Bunit.TestContext
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
                if (request.Method == HttpMethod.Post && request.RequestUri != null && request.RequestUri.ToString().Contains("api/UserFeedback"))
                {
                    var responseContent = new StringContent("{\"Text\":\"Great service!\",\"Email\":\"john.doe@example.com\",\"PhoneNumber\":\"1234567890\",\"Sentiment\":\"positive\"}");
                    return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = responseContent });
                }

                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
            };
        }

        [Test]
        public void FeedbackComponent_ShouldRenderCorrectly()
        {
            // Act
            var cut = RenderComponent<Feedback>();

            // Assert
            cut.Markup.Should().Contain("Feedback");
            cut.Markup.Should().Contain("Feedback");
            cut.Markup.Should().Contain("Email");
            cut.Markup.Should().Contain("Phone Number");
        }

        [Test]
        public void FeedbackComponent_ShouldSubmitFeedback()
        {
            // Act
            var cut = RenderComponent<Feedback>();

            cut.Find("#feedbackText").Change("Great service!");
            cut.Find("#email").Change("john.doe@example.com");
            cut.Find("#phoneNumber").Change("1234567890");

            cut.Find("form").Submit();

            // Assert
            customHttpMessageHandler.SendAsyncFunc = (request, cancellationToken) =>
            {
                if (request.Method == HttpMethod.Post && request.RequestUri != null && request.RequestUri.ToString().Contains("api/UserFeedback"))
                {
                    var content = request.Content?.ReadAsStringAsync().Result;
                    content.Should().Contain("Great service!");
                    content.Should().Contain("john.doe@example.com");
                    content.Should().Contain("1234567890");
                    return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
                }

                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
            };

            cut.Markup.Should().Contain("Thank you for the positive feedback.");
        }

        [Test]
        public void FeedbackComponent_ShouldShowErrorMessage_WhenFieldsAreEmpty()
        {
            // Act
            var cut = RenderComponent<Feedback>();

            cut.Find("form").Submit();

            // Assert
            cut.Markup.Should().Contain("Please enter all required information.");
        }
    }
}


