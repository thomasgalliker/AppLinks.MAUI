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

        public readonly AppLinkRule HomeRule = new AppLinkRule(
            "HomeRule",
            uri => uri.Host == "example.com" && uri.AbsolutePath == "/home");

        public readonly AppLinkRule SettingsRule = new AppLinkRule(
            "SettingsRule",
            uri => uri.Host == "example.com" && uri.AbsolutePath == "/settings");

        public IEnumerable<AppLinkRule> Get()
        {
            yield return this.HomeRule;
            yield return this.SettingsRule;
        }
    }
}