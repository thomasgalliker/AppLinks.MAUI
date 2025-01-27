using System.Diagnostics;

namespace AppLinks.MAUI
{
    [DebuggerDisplay("{RuleId}")]
    public class AppLinkRule : IEquatable<AppLinkRule>
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
        ///     A unique identifier for the rule (useful for debugging or dynamic association).
        /// </summary>
        public string RuleId { get; }

        /// <summary>
        ///     Determines if the rule matches the given Uri.
        /// </summary>
        public bool Matches(Uri uri)
        {
            return this.matchCondition(uri);
        }

        public bool Equals(AppLinkRule other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(this.RuleId, other.RuleId, StringComparison.InvariantCultureIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((AppLinkRule)obj);
        }

        public override int GetHashCode()
        {
            return this.RuleId != null ? StringComparer.InvariantCultureIgnoreCase.GetHashCode(this.RuleId) : 0;
        }

        public static bool operator ==(AppLinkRule left, AppLinkRule right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AppLinkRule left, AppLinkRule right)
        {
            return !Equals(left, right);
        }
    }
}