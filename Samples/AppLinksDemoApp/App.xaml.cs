using AppLinks.MAUI;
using AppLinksDemoApp.Views;

namespace AppLinksDemoApp
{
    public partial class App : Application
    {
        private readonly IServiceProvider serviceProvider;

        public App(IServiceProvider serviceProvider, IAppLinkRuleManager appLinkRuleManager)
        {
            this.serviceProvider = serviceProvider;
            this.InitializeComponent();

            // Register uri processing rules at startup of your app.
            // Here we use static declarations of app link rules.
            // appLinkRuleManager.Add(StaticAppLinkRules.HomeRule);
            // appLinkRuleManager.Add(StaticAppLinkRules.SettingsRule);
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var mainPage = this.serviceProvider.GetRequiredService<MainPage>();
            return new Window(new NavigationPage(mainPage));
        }
    }
}
