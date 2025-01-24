namespace AppLinks.MAUI
{
    public interface IUriProcessorRules
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IUriProcessor"/>.
        /// </summary>
        public static IUriProcessorRules Current { get; set; } = (IUriProcessorRules)UriProcessor.Current;

        /// <summary>
        /// Adds a new URI processing rule.
        /// </summary>
        /// <param name="rule"></param>
        void Add(UriRule rule);

        /// <summary>
        /// Removes a rule by given <paramref name="rule"/>.
        /// </summary>
        void Remove(UriRule rule);

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