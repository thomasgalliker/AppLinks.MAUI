using AppLinks.MAUI;

namespace AppLinksDemoApp.Services.AppLinks
{
    /// <summary>
    /// You can implement <see cref="IAppLinkRules"/> to automatically
    /// configure app link rules at startup time.
    /// This solution gives you the ability to inject further dependencies
    /// used for app link validation.
    /// </summary>
    public class CustomAppLinkRules : IAppLinkRules
    {
        public CustomAppLinkRules()
        {
        }

        public readonly UriRule HomeRule = new UriRule(
            "HomeRule",
            uri => uri.Host == "example.com" && uri.AbsolutePath == "/home");

        public readonly UriRule SettingsRule = new UriRule(
            "SettingsRule",
            uri => uri.Host == "example.com" && uri.AbsolutePath == "/settings");

        public IEnumerable<UriRule> Get()
        {
            yield return this.HomeRule;
            yield return this.SettingsRule;
        }
    }
}