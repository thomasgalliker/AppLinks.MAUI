namespace AppLinks.MAUI
{
    public interface IAppLinkProcessor
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IAppLinkProcessor"/>.
        /// </summary>
        public static IAppLinkProcessor Current { get; set; } = AppLinkProcessor.Current;

        /// <summary>
        /// Registers a callback <paramref name="action"/> for a specific rule.
        /// This method is called by the consumer that expects
        /// app link data from a certain <paramref name="ruleId"/>.
        /// This could be a view model or just another service.
        /// </summary>
        /// <remarks>
        /// Don't forget to call <see cref="RemoveCallback(string)"/> or <see cref="RemoveCallback(AppLinkRule)"/>
        /// when the consumer is no longer interested in the app link data for the given rule.
        /// </remarks>
        void RegisterCallback(string ruleId, Action<Uri> action);

        /// <summary>
        /// <inheritdoc cref="RegisterCallback(string,System.Action{System.Uri})"/>
        /// </summary>
        /// <remarks>
        /// Don't forget to call <see cref="RemoveCallback(object, string)"/> or <see cref="RemoveCallback(object, AppLinkRule)"/>
        /// when the consumer is no longer interested in the app link data for the given rule.
        /// </remarks>
        void RegisterCallback(object target, string ruleId, Action<Uri> action);

        /// <summary>
        /// Registers a callback <paramref name="action"/> for a specific rule.
        /// This method is called by the consumer that expects
        /// app link data from a certain <paramref name="rule"/>.
        /// This could be a view model or just another service.
        /// </summary>
        /// <remarks>
        /// Don't forget to call <see cref="RemoveCallback(object, string)"/> or <see cref="RemoveCallback(object, AppLinkRule)"/>
        /// when the consumer is no longer interested in the app link data for the given rule.
        /// </remarks>
        void RegisterCallback(AppLinkRule rule, Action<Uri> action);

        /// <summary>
        /// <inheritdoc cref="RegisterCallback(AppLinkRule,System.Action{System.Uri})"/>
        /// </summary>
        /// <remarks>
        /// Don't forget to call <see cref="RemoveCallback(object, string)"/> or <see cref="RemoveCallback(object, AppLinkRule)"/>
        /// when the consumer is no longer interested in the app link data for the given rule.
        /// </remarks>
        void RegisterCallback(object target, AppLinkRule rule, Action<Uri> action);

        /// <summary>
        /// Removes all callbacks for a specific rule.
        /// </summary>
        bool RemoveCallback(string ruleId);

        /// <summary>
        /// <inheritdoc cref="RemoveCallback(string)"/>
        /// </summary>
        bool RemoveCallback(object target, string ruleId);

        /// <summary>
        /// Removes all callbacks for a specific rule.
        /// </summary>
        bool RemoveCallback(AppLinkRule rule);

        /// <summary>
        /// <inheritdoc cref="RemoveCallback(AppLinkRule)"/>
        /// </summary>
        bool RemoveCallback(object target, AppLinkRule rule);

        /// <summary>
        /// Removes all callbacks for all rules.
        /// </summary>
        void ClearCallbacks();

        /// <summary>
        /// Removes all callbacks for all rules of a given <paramref name="target"/>.
        /// </summary>
        void ClearCallbacks(object target);

        /// <summary>
        /// Processes the given <paramref name="uri"/> with all registered rules.
        /// </summary>
        internal void Process(Uri uri);

        /// <summary>
        /// Removes all pending URIs without processing them.
        /// </summary>
        void ClearCache();
    }
}