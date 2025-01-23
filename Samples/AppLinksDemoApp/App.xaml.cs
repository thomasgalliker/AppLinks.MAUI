using AppLinks.MAUI;
using AppLinksDemoApp.Views;

namespace AppLinksDemoApp
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider, IUriProcessorRules uriProcessorRules)
        {
            this.InitializeComponent();

            // Register uri processing rules at startup of your app:
            uriProcessorRules.Add(UriRules.HomeRule);
            uriProcessorRules.Add(UriRules.SettingsRule);

            var mainPage = serviceProvider.GetRequiredService<MainPage>();
            this.MainPage = new NavigationPage(mainPage);
        }
    }
}
