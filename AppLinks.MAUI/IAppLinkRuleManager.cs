namespace AppLinks.MAUI
{
    public interface IAppLinkRuleManager
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IAppLinkProcessor"/>.
        /// </summary>
        public static IAppLinkRuleManager Current { get; set; } = (IAppLinkRuleManager)AppLinkProcessor.Current;

        /// <summary>
        /// Adds a new URI processing rule.
        /// </summary>
        /// <param name="rule"></param>
        void Add(AppLinkRule rule);

        /// <summary>
        /// Removes a rule by given <paramref name="rule"/>.
        /// </summary>
        void Remove(AppLinkRule rule);

        /// <summary>
        /// Removes a rule by given <paramref name="ruleId"/>.
        /// </summary>
        void Remove(string ruleId);

        /// <summary>
        /// Removes all registered rules.
        /// </summary>
        void Clear();
    }
}