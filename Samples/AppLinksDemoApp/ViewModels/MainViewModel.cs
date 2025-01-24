using System.Windows.Input;
using AppLinksDemoApp.Services.Navigation;
using AppLinks.MAUI;
using AppLinks.MAUI.Services;
using AppLinksDemoApp.Services.AppLinks;
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
        private readonly IAppLinkProcessor appLinkProcessor;
        private readonly IAppLinkRuleManager appLinkRuleManager;
        private readonly CustomAppLinkRules appLinkRules;
        private readonly ILauncher launcher;

        private IAsyncRelayCommand appearingCommand;
        private bool isInitialized;
        private string appLinkUri;
        private IAsyncRelayCommand subscribeToAppLinkReceivedEventCommand;
        private IAsyncRelayCommand unsubscribeFromAppLinkReceivedEventCommand;
        private IAsyncRelayCommand addHomeRuleCommand;
        private IAsyncRelayCommand clearRulesCommand;
        private IAsyncRelayCommand processUriCommand;
        private IAsyncRelayCommand clearPendingUrisCommand;
        private IAsyncRelayCommand<string> navigateToPageCommand;
        private IAsyncRelayCommand<string> navigateToModalPageCommand;
        private IAsyncRelayCommand<string> openUrlCommand;
        private string testUri;

        public MainViewModel(
            ILogger<MainViewModel> logger,
            INavigationService navigationService,
            IDialogService dialogService,
            IAppLinkHandler appLinkHandler,
            IAppLinkProcessor appLinkProcessor,
            IAppLinkRuleManager appLinkRuleManager,
            IAppLinkRules appLinkRules,
            ILauncher launcher)
        {
            this.logger = logger;
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            this.appLinkHandler = appLinkHandler;
            this.appLinkProcessor = appLinkProcessor;
            this.appLinkRuleManager = appLinkRuleManager;
            this.appLinkRules = (CustomAppLinkRules)appLinkRules;
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

                // Register for app link calls backs.
                // We use the static app link rules here.
                // IAppLinkProcessor.Current.RegisterCallback(StaticAppLinkRules.HomeRule,
                //     uri => this.dialogService.DisplayAlertAsync(
                //         StaticAppLinkRules.HomeRule.RuleId,
                //         $"Callback for rule \"{StaticAppLinkRules.HomeRule.RuleId}\"{Environment.NewLine}{Environment.NewLine}" +
                //         $"URI {uri}",
                //         "OK"));

                // Register for app link calls backs.
                // We use injected app link rules here.
                this.appLinkProcessor.RegisterCallback(this.appLinkRules.HomeRule,
                    uri => this.dialogService.DisplayAlertAsync(
                        this.appLinkRules.HomeRule.RuleId,
                        $"Callback for rule \"{this.appLinkRules.HomeRule.RuleId}\"{Environment.NewLine}{Environment.NewLine}" +
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
                // Add/update the app link rule "HomeRule" using static app link rules.
                // IAppLinkRuleManager.Current.Add(StaticAppLinkRules.HomeRule);

                // Add/update the app link rule "HomeRule" using injected app link rules.
                this.appLinkRuleManager.Add(this.appLinkRules.HomeRule);
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
                this.appLinkRuleManager.Clear();
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
                this.appLinkProcessor.Process(uri);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "ProcessUriAsync failed with exception");
                await this.dialogService.DisplayAlertAsync("Error", $"ProcessUriAsync failed with exception: {ex.Message}", "OK");
            }
        }

        public ICommand ClearPendingUrisCommand
        {
            get => this.clearPendingUrisCommand ??= new AsyncRelayCommand(this.ClearPendingUrisAsync);
        }

        private async Task ClearPendingUrisAsync()
        {
            try
            {
                this.appLinkProcessor.Clear();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "ClearPendingUrisAsync failed with exception");
                await this.dialogService.DisplayAlertAsync("Error", $"ClearPendingUrisAsync failed with exception: {ex.Message}", "OK");
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