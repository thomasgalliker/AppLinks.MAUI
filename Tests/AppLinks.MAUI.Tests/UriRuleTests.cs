using FluentAssertions;
using Xunit;

namespace AppLinks.MAUI.Tests
{
    public class UriRuleTests
    {
        [Fact]
        public void ShouldMatch_IfConditionReturnsTrue()
        {
            // Arrange
            var testUri = new Uri("https://example.com/test/uri");
            var uriRule = new UriRule("Rule1", uri => uri.Host == "example.com");

            // Act
            var matches = uriRule.Matches(testUri);

            // Assert
            matches.Should().BeTrue();
        }

        [Fact]
        public void ShouldNotMatch_IfConditionReturnsFalse()
        {
            // Arrange
            var testUri = new Uri("https://localhost");
            var uriRule = new UriRule("Rule1", uri => uri.Host == "example.com");

            // Act
            var matches = uriRule.Matches(testUri);

            // Assert
            matches.Should().BeFalse();
        }
    }
}