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
        private readonly ILauncher launcher;

        private IAsyncRelayCommand appearingCommand;
        private bool isInitialized;
        private string appLinkUrl;
        private IAsyncRelayCommand subscribeToAppLinkReceivedEventCommand;
        private IAsyncRelayCommand<string> navigateToPageCommand;
        private IAsyncRelayCommand<string> navigateToModalPageCommand;
        private IAsyncRelayCommand<string> openUrlCommand;

        public MainViewModel(
            ILogger<MainViewModel> logger,
            INavigationService navigationService,
            IDialogService dialogService,
            IAppLinkHandler appLinkHandler,
            ILauncher launcher)
        {
            this.logger = logger;
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            this.appLinkHandler = appLinkHandler;
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
                await this.SubscribeToAppLinkReceivedEventAsync();
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
                await this.dialogService.DisplayAlertAsync("Error", $"Failed to subscribe to AppLinkReceived event: {ex.Message}", "OK");
            }
        }

        private void OnAppLinkReceived(object sender, AppLinkReceivedEventArgs e)
        {
            this.AppLinkUrl = e.Uri.ToString();
        }

        public string AppLinkUrl
        {
            get => this.appLinkUrl;
            private set => this.SetProperty(ref this.appLinkUrl, value);
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