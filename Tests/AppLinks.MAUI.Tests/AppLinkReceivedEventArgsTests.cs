using AppLinks.MAUI.Services;
using FluentAssertions;
using Xunit;

namespace AppLinks.MAUI.Tests
{
    public class AppLinkReceivedEventArgsTests
    {
        [Fact]
        public void ShouldThrowArgumentNullExceptionWhenUriIsNull()
        {
            // Act
            Action action = () => new AppLinkReceivedEventArgs(null);

            // Assert
            var ex = action.Should().Throw<ArgumentNullException>().Which;
            ex.ParamName.Should().Be("uri");
        }

        [Fact]
        public void ShouldCreateAppLinkReceivedEventArgs()
        {
            // Arrange
            var uri = new Uri("https://example.com");

            // Act
            var appLinkReceivedEventArgs = new AppLinkReceivedEventArgs(uri);

            // Assert
            appLinkReceivedEventArgs.Uri.Should().Be(uri);
        }
    }
}