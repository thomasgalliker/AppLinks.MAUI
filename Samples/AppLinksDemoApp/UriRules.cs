using AppLinks.MAUI;

namespace AppLinksDemoApp
{
    public static class UriRules
    {
        public static readonly UriRule HomeRule = new UriRule(
            "HomeRule",
            uri => uri.Host == "example.com" && uri.AbsolutePath == "/home");

        public static readonly UriRule SettingsRule = new UriRule(
            "SettingsRule",
            uri => uri.Host == "example.com" && uri.AbsolutePath == "/settings");
    }
}