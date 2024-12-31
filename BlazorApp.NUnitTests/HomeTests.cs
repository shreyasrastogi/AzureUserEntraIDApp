using Bunit;
using FluentAssertions;
using NUnit.Framework;
using AzureUserEntraIDApp.Pages;

namespace BlazorApp.NUnitTests
{
    [TestFixture]
    public class HomeTests : Bunit.TestContext
    {
        [Test]
        public void HomeComponent_ShouldRenderCorrectly()
        {
            // Act
            var cut = RenderComponent<Home>();

            // Assert
            cut.Markup.Should().Contain("Hello, Users!");
            cut.Markup.Should().Contain("Welcome to Technical Parrot Corp Entra ID Accounts & Access Site");
            cut.Markup.Should().Contain("Actions you can perform on this website:");
            cut.Markup.Should().Contain("Display All Users");
            cut.Markup.Should().Contain("Create User");
            cut.Markup.Should().Contain("Modify User");
            cut.Markup.Should().Contain("Disable User");
            cut.Markup.Should().Contain("Delete User");
            cut.Markup.Should().Contain("Perform Any Action");
            cut.Markup.Should().Contain("Please submit feedback and any new feature that you would like to see on site, we will reach out to you soon!!!");
            cut.Markup.Should().Contain("Welcome to your new app.");
        }
    }
}


