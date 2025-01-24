using Microsoft.Extensions.Logging;

namespace AppLinks.MAUI
{
    internal class UriProcessor : IUriProcessor, IUriProcessorRules
    {
        private static readonly Lazy<IUriProcessor> Implementation =
            new Lazy<IUriProcessor>(CreateUriProcessor, LazyThreadSafetyMode.PublicationOnly);

        public static IUriProcessor Current
        {
            get => Implementation.Value;
        }

        private static IUriProcessor CreateUriProcessor()
        {
            var logger = IPlatformApplication.Current.Services.GetRequiredService<ILogger<UriProcessor>>();
            var appLinkRules = IPlatformApplication.Current.Services.GetService<IAppLinkRules>();
            return new UriProcessor(logger, appLinkRules);
        }

        private readonly Dictionary<string, Action<Uri>> ruleActions = new Dictionary<string, Action<Uri>>();

        private readonly ILogger<UriProcessor> logger;
        private readonly object lockObj = new object();
        private readonly List<UriRule> rules = new List<UriRule>();

        private Queue<Uri> pendingUris = new Queue<Uri>();

        internal UriProcessor(
            ILogger<UriProcessor> logger,
            IAppLinkRules appLinkRules)
        {
            ArgumentNullException.ThrowIfNull(logger);

            this.logger = logger;

            if (appLinkRules != null)
            {
                foreach (var appLinkRule in appLinkRules.Get())
                {
                    this.Add(appLinkRule);
                }
            }
        }

        public void RegisterCallback(string ruleId, Action<Uri> action)
        {
            ArgumentException.ThrowIfNullOrEmpty(ruleId);
            ArgumentNullException.ThrowIfNull(action);

            this.ruleActions[ruleId] = action;

            this.ProcessPendingUris();
        }

        public void RegisterCallback(UriRule rule, Action<Uri> action)
        {
            ArgumentNullException.ThrowIfNull(rule);
            ArgumentNullException.ThrowIfNull(action);

            this.RegisterCallback(rule.RuleId, action);
        }

        public bool RemoveCallback(string ruleId)
        {
            ArgumentException.ThrowIfNullOrEmpty(ruleId);

            return this.ruleActions.Remove(ruleId);
        }

        public bool RemoveCallback(UriRule rule)
        {
            ArgumentNullException.ThrowIfNull(rule);

            return this.RemoveCallback(rule.RuleId);
        }

        public void ClearCallbacks()
        {
            this.ruleActions.Clear();
        }

        public void Add(UriRule rule)
        {
            ArgumentNullException.ThrowIfNull(rule);

            this.Remove(rule.RuleId);
            this.rules.Add(rule);

            this.ProcessPendingUris();
        }

        public void Remove(UriRule rule)
        {
            ArgumentNullException.ThrowIfNull(rule);

            this.Remove(rule.RuleId);
        }

        public void Remove(string ruleId)
        {
            var rule = this.rules.FirstOrDefault(r => r.RuleId == ruleId);
            if (rule != null)
            {
                this.rules.Remove(rule);
            }
        }

        /// <summary>
        /// Processes a Uri immediately if a matching rule exists.
        /// Otherwise, queues the Uri for later processing.
        /// </summary>
        public void Process(Uri uri)
        {
            ArgumentNullException.ThrowIfNull(uri);

            this.logger.LogDebug($"Process: uri={uri}");

            lock (this.lockObj)
            {
                if (this.TryProcessUri(uri))
                {
                    return;
                }

                // If no rule matches, add the Uri to the pending queue
                this.pendingUris.Enqueue(uri);
            }
        }

        private bool TryProcessUri(Uri uri)
        {
            foreach (var rule in this.rules)
            {
                try
                {
                    if (rule.Matches(uri))
                    {
                        var action = this.ruleActions.GetValueOrDefault(rule.RuleId);
                        if (action != null)
                        {
                            action(uri);
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, $"TryProcessUri failed with exception for uri={uri}: {ex.Message}");
                }
            }

            return false;
        }

        void IUriProcessor.Clear()
        {
            lock (this.lockObj)
            {
                this.pendingUris.Clear();
            }
        }

        void IUriProcessorRules.Clear()
        {
            this.rules.Clear();
        }

        private void ProcessPendingUris()
        {
            lock (this.lockObj)
            {
                var processedUris = new List<Uri>();
                foreach (var uri in this.pendingUris)
                {
                    if (this.TryProcessUri(uri))
                    {
                        processedUris.Add(uri);
                        break;
                    }
                }

                // This not only updates the pending URI list
                // it also eliminates duplicate URIs from the queue.
                this.pendingUris = new Queue<Uri>(this.pendingUris.Except(processedUris));
            }
        }
    }
}