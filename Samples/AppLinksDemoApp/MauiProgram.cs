using System.Collections;
using AppLinksDemoApp.Services.Logging;
using AppLinksDemoApp.Services.Navigation;
using AppLinksDemoApp.ViewModels;
using AppLinksDemoApp.Views;
using AppLinks.MAUI;
using AppLinksDemoApp.Services;
using AppLinksDemoApp.Services.AppLinks;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace AppLinksDemoApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseAppLinks()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("IBMPlexMono-Bold.ttf", "IBMPlexMonoBold");
                    fonts.AddFont("IBMPlexMono-Regular.ttf", "IBMPlexMonoRegular");
                    fonts.AddFont("IBMPlexSans-Bold.ttf", "IBMPlexSansBold");
                    fonts.AddFont("IBMPlexSans-Regular.ttf", "IBMPlexSansRegular");
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });


            builder.Services.AddLogging(b =>
            {
                b.ClearProviders();
                b.SetMinimumLevel(LogLevel.Trace);
                b.AddNLog(NLogLoggerConfiguration.GetLoggingConfiguration());
                //b.AddSentry(SentryConfiguration.Configure);
            });

            // Register services
            builder.Services.AddSingleton<INavigationService, MauiNavigationService>();
            builder.Services.AddSingleton<IDialogService, DialogService>();
            builder.Services.AddSingleton<ILauncher>(_ => Launcher.Default);
            builder.Services.AddSingleton<IBrowser>(_ => Browser.Default);

            // Register pages and view models
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddSingleton<IAppLinkRules, CustomAppLinkRules>();

            return builder.Build();
        }
    }
}
