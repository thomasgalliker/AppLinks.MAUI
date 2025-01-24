using System.Collections.Concurrent;
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
    public class AppLinkHandlerTests
    {
        private readonly AutoMocker autoMocker;

        public AppLinkHandlerTests(ITestOutputHelper testOutputHelper)
        {
            this.autoMocker = new AutoMocker();

            this.autoMocker.Use<IMainThread>(new StaticMainThread());
            this.autoMocker.Use<ILogger<AppLinkHandler>>(new TestOutputHelperLogger<AppLinkHandler>(testOutputHelper));
        }

        [Fact]
        public void ShouldEnqueueAppLink_WithoutAppLinkReceivedSubscribers()
        {
            // Arrange
            var appLinkHandler = this.autoMocker.CreateInstance<AppLinkHandler>(enablePrivate: true);
            var uri = new Uri("https://example.com");

            // Act
            Action action = () => appLinkHandler.EnqueueAppLink(uri);

            // Assert
            action.Should().NotThrow();
        }

        [Fact]
        public void ShouldEnqueueAppLink_AppLinkReceivedSubscribedBeforeEnqueue()
        {
            // Arrange
            var appLinkEventArgs = new List<AppLinkReceivedEventArgs>();

            var appLinkHandler = this.autoMocker.CreateInstance<AppLinkHandler>(enablePrivate: true);
            appLinkHandler.AppLinkReceived += (_, args) => { appLinkEventArgs.Add(args); };

            var uri = new Uri("https://example.com");

            // Act
            appLinkHandler.EnqueueAppLink(uri);

            // Assert
            appLinkEventArgs.Should().HaveCount(1);
        }

        [Fact]
        public void ShouldEnqueueAppLink_AppLinkReceivedSubscribedAfterEnqueue()
        {
            // Arrange
            var appLinkEventArgs = new List<AppLinkReceivedEventArgs>();

            var appLinkHandler = this.autoMocker.CreateInstance<AppLinkHandler>(enablePrivate: true);

            appLinkHandler.EnqueueAppLink(new Uri("https://uri1.com"));
            appLinkHandler.EnqueueAppLink(new Uri("https://uri2.com"));
            appLinkHandler.EnqueueAppLink(new Uri("https://uri3.com"));

            // Act
            appLinkHandler.AppLinkReceived += (_, args) => { appLinkEventArgs.Add(args); };

            // Assert
            appLinkEventArgs.Should().HaveCount(3);
            appLinkEventArgs.ElementAt(0).Should().BeEquivalentTo(new AppLinkReceivedEventArgs(new Uri("https://uri1.com")));
            appLinkEventArgs.ElementAt(1).Should().BeEquivalentTo(new AppLinkReceivedEventArgs(new Uri("https://uri2.com")));
            appLinkEventArgs.ElementAt(2).Should().BeEquivalentTo(new AppLinkReceivedEventArgs(new Uri("https://uri3.com")));
        }

        [Fact]
        public void ShouldResetCacheAfterEnqueue()
        {
            // Arrange
            var appLinkEventArgs = new List<AppLinkReceivedEventArgs>();

            var appLinkHandler = this.autoMocker.CreateInstance<AppLinkHandler>(enablePrivate: true);

            var uri = new Uri("https://example.com");
            appLinkHandler.EnqueueAppLink(uri);
            appLinkHandler.ResetCache();

            // Act
            appLinkHandler.AppLinkReceived += (_, args) => { appLinkEventArgs.Add(args); };

            // Assert
            appLinkEventArgs.Should().HaveCount(0);
        }
    }
}