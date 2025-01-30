using FluentAssertions;
using Xunit;

namespace AppLinks.MAUI.Tests
{
    public class AppLinkRuleTests
    {
        [Fact]
        public void ShouldMatch_IfConditionReturnsTrue()
        {
            // Arrange
            var testUri = new Uri("https://example.com/test/uri");
            var appLinkRule = new AppLinkRule("AppLinkRule1", uri => uri.Host == "example.com");

            // Act
            var matches = appLinkRule.Matches(testUri);

            // Assert
            matches.Should().BeTrue();
        }

        [Fact]
        public void ShouldNotMatch_IfConditionReturnsFalse()
        {
            // Arrange
            var testUri = new Uri("https://localhost");
            var appLinkRule = new AppLinkRule("AppLinkRule1", uri => uri.Host == "example.com");

            // Act
            var matches = appLinkRule.Matches(testUri);

            // Assert
            matches.Should().BeFalse();
        }

        [Fact]
        public void ShouldBeEqual_False()
        {
            // Arrange
            var appLinkRule1 = new AppLinkRule("AppLinkRule1", uri => true);
            var appLinkRule2 = new AppLinkRule("AppLinkRule2", uri => false);

            // Act
            var isUnequal = appLinkRule1 != appLinkRule2;

            // Assert
            isUnequal.Should().BeTrue();
        }

        [Fact]
        public void ShouldBeEqual_True()
        {
            // Arrange
            var appLinkRule1 = new AppLinkRule("AppLinkRule", uri => true);
            var appLinkRule2 = new AppLinkRule("applinkrule", uri => false);

            // Act
            var isEqual = appLinkRule1 == appLinkRule2;

            // Assert
            isEqual.Should().BeTrue();
        }
    }
}