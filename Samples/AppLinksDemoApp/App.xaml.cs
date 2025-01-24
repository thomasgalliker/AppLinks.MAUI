using AppLinks.MAUI;
using AppLinksDemoApp.Views;

namespace AppLinksDemoApp
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider, IAppLinkRuleManager appLinkRuleManager)
        {
            this.InitializeComponent();

            // Register uri processing rules at startup of your app.
            // Here we use static declarations of app link rules.
            // appLinkRuleManager.Add(StaticAppLinkRules.HomeRule);
            // appLinkRuleManager.Add(StaticAppLinkRules.SettingsRule);

            var mainPage = serviceProvider.GetRequiredService<MainPage>();
            this.MainPage = new NavigationPage(mainPage);
        }
    }
}
