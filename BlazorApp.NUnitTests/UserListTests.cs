using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AzureUserEntraIDApp.Pages;
using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace BlazorApp.NUnitTests
{
    [TestFixture]
    public class UserListTests : Bunit.TestContext
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
            // Mock the GetFromJsonAsync method to return a list of users
            customHttpMessageHandler.SendAsyncFunc = (request, cancellationToken) =>
            {
                if (request.Method == HttpMethod.Get && request.RequestUri != null && request.RequestUri.ToString().Contains("/api/GetUsers"))
                {
                    return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new StringContent("[{\"givenName\":\"John\",\"surname\":\"Doe\",\"UserPrincipalName\":\"john.doe@example.com\",\"Department\":\"IT\"}]")
                    });
                }

                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
            };
        }

        [Test]
        public void UserListComponent_ShouldRenderCorrectly()
        {
            // Act
            var cut = RenderComponent<UserList>();

            // Assert
            cut.Markup.Should().Contain("User List");
            cut.Markup.Should().Contain("First Name");
            cut.Markup.Should().Contain("Last Name");
            cut.Markup.Should().Contain("User Principal Name");
            cut.Markup.Should().Contain("Department");
        }

        [Test]
        public void UserListComponent_ShouldShowNoUsersMessage()
        {
            // Arrange
            customHttpMessageHandler.SendAsyncFunc = (request, cancellationToken) =>
            {
                if (request.Method == HttpMethod.Get && request.RequestUri != null && request.RequestUri.ToString().Contains("/api/GetUsers"))
                {
                    return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                    {
                        Content = new StringContent("[]")
                    });
                }

                return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
            };

            // Act
            var cut = RenderComponent<UserList>();

            // Assert
            cut.Markup.Should().Contain("No users found.");
        }

        [Test]
        public void UserListComponent_ShouldShowLoadingMessage()
        {
            // Arrange
            customHttpMessageHandler.SendAsyncFunc = (request, cancellationToken) =>
            {
                return Task.Delay(1000).ContinueWith(_ => new HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new StringContent("[{\"givenName\":\"John\",\"surname\":\"Doe\",\"UserPrincipalName\":\"john.doe@example.com\",\"Department\":\"IT\"}]")
                });
            };

            // Act
            var cut = RenderComponent<UserList>();

            // Assert
            cut.Markup.Should().Contain("Loading...");
        }

        [Test]
        public void UserListComponent_ShouldNavigateToEditUser()
        {
            // Act
            var cut = RenderComponent<UserList>(parameters => parameters.Add(p => p.showEditOnly, true));

            cut.Find("button").Click();

            // Assert
            Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/edituser/john.doe@example.com");
        }

        [Test]
        public void UserListComponent_ShouldNavigateToDisableUser()
        {
            // Act
            var cut = RenderComponent<UserList>(parameters => parameters.Add(p => p.showDisableOnly, true));

            cut.Find("button").Click();

            // Assert
            Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/disableuser/john.doe@example.com");
        }

        [Test]
        public void UserListComponent_ShouldNavigateToDeleteUser()
        {
            // Act
            var cut = RenderComponent<UserList>(parameters => parameters.Add(p => p.showDeleteOnly, true));

            cut.Find("button").Click();

            // Assert
            Services.GetRequiredService<FakeNavigationManager>().Uri.Should().EndWith("/deleteuser/john.doe@example.com");
        }
    }
}

