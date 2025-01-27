using Microsoft.Extensions.Logging;

namespace AppLinks.MAUI
{
    internal class AppLinkProcessor : IAppLinkProcessor, IAppLinkRuleManager
    {
        private static readonly Lazy<IAppLinkProcessor> Implementation =
            new Lazy<IAppLinkProcessor>(CreateAppLinkProcessor, LazyThreadSafetyMode.PublicationOnly);

        public static IAppLinkProcessor Current
        {
            get => Implementation.Value;
        }

        private static IAppLinkProcessor CreateAppLinkProcessor()
        {
            var logger = IPlatformApplication.Current.Services.GetRequiredService<ILogger<AppLinkProcessor>>();
            var appLinkRules = IPlatformApplication.Current.Services.GetService<IAppLinkRules>();
            return new AppLinkProcessor(logger, appLinkRules);
        }

        private readonly Dictionary<string, Action<Uri>> ruleCallbacks = new Dictionary<string, Action<Uri>>();

        private readonly ILogger<AppLinkProcessor> logger;
        private readonly object lockObj = new object();
        private readonly List<AppLinkRule> rules = new List<AppLinkRule>();

        private Queue<Uri> pendingUris = new Queue<Uri>();

        internal AppLinkProcessor(
            ILogger<AppLinkProcessor> logger,
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

            if (this.rules.All(r => r.RuleId != ruleId))
            {
                this.logger.LogWarning(
                    $"RegisterCallback: Rule with ID \"{ruleId}\" is not registered. " +
                    $"Call {nameof(IAppLinkRuleManager)}.{nameof(IAppLinkRuleManager.Add)} to register new app link rules.");
            }

            this.ruleCallbacks[ruleId] = action;

            this.ProcessPendingUris();
        }

        public void RegisterCallback(AppLinkRule rule, Action<Uri> action)
        {
            ArgumentNullException.ThrowIfNull(rule);
            ArgumentNullException.ThrowIfNull(action);

            this.RegisterCallback(rule.RuleId, action);
        }

        public bool RemoveCallback(string ruleId)
        {
            ArgumentException.ThrowIfNullOrEmpty(ruleId);

            return this.ruleCallbacks.Remove(ruleId);
        }

        public bool RemoveCallback(AppLinkRule rule)
        {
            ArgumentNullException.ThrowIfNull(rule);

            return this.RemoveCallback(rule.RuleId);
        }

        public void ClearCallbacks()
        {
            this.ruleCallbacks.Clear();
        }

        public void Add(AppLinkRule rule)
        {
            ArgumentNullException.ThrowIfNull(rule);

            this.Remove(rule.RuleId);
            this.rules.Add(rule);

            this.ProcessPendingUris();
        }

        public void Remove(AppLinkRule rule)
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
                        var action = this.ruleCallbacks.GetValueOrDefault(rule.RuleId);
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

        public void ClearCache()
        {
            lock (this.lockObj)
            {
                this.pendingUris.Clear();
            }
        }

        void IAppLinkRuleManager.Clear()
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