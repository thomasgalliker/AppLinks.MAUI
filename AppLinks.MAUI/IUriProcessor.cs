namespace AppLinks.MAUI
{
    public interface IUriProcessor
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IUriProcessor"/>.
        /// </summary>
        public static IUriProcessor Current { get; set; } = UriProcessor.Current;

        /// <summary>
        /// Registers a callback <paramref name="action"/> for a specific rule.
        /// </summary>
        void RegisterCallback(string ruleId, Action<Uri> action);

        /// <summary>
        /// Registers a callback <paramref name="action"/> for a specific rule.
        /// </summary>
        void RegisterCallback(UriRule rule, Action<Uri> action);

        /// <summary>
        /// Removes all callbacks for a specific rule.
        /// </summary>
        bool RemoveCallback(string ruleId);

        /// <summary>
        /// Removes all callbacks for a specific rule.
        /// </summary>
        bool RemoveCallback(UriRule rule);

        /// <summary>
        /// Removes all callbacks for all rules.
        /// </summary>
        void ClearCallbacks();

        /// <summary>
        /// Processes the given <paramref name="uri"/> with all registered rules.
        /// </summary>
        /// <param name="uri"></param>
        internal void Process(Uri uri);

        /// <summary>
        /// Removes all pending URIs without processing them.
        /// </summary>
        void Clear();
    }
}