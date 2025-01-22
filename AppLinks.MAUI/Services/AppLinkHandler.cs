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
        private AppLinkReceivedEventArgs cachedAppLinkReceivedEventArgs;

        internal AppLinkHandler(
            ILogger<AppLinkHandler> logger,
            IMainThread mainThread)
        {
            this.logger = logger;
            this.mainThread = mainThread;
        }

        public void EnqueueAppLink(Uri uri)
        {
            this.logger.LogDebug($"EnqueueAppLink: uri={uri}");
            this.RaiseAppLinkReceivedEvent(uri);
        }

        public event EventHandler<AppLinkReceivedEventArgs> AppLinkReceived
        {
            add
            {
                var previousSubscriptions = this.appLinkReceivedEventHandler;
                this.appLinkReceivedEventHandler += value;

                if (this.cachedAppLinkReceivedEventArgs is AppLinkReceivedEventArgs eventArgs &&
                    previousSubscriptions == null)
                {
                    this.logger.LogDebug($"AppLinkReceived: Cached event raised to new subscribed (uri={eventArgs.Uri})");

                    this.mainThread.BeginInvokeOnMainThread(() =>
                    {
                        this.appLinkReceivedEventHandler.Invoke(this, eventArgs);
                    });

                    this.ResetCache();
                }
            }
            remove
            {
                this.appLinkReceivedEventHandler -= value;
            }
        }

        private void RaiseAppLinkReceivedEvent(Uri uri)
        {
            var appLinkReceivedEventArgs = new AppLinkReceivedEventArgs(uri);

            if (this.appLinkReceivedEventHandler == null)
            {
                this.logger.LogDebug($"RaiseAppLinkReceivedEvent: uri={uri} ({nameof(this.appLinkReceivedEventHandler)} not set)");
                this.cachedAppLinkReceivedEventArgs = appLinkReceivedEventArgs;
            }
            else
            {
                this.logger.LogDebug($"RaiseAppLinkReceivedEvent: uri={uri}");
                this.mainThread.BeginInvokeOnMainThread(() =>
                {
                    this.appLinkReceivedEventHandler.Invoke(this, appLinkReceivedEventArgs);
                });
            }
        }

        public void ResetCache()
        {
            this.logger.LogDebug("ResetCache");
            this.cachedAppLinkReceivedEventArgs = null;
        }
    }
}