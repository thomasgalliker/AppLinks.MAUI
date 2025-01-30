using AppLinks.MAUI.Services;
using AppLinks.MAUI.Tests.Logging;
using AppLinks.MAUI.Tests.Utils;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.AutoMock;
using Xunit;
using Xunit.Abstractions;

namespace AppLinks.MAUI.Tests
{
    public class AppLinkProcessorTests
    {
        private readonly AutoMocker autoMocker;

        public AppLinkProcessorTests(ITestOutputHelper testOutputHelper)
        {
            this.autoMocker = new AutoMocker();

            this.autoMocker.Use<IMainThread>(new StaticMainThread());
            this.autoMocker.Use<ILogger<AppLinkProcessor>>(new TestOutputHelperLogger<AppLinkProcessor>(testOutputHelper));
        }

        [Fact]
        public void Process_ShouldCallRegisteredCallbacks()
        {
            // Arrange
            var uris = new List<(string, Uri)>();

            var processor = this.autoMocker.CreateInstance<AppLinkProcessor>(enablePrivate: true);

            processor.Add(new AppLinkRule("HomePageRule", uri => uri.Host == "example.com" && uri.AbsolutePath == "/home"));
            processor.Add(new AppLinkRule("SettingsPageRule", uri => uri.Host == "example.com" && uri.AbsolutePath == "/settings"));
            processor.Add(new AppLinkRule("QueryActionRule", uri => uri.Host == "example.com" && uri.Query.Contains("action=view")));

            processor.RegisterCallback("HomePageRule", u => uris.Add(("HomePageRule", u)));
            processor.RegisterCallback("SettingsPageRule", u => uris.Add(("SettingsPageRule", u)));
            processor.RegisterCallback("QueryActionRule", u => uris.Add(("QueryActionRule", u)));

            // Act
            processor.Process(new Uri("https://example.com/home"));
            processor.Process(new Uri("https://example.com/settings"));
            processor.Process(new Uri("https://example.com?action=view"));

            // Assert
            uris.Should().HaveCount(3);
            uris.ElementAt(0).Item1.Should().Be("HomePageRule");
            uris.ElementAt(1).Item1.Should().Be("SettingsPageRule");
            uris.ElementAt(2).Item1.Should().Be("QueryActionRule");
        }

        [Fact]
        public void RegisterCallback_ProcessesQueuedUris()
        {
            // Arrange
            var uris = new List<(string, Uri)>();
            var processor = this.autoMocker.CreateInstance<AppLinkProcessor>(enablePrivate: true);
            processor.Add(new AppLinkRule("HomePageRule", uri => uri.Host == "example.com" && uri.AbsolutePath == "/home"));
            processor.Add(new AppLinkRule("SettingsPageRule", uri => uri.Host == "example.com" && uri.AbsolutePath == "/settings"));

            processor.RegisterCallback("HomePageRule", u => uris.Add(("HomePageRule", u)));

            processor.Process(new Uri("https://example.com/home"));
            processor.Process(new Uri("https://example.com/settings"));

            // Act
            processor.RegisterCallback("SettingsPageRule", u => uris.Add(("SettingsPageRule", u)));

            // Assert
            uris.Should().HaveCount(2);
            uris.ElementAt(0).Item1.Should().Be("HomePageRule");
            uris.ElementAt(1).Item1.Should().Be("SettingsPageRule");
        }

        [Fact]
        public void RegisterCallback_ProcessesQueuedUris_DropsDuplicateUris()
        {
            // Arrange
            var uris = new List<(string, Uri)>();
            var processor = this.autoMocker.CreateInstance<AppLinkProcessor>(enablePrivate: true);
            processor.Add(new AppLinkRule("HomePageRule", uri => uri.Host == "example.com" && uri.AbsolutePath == "/home"));

            processor.Process(new Uri("https://example.com/home"));
            processor.Process(new Uri("https://example.com/home"));
            processor.Process(new Uri("https://example.com/home"));

            // Act
            processor.RegisterCallback("HomePageRule", u => uris.Add(("HomePageRule", u)));

            // Assert
            uris.Should().HaveCount(1);
            uris.ElementAt(0).Item1.Should().Be("HomePageRule");
        }

        [Fact]
        public void RegisterCallback_ShouldLogWarning_IfNoRuleIsRegistered()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<AppLinkProcessor>>();
            this.autoMocker.Use(loggerMock.Object);
            var processor = this.autoMocker.CreateInstance<AppLinkProcessor>(enablePrivate: true);

            // Act
            processor.RegisterCallback("HomePageRule", _ => {});

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    It.Is<LogLevel>(logLevel => logLevel == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void AddRule_ShouldReplaceExistingRule()
        {
            // Arrange
            var appRuleCalls = new List<string>();
            var processor = this.autoMocker.CreateInstance<AppLinkProcessor>(enablePrivate: true);
            processor.RegisterCallback("HomePageRule", uri => {});

            var appLinkRuleHome1 = new AppLinkRule("HomePageRule", uri => { appRuleCalls.Add("appLinkRuleHome1"); return true; });
            var appLinkRuleHome2 = new AppLinkRule("HomePageRule", uri => { appRuleCalls.Add("appLinkRuleHome2"); return true; });

            processor.Add(appLinkRuleHome1);

            // Act
            processor.Add(appLinkRuleHome2);

            processor.Process(new Uri("https://example.com/home1"));
            processor.Process(new Uri("https://example.com/home2"));

            // Assert
            appRuleCalls.Should().HaveCount(2);
            appRuleCalls.ElementAt(0).Should().Be("appLinkRuleHome2");
            appRuleCalls.ElementAt(1).Should().Be("appLinkRuleHome2");
        }

        [Fact]
        public void RemoveCallback_ShouldRemoveCallback_ByRuleId()
        {
            // Arrange
            var uris = new List<(string, Uri)>();
            var processor = this.autoMocker.CreateInstance<AppLinkProcessor>(enablePrivate: true);
            processor.Add(new AppLinkRule("HomePageRule", uri => uri.Host == "example.com" && uri.AbsolutePath == "/home"));
            processor.RegisterCallback("HomePageRule", u => uris.Add(("HomePageRule", u)));

            // Act
            var removed = processor.RemoveCallback("HomePageRule");
            processor.Process(new Uri("https://example.com/home"));

            // Assert
            removed.Should().BeTrue();
            uris.Should().BeEmpty();
        }

        [Fact]
        public void RemoveCallback_ShouldRemoveCallback_ByRule()
        {
            // Arrange
            var uris = new List<(string, Uri)>();
            var processor = this.autoMocker.CreateInstance<AppLinkProcessor>(enablePrivate: true);
            var appLinkRule = new AppLinkRule("HomePageRule", uri => uri.Host == "example.com" && uri.AbsolutePath == "/home");
            processor.Add(appLinkRule);
            processor.RegisterCallback(appLinkRule, u => uris.Add(("HomePageRule", u)));

            // Act
            var removed = processor.RemoveCallback(appLinkRule);
            processor.Process(new Uri("https://example.com/home"));

            // Assert
            removed.Should().BeTrue();
            uris.Should().BeEmpty();
        }

        [Fact]
        public void RemoveRule_ShouldNoLongerProcessUris_ByAppLinkRule()
        {
            // Arrange
            var uris = new List<(string, Uri)>();
            var processor = this.autoMocker.CreateInstance<AppLinkProcessor>(enablePrivate: true);
            var appLinkRule = new AppLinkRule("HomePageRule", uri => uri.Host == "example.com" && uri.AbsolutePath == "/home");
            processor.Add(appLinkRule);
            processor.RegisterCallback(appLinkRule, u => uris.Add(("HomePageRule", u)));

            // Act
            processor.Remove(appLinkRule);
            processor.Process(new Uri("https://example.com/home"));

            // Assert
            uris.Should().BeEmpty();
        }

        [Fact]
        public void RemoveRule_ShouldNoLongerProcessUris_ByAppLinkRuleId()
        {
            // Arrange
            var uris = new List<(string, Uri)>();
            var processor = this.autoMocker.CreateInstance<AppLinkProcessor>(enablePrivate: true);
            var appLinkRule = new AppLinkRule("HomePageRule", uri => uri.Host == "example.com" && uri.AbsolutePath == "/home");
            processor.Add(appLinkRule);
            processor.RegisterCallback(appLinkRule, u => uris.Add(("HomePageRule", u)));

            // Act
            processor.Remove("HomePageRule");
            processor.Process(new Uri("https://example.com/home"));

            // Assert
            uris.Should().BeEmpty();
        }

        [Fact]
        public void ClearCache_ShouldRemoveAllPendingUris()
        {
            // Arrange
            var uris = new List<(string, Uri)>();
            var processor = this.autoMocker.CreateInstance<AppLinkProcessor>(enablePrivate: true);
            processor.Add(new AppLinkRule("HomePageRule", uri => uri.Host == "example.com" && uri.AbsolutePath == "/home"));

            processor.Process(new Uri("https://example.com/home"));

            // Act
            processor.ClearCache();
            processor.RegisterCallback("HomePageRule", u => uris.Add(("HomePageRule", u)));

            // Assert
            uris.Should().BeEmpty();
        }
    }
}