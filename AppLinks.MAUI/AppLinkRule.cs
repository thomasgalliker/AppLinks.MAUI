using System.Diagnostics;

namespace AppLinks.MAUI
{
    [DebuggerDisplay("{RuleId}")]
    public class AppLinkRule
    {
        private readonly Func<Uri, bool> matchCondition;

        public AppLinkRule(string ruleId, Func<Uri, bool> matchCondition)
        {
            ArgumentException.ThrowIfNullOrEmpty(ruleId);
            ArgumentNullException.ThrowIfNull(matchCondition);

            this.RuleId = ruleId;
            this.matchCondition = matchCondition;
        }

        /// <summary>
        /// A unique identifier for the rule (useful for debugging or dynamic association).
        /// </summary>
        public string RuleId { get; }

        /// <summary>
        /// Determines if the rule matches the given Uri.
        /// </summary>
        public bool Matches(Uri uri)
        {
            return this.matchCondition(uri);
        }
    }
}