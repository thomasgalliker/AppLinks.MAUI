using Microsoft.Extensions.Logging;

namespace AppLinks.MAUI.Services
{
    internal class AppLinkHandler : IAppLinkHandler
    {
        private static readonly Lazy<IAppLinkHandler> Implementation = new Lazy<IAppLinkHandler>(CreateAppLinkHandler, LazyThreadSafetyMode.PublicationOnly);

        public static IAppLinkHandler Current
        {
            get => Implementation.Value;
        }

        private static IAppLinkHandler CreateAppLinkHandler()
        {
            var logger = IPlatformApplication.Current.Services.GetRequiredService<ILogger<AppLinkHandler>>();
            var mainThread = IPlatformApplication.Current.Services.GetRequiredService<IMainThread>();
            return new AppLinkHandler(logger, mainThread);
        }

        private readonly ILogger logger;
        private readonly IMainThread mainThread;

        private EventHandler<AppLinkReceivedEventArgs> appLinkReceivedEventHandler;
        private AppLinkReceivedEventArgs queuedAppLinkEvent;

        private AppLinkHandler(
            ILogger<AppLinkHandler> logger,
            IMainThread mainThread)
        {
            this.logger = logger;
            this.mainThread = mainThread;
        }

        public void EnqueueAppLink(Uri uri)
        {
            this.logger.LogDebug($"EnqueueAppLink: externalUri={uri}");
            this.RaiseAppLinkReceivedEvent(uri);
        }

        public event EventHandler<AppLinkReceivedEventArgs> AppLinkReceived
        {
            add
            {
                var previousSubscriptions = this.appLinkReceivedEventHandler;
                this.appLinkReceivedEventHandler += value;

                if (this.queuedAppLinkEvent is AppLinkReceivedEventArgs appLinkEvent &&
                    previousSubscriptions == null)
                {
                    this.mainThread.BeginInvokeOnMainThread(() =>
                    {
                        this.logger.LogDebug(
                            $"AppLinkReceived: Event subscribed and raised for " +
                            $"Uri={appLinkEvent.Uri}");
                        this.appLinkReceivedEventHandler.Invoke(this, appLinkEvent);
                        this.queuedAppLinkEvent = null;
                    });
                }
            }
            remove
            {
                this.appLinkReceivedEventHandler -= value;
            }
        }

        private void RaiseAppLinkReceivedEvent(Uri uri)
        {
            if (this.appLinkReceivedEventHandler == null)
            {
                this.logger.LogDebug($"RaiseAppLinkReceivedEvent: uri={uri} (_appLinkReceivedEventHandler not present)");
                this.queuedAppLinkEvent = new AppLinkReceivedEventArgs(uri);
            }
            else
            {
                this.mainThread.BeginInvokeOnMainThread(() =>
                {
                    this.logger.LogDebug($"RaiseAppLinkReceivedEvent: uri={uri} (_appLinkReceivedEventHandler present)");
                    this.appLinkReceivedEventHandler.Invoke(this, new AppLinkReceivedEventArgs(uri));
                });
            }
        }
    }
}