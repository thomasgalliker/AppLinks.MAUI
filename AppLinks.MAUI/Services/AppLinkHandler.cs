using System.Runtime.CompilerServices;
using AppLinks.MAUI.Extensions;
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
            var appLinkOptions = IPlatformApplication.Current.Services.GetRequiredService<AppLinkOptions>();
            var mainThread = IPlatformApplication.Current.Services.GetRequiredService<IMainThread>();
            var uriProcessor = IPlatformApplication.Current.Services.GetRequiredService<IUriProcessor>();
            return new AppLinkHandler(logger, appLinkOptions, mainThread, uriProcessor);
        }

        private readonly ILogger logger;
        private readonly AppLinkOptions options;
        private readonly IMainThread mainThread;
        private readonly IUriProcessor uriProcessor;
        private readonly Queue<AppLinkReceivedEventArgs> appLinkReceivedQueue = new Queue<AppLinkReceivedEventArgs>();

        private EventHandler<AppLinkReceivedEventArgs> appLinkReceivedEventHandler;

        internal AppLinkHandler(
            ILogger<AppLinkHandler> logger,
            AppLinkOptions options,
            IMainThread mainThread,
            IUriProcessor uriProcessor)
        {
            this.logger = logger;
            this.options = options;
            this.mainThread = mainThread;
            this.uriProcessor = uriProcessor;
        }

        public void EnqueueAppLink(Uri uri)
        {
            this.logger.LogDebug($"EnqueueAppLink: uri={uri}");

            if (this.options.EnableUriProcessor)
            {
                this.uriProcessor.Process(uri);
            }

            this.RaiseOrQueueEvent(
                this.appLinkReceivedEventHandler,
                () => new AppLinkReceivedEventArgs(uri),
                this.appLinkReceivedQueue,
                nameof(this.AppLinkReceived));
        }

        public event EventHandler<AppLinkReceivedEventArgs> AppLinkReceived
        {
            add
            {
                this.mainThread.BeginInvokeOnMainThread(() =>
                {
                    this.DequeueAndSubscribe(
                        value,
                        ref this.appLinkReceivedEventHandler,
                        this.appLinkReceivedQueue);
                });
            }
            remove => this.appLinkReceivedEventHandler -= value;
        }

        private void RaiseOrQueueEvent<TEventArgs>(
            EventHandler<TEventArgs> eventHandler,
            Func<TEventArgs> getEventArgs,
            Queue<TEventArgs> queue,
            string eventName,
            [CallerMemberName] string callerName = null) where TEventArgs : EventArgs
        {
            if (eventHandler != null && eventHandler.GetInvocationList().Length is var subscribersCount and > 0)
            {
                // If subscribers are present, invoke the event handler
                this.logger.LogDebug(
                    $"{callerName ?? nameof(this.RaiseOrQueueEvent)} raises event \"{eventName}\" " +
                    $"to {subscribersCount} subscriber{(subscribersCount != 1 ? "s" : "")}");

                var args = getEventArgs();
                eventHandler.Invoke(this, args);
            }
            else
            {
                if (queue != null)
                {
                    // If no subscribers are present, queue the event args
                    this.logger.LogDebug(
                        $"{callerName ?? nameof(this.RaiseOrQueueEvent)} queues event \"{eventName}\" " +
                        $"into {queue.GetType().GetFormattedName()} for deferred delivery");

                    var args = getEventArgs();
                    queue.Enqueue(args);
                }
                else
                {
                    // If no subscribers are present and no queue is present, we just drop the event...
                    this.logger.LogWarning(
                        $"{callerName ?? nameof(this.RaiseOrQueueEvent)} drops event \"{eventName}\" " +
                        $"(no event subscribers / no queue present).");
                }
            }
        }

        private void DequeueAndSubscribe<TEventArgs>(
            EventHandler<TEventArgs> value,
            ref EventHandler<TEventArgs> eventHandler,
            Queue<TEventArgs> queue,
            [CallerMemberName] string eventName = null) where TEventArgs : EventArgs
        {
            if (queue != null)
            {
                var previousSubscriptions = eventHandler;
                eventHandler += value;

                if (previousSubscriptions == null && eventHandler != null)
                {
                    this.logger.LogDebug(
                        $"{nameof(this.DequeueAndSubscribe)} dequeues {queue.Count} event{(queue.Count == 1 ? "" : "s")} \"{eventName}\" " +
                        $"from {queue.GetType().GetFormattedName()} for deferred delivery");

                    foreach (var args in queue.TryDequeueAll())
                    {
                        eventHandler.Invoke(this, args);
                    }
                }
            }
            else
            {
                eventHandler += value;
            }
        }


        public void ResetCache()
        {
            this.logger.LogDebug("ResetCache");

            this.appLinkReceivedQueue.Clear();
        }
    }
}