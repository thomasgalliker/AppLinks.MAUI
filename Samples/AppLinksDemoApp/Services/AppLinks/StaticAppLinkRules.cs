using AppLinks.MAUI;

namespace AppLinksDemoApp
{
    /// <summary>
    /// You can either use static app link rules across your app.
    /// With this solution, you lose the ability to inject further dependencies
    /// used to check if the received app links are valid.
    /// </summary>
    public static class StaticAppLinkRules
    {
        public static readonly UriRule HomeRule = new UriRule(
            "HomeRule",
            uri => uri.Host == "example.com" && uri.AbsolutePath == "/home");

        public static readonly UriRule SettingsRule = new UriRule(
            "SettingsRule",
            uri => uri.Host == "example.com" && uri.AbsolutePath == "/settings");
    }
}