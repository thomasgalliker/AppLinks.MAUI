using System.Windows.Input;
using AppLinksDemoApp.Services.Navigation;
using AppLinks.MAUI;
using AppLinks.MAUI.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace AppLinksDemoApp.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly ILogger logger;
        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;
        private readonly IAppLinkHandler appLinkHandler;
        private readonly IUriProcessor uriProcessor;
        private readonly IUriProcessorRules uriProcessorRules;
        private readonly ILauncher launcher;

        private IAsyncRelayCommand appearingCommand;
        private bool isInitialized;
        private string appLinkUri;
        private IAsyncRelayCommand subscribeToAppLinkReceivedEventCommand;
        private IAsyncRelayCommand unsubscribeFromAppLinkReceivedEventCommand;
        private IAsyncRelayCommand addHomeRuleCommand;
        private IAsyncRelayCommand clearRulesCommand;
        private IAsyncRelayCommand processUriCommand;
        private IAsyncRelayCommand<string> navigateToPageCommand;
        private IAsyncRelayCommand<string> navigateToModalPageCommand;
        private IAsyncRelayCommand<string> openUrlCommand;
        private string testUri;

        public MainViewModel(
            ILogger<MainViewModel> logger,
            INavigationService navigationService,
            IDialogService dialogService,
            IAppLinkHandler appLinkHandler,
            IUriProcessor uriProcessor,
            IUriProcessorRules uriProcessorRules,
            ILauncher launcher)
        {
            this.logger = logger;
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            this.appLinkHandler = appLinkHandler;
            this.uriProcessor = uriProcessor;
            this.uriProcessorRules = uriProcessorRules;
            this.launcher = launcher;
        }

        public IAsyncRelayCommand AppearingCommand
        {
            get => this.appearingCommand ??= new AsyncRelayCommand(this.OnAppearingAsync);
        }

        private async Task OnAppearingAsync()
        {
            if (!this.isInitialized)
            {
                await this.InitializeAsync();
                this.isInitialized = true;
            }
        }

        private async Task InitializeAsync()
        {
            try
            {
                this.TestUri = "https://example.com/home";

                this.uriProcessor.RegisterCallback(
                    UriRules.HomeRule,
                    uri => this.dialogService.DisplayAlertAsync(
                        UriRules.HomeRule.RuleId,
                        $"Callback for rule \"{UriRules.HomeRule.RuleId}\"{Environment.NewLine}{Environment.NewLine}" +
                        $"URI {uri}",
                        "OK"));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "InitializeAsync failed with exception");
                await this.dialogService.DisplayAlertAsync("Error", "Initialization failed", "OK");
            }
        }

        public ICommand SubscribeToAppLinkReceivedEventCommand
        {
            get => this.subscribeToAppLinkReceivedEventCommand ??= new AsyncRelayCommand(this.SubscribeToAppLinkReceivedEventAsync);
        }

        private async Task SubscribeToAppLinkReceivedEventAsync()
        {
            try
            {
                this.appLinkHandler.AppLinkReceived += this.OnAppLinkReceived;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "SubscribeToAppLinkReceivedEventAsync failed with exception");
                await this.dialogService.DisplayAlertAsync("Error", $"Subscribe failed with exception: {ex.Message}", "OK");
            }
        }

        public ICommand UnsubscribeFromAppLinkReceivedEventCommand
        {
            get => this.unsubscribeFromAppLinkReceivedEventCommand ??= new AsyncRelayCommand(this.UnsubscribeFromAppLinkReceivedEventAsync);
        }

        private async Task UnsubscribeFromAppLinkReceivedEventAsync()
        {
            try
            {
                this.appLinkHandler.AppLinkReceived -= this.OnAppLinkReceived;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "UnsubscribeFromAppLinkReceivedEventAsync failed with exception");
                await this.dialogService.DisplayAlertAsync("Error", $"Unsubscribe failed with exception: {ex.Message}", "OK");
            }
        }

        private void OnAppLinkReceived(object sender, AppLinkReceivedEventArgs e)
        {
            this.AppLinkUri = e.Uri.ToString();
        }

        public string AppLinkUri
        {
            get => this.appLinkUri;
            private set => this.SetProperty(ref this.appLinkUri, value);
        }

        public ICommand AddHomeRuleCommand
        {
            get => this.addHomeRuleCommand ??= new AsyncRelayCommand(this.AddHomeRuleAsync);
        }

        private async Task AddHomeRuleAsync()
        {
            try
            {
                this.uriProcessorRules.Add(UriRules.HomeRule);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "AddHomeRuleAsync failed with exception");
                await this.dialogService.DisplayAlertAsync("Error", $"AddHomeRuleAsync failed with exception: {ex.Message}", "OK");
            }
        }

        public ICommand ClearRulesCommand
        {
            get => this.clearRulesCommand ??= new AsyncRelayCommand(this.ClearRulesAsync);
        }

        private async Task ClearRulesAsync()
        {
            try
            {
                this.uriProcessorRules.Clear();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "ClearRulesAsync failed with exception");
                await this.dialogService.DisplayAlertAsync("Error", $"ClearRulesAsync failed with exception: {ex.Message}", "OK");
            }
        }

        public string TestUri
        {
            get => this.testUri;
            set => this.SetProperty(ref this.testUri, value);
        }

        public ICommand ProcessUriCommand
        {
            get => this.processUriCommand ??= new AsyncRelayCommand(this.ProcessUriAsync);
        }

        private async Task ProcessUriAsync()
        {
            try
            {
                var uri = new Uri(this.TestUri);
                this.uriProcessor.Process(uri);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "ProcessUriAsync failed with exception");
                await this.dialogService.DisplayAlertAsync("Error", $"ProcessUriAsync failed with exception: {ex.Message}", "OK");
            }
        }

        public IAsyncRelayCommand<string> NavigateToPageCommand
        {
            get => this.navigateToPageCommand ??= new AsyncRelayCommand<string>(this.NavigateToPageAsync);
        }

        private async Task NavigateToPageAsync(string page)
        {
            await this.navigationService.PushAsync(page);
        }


        public IAsyncRelayCommand<string> NavigateToModalPageCommand
        {
            get => this.navigateToModalPageCommand ??= new AsyncRelayCommand<string>(this.NavigateToModalPageAsync);
        }

        private async Task NavigateToModalPageAsync(string page)
        {
            await this.navigationService.PushModalAsync(page);
        }

        public IAsyncRelayCommand<string> OpenUrlCommand
        {
            get => this.openUrlCommand ??= new AsyncRelayCommand<string>(this.OpenUrlAsync);
        }

        private async Task OpenUrlAsync(string url)
        {
            try
            {
                await this.launcher.TryOpenAsync(url);
            }
            catch
            {
                // Ignore exceptions
            }
        }
    }
}