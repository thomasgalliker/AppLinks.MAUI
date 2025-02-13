using AppLinks.MAUI.Extensions;
using AppLinks.MAUI.Services;
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
            var mainThread = IPlatformApplication.Current.Services.GetService<IMainThread>();
            return new AppLinkProcessor(logger, appLinkRules, mainThread);
        }

        private const string DefaultTargetName = "__default__";

        private readonly Dictionary<string, Dictionary<string, Action<Uri>>> ruleCallbacks =
            new Dictionary<string, Dictionary<string, Action<Uri>>>();

        private readonly ILogger<AppLinkProcessor> logger;
        private readonly IMainThread mainThread;
        private readonly object lockObj = new object();
        private readonly List<AppLinkRule> rules = new List<AppLinkRule>();

        private Queue<Uri> pendingUris = new Queue<Uri>();

        internal AppLinkProcessor(
            ILogger<AppLinkProcessor> logger,
            IAppLinkRules appLinkRules,
            IMainThread mainThread)
        {
            ArgumentNullException.ThrowIfNull(logger);

            this.logger = logger;
            this.mainThread = mainThread;

            if (appLinkRules != null)
            {
                foreach (var appLinkRule in appLinkRules.Get())
                {
                    this.Add(appLinkRule);
                }
            }
        }

        public void RegisterCallback(AppLinkRule rule, Action<Uri> action)
        {
            this.RegisterCallback(null, rule, action);
        }

        public void RegisterCallback(object target, AppLinkRule rule, Action<Uri> action)
        {
            ArgumentNullException.ThrowIfNull(rule);
            ArgumentNullException.ThrowIfNull(action);

            this.RegisterCallback(target, rule.RuleId, action);
        }

        public void RegisterCallback(string ruleId, Action<Uri> action)
        {
            this.RegisterCallback(null, ruleId, action);
        }

        public void RegisterCallback(object target, string ruleId, Action<Uri> action)
        {
            ArgumentException.ThrowIfNullOrEmpty(ruleId);
            ArgumentNullException.ThrowIfNull(action);

            var targetName = GetTargetName(target);
            this.logger.LogDebug($"RegisterCallback: target={targetName}, rulesId={ruleId}");

            lock (this.lockObj)
            {
                if (this.rules.All(r => r.RuleId != ruleId))
                {
                    this.logger.LogWarning(
                        $"RegisterCallback: Rule with ID \"{ruleId}\" is not registered. " +
                        $"Call {nameof(IAppLinkRuleManager)}.{nameof(IAppLinkRuleManager.Add)} to register new app link rules.");
                }

                if (!this.ruleCallbacks.TryGetValue(targetName, out var targetRuleCallbacks))
                {
                    targetRuleCallbacks = new Dictionary<string, Action<Uri>>();
                    this.ruleCallbacks.Add(targetName, targetRuleCallbacks);
                }

                targetRuleCallbacks[ruleId] = action;
            }

            this.ProcessPendingUris();
        }

        public bool RemoveCallback(string ruleId)
        {
            return this.RemoveCallback(null, ruleId);
        }

        public bool RemoveCallback(AppLinkRule rule)
        {
            return this.RemoveCallback(null, rule);
        }

        public bool RemoveCallback(object target, AppLinkRule rule)
        {
            ArgumentNullException.ThrowIfNull(rule);

            return this.RemoveCallback(target, rule.RuleId);
        }

        public bool RemoveCallback(object target, string ruleId)
        {
            ArgumentException.ThrowIfNullOrEmpty(ruleId);

            var targetName = GetTargetName(target);
            this.logger.LogDebug($"RemoveCallback: target={targetName}, rulesId={ruleId}");

            var removed = false;

            lock (this.lockObj)
            {
                if (this.ruleCallbacks.TryGetValue(targetName, out var targetRuleCallbacks))
                {
                    removed = targetRuleCallbacks.Remove(ruleId);

                    if (targetRuleCallbacks.Count == 0)
                    {
                        this.ruleCallbacks.Remove(targetName);
                    }
                }
            }

            return removed;
        }

        public void ClearCallbacks(object target)
        {
            var targetName = GetTargetName(target);
            this.logger.LogDebug($"ClearCallbacks: target={targetName}");

            lock (this.lockObj)
            {
                this.ruleCallbacks.Remove(targetName);
            }
        }

        public void ClearCallbacks()
        {
            this.logger.LogDebug("ClearCallbacks");

            lock (this.lockObj)
            {
                this.ruleCallbacks.Clear();
            }
        }

        public void Add(AppLinkRule rule)
        {
            ArgumentNullException.ThrowIfNull(rule);

            lock (this.lockObj)
            {
                this.Remove(rule.RuleId);
                this.rules.Add(rule);
            }

            this.ProcessPendingUris();
        }

        public void Remove(AppLinkRule rule)
        {
            ArgumentNullException.ThrowIfNull(rule);

            this.Remove(rule.RuleId);
        }

        public void Remove(string ruleId)
        {
            lock (this.lockObj)
            {
                var rule = this.rules.FirstOrDefault(r => r.RuleId == ruleId);
                if (rule != null)
                {
                    this.rules.Remove(rule);
                }
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
            var hasAnyActions = false;

            foreach (var rule in this.rules)
            {
                try
                {
                    if (rule.Matches(uri))
                    {
                        foreach (var targetRuleCallbacks in this.ruleCallbacks.Values)
                        {
                            var action = targetRuleCallbacks.GetValueOrDefault(rule.RuleId);
                            if (action != null)
                            {
                                this.mainThread.BeginInvokeOnMainThread(() => action(uri));
                                hasAnyActions = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, $"TryProcessUri failed with exception for uri={uri}: {ex.Message}");
                }
            }

            return hasAnyActions;
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

        private static string GetTargetName(object target)
        {
            return target?.GetType().GetFormattedName() ?? DefaultTargetName;
        }
    }
}