using AppLinks.MAUI.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.LifecycleEvents;

#if IOS
using Foundation;
#endif

#if ANDROID
using Android.Content;
#endif

namespace AppLinks.MAUI
{
    public static class MauiAppBuilderExtensions
    {
        public static MauiAppBuilder UseAppLinks(this MauiAppBuilder builder, Action<AppLinkOptions> options = null)
        {
            var defaultOptions = new AppLinkOptions();
            options?.Invoke(defaultOptions);

#if (ANDROID || IOS)

            builder.ConfigureLifecycleEvents(lifecycle =>
            {
#if ANDROID
                lifecycle.AddAndroid(android =>
                {
                    // Called, when the app is not running/on the activity stack and started from scratch.
                    // The App link is included in the activity.Intent.Data parameter in this case.
                    // Intent action is "view".
                    android.OnCreate((activity, _) =>
                    {
                        var action = activity.Intent?.Action;
                        var data = activity.Intent?.Data?.ToString();

                        if (action == Intent.ActionView && data is not null)
                        {
                            Task.Run(() => HandleAppLink(data, defaultOptions));
                        }
                    });

                    // Called, when the app is present on the activity stack and brought back to the front.
                    // The App Link is included in the intent.Data parameter attribute,
                    // NOT in the activity.Intent.Data parameter attribute in this case.
                    // Intent action is "main".
                    android.OnNewIntent((activity, intent) =>
                    {
                        var action = activity.Intent?.Action;
                        var data = intent?.Data?.ToString();

                        if (action == Intent.ActionMain && data is not null)
                        {
                            Task.Run(() => HandleAppLink(data, defaultOptions));
                        }
                    });
                });
#elif IOS
                lifecycle.AddiOS(ios =>
                {
                    // Universal link delivered to FinishedLaunching after app launch.
                    ios.FinishedLaunching((app, _) =>
                    {
                        HandleAppLink(app.UserActivity, defaultOptions);
                        return true;
                    });

                    ios.OpenUrl((_, url, _) => HandleAppLink(url, defaultOptions));

                    // Universal link delivered to ContinueUserActivity when the app is running or suspended.
                    ios.ContinueUserActivity((_, userActivity, _) => HandleAppLink(userActivity, defaultOptions));

                    // Only required if using Scenes for multi-window support.
                    if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13))
                    {
                        // Universal link delivered to SceneWillConnect after app launch
                        ios.SceneWillConnect((_, _, sceneConnectionOptions)
                            =>
                        {
                            var userActivity = sceneConnectionOptions.UserActivities.ToArray()
                                .FirstOrDefault(a => a.ActivityType == NSUserActivityType.BrowsingWeb);
                            HandleAppLink(userActivity, defaultOptions);
                        });

                        // Universal link delivered to SceneContinueUserActivity when the app is running or suspended
                        ios.SceneContinueUserActivity((_, userActivity) => HandleAppLink(userActivity, defaultOptions));
                    }
                });
#endif
            });
#endif

            builder.Services.AddSingleton<AppLinkOptions>(defaultOptions);
            builder.Services.AddSingleton<IMainThread>(_ => EssentialsMainThread.Current);
            builder.Services.AddSingleton<IAppLinkHandler>(_ => IAppLinkHandler.Current);
            builder.Services.AddSingleton<IAppLinkRuleManager>(_ => IAppLinkRuleManager.Current);
            builder.Services.AddSingleton<IAppLinkProcessor>(_ => IAppLinkProcessor.Current);

            return builder;
        }

#if IOS
        private static bool HandleAppLink(NSUserActivity userActivity, AppLinkOptions options)
        {
            if (userActivity != null &&
                userActivity.ActivityType == NSUserActivityType.BrowsingWeb &&
                userActivity.WebPageUrl is NSUrl webPageUrl)
            {
                HandleAppLink(webPageUrl, options);
                return true;
            }

            return false;
        }

        private static bool HandleAppLink(NSUrl url, AppLinkOptions options)
        {
            try
            {
                HandleAppLink((Uri)url, options);
                return true;
            }
            catch
            {
                return false;
            }
        }

#endif

        private static void HandleAppLink(string url, AppLinkOptions options)
        {
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
            {
                HandleAppLink(uri, options);
            }
        }

        private static void HandleAppLink(Uri uri, AppLinkOptions options)
        {
            if (options.EnableSendOnAppLinkRequestReceived)
            {
                Application.Current?.SendOnAppLinkRequestReceived(uri);
            }

            AppLinkHandler.Current.EnqueueAppLink(uri);
        }
    }
}