using AppLinks.MAUI.Services;
using AppLinks.MAUI.Tests.Logging;
using AppLinks.MAUI.Tests.Utils;
using FluentAssertions;
using Microsoft.Extensions.Logging;
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
    }
}