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
    public class UriProcessorTests
    {
        private readonly AutoMocker autoMocker;

        public UriProcessorTests(ITestOutputHelper testOutputHelper)
        {
            this.autoMocker = new AutoMocker();

            this.autoMocker.Use<IMainThread>(new StaticMainThread());
            this.autoMocker.Use<ILogger<UriProcessor>>(new TestOutputHelperLogger<UriProcessor>(testOutputHelper));
        }

        [Fact]
        public void Process_ShouldCallRegisteredCallbacks()
        {
            // Arrange
            var uris = new List<(string, Uri)>();

            var uriProcessor = this.autoMocker.CreateInstance<UriProcessor>(enablePrivate: true);

            uriProcessor.Add(new UriRule("HomePageRule", uri => uri.Host == "example.com" && uri.AbsolutePath == "/home"));
            uriProcessor.Add(new UriRule("SettingsPageRule", uri => uri.Host == "example.com" && uri.AbsolutePath == "/settings"));
            uriProcessor.Add(new UriRule("QueryActionRule", uri => uri.Host == "example.com" && uri.Query.Contains("action=view")));

            uriProcessor.RegisterCallback("HomePageRule", u => uris.Add(("HomePageRule", u)));
            uriProcessor.RegisterCallback("SettingsPageRule", u => uris.Add(("SettingsPageRule", u)));
            uriProcessor.RegisterCallback("QueryActionRule", u => uris.Add(("QueryActionRule", u)));

            // Act
            uriProcessor.Process(new Uri("https://example.com/home"));
            uriProcessor.Process(new Uri("https://example.com/settings"));
            uriProcessor.Process(new Uri("https://example.com?action=view"));

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
            var uriProcessor = this.autoMocker.CreateInstance<UriProcessor>(enablePrivate: true);
            uriProcessor.Add(new UriRule("HomePageRule", uri => uri.Host == "example.com" && uri.AbsolutePath == "/home"));
            uriProcessor.Add(new UriRule("SettingsPageRule", uri => uri.Host == "example.com" && uri.AbsolutePath == "/settings"));

            uriProcessor.RegisterCallback("HomePageRule", u => uris.Add(("HomePageRule", u)));

            uriProcessor.Process(new Uri("https://example.com/home"));
            uriProcessor.Process(new Uri("https://example.com/settings"));

            // Act
            uriProcessor.RegisterCallback("SettingsPageRule", u => uris.Add(("SettingsPageRule", u)));

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
            var uriProcessor = this.autoMocker.CreateInstance<UriProcessor>(enablePrivate: true);
            uriProcessor.Add(new UriRule("HomePageRule", uri => uri.Host == "example.com" && uri.AbsolutePath == "/home"));

            uriProcessor.Process(new Uri("https://example.com/home"));
            uriProcessor.Process(new Uri("https://example.com/home"));
            uriProcessor.Process(new Uri("https://example.com/home"));

            // Act
            uriProcessor.RegisterCallback("HomePageRule", u => uris.Add(("HomePageRule", u)));

            // Assert
            uris.Should().HaveCount(1);
            uris.ElementAt(0).Item1.Should().Be("HomePageRule");
        }
    }
}