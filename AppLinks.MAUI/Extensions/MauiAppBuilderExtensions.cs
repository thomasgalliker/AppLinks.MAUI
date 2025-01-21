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
        public static MauiAppBuilder UseAppLinks(this MauiAppBuilder builder)
        {
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
                            Task.Run(() => HandleAppLink(data));
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
                            Task.Run(() => HandleAppLink(data));
                        }
                    });
                });
#elif IOS
                lifecycle.AddiOS(ios =>
                {
                    // Universal link delivered to FinishedLaunching after app launch.
                    ios.FinishedLaunching((app, _) =>
                    {
                        HandleAppLink(app.UserActivity);
                        return true;
                    });

                    ios.OpenUrl((_, url, _) => HandleAppLink(url));

                    // Universal link delivered to ContinueUserActivity when the app is running or suspended.
                    ios.ContinueUserActivity((_, userActivity, _) => HandleAppLink(userActivity));

                    // Only required if using Scenes for multi-window support.
                    if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13))
                    {
                        // Universal link delivered to SceneWillConnect after app launch
                        ios.SceneWillConnect((_, _, sceneConnectionOptions)
                            => HandleAppLink(sceneConnectionOptions.UserActivities.ToArray()
                                .FirstOrDefault(a => a.ActivityType == NSUserActivityType.BrowsingWeb)));

                        // Universal link delivered to SceneContinueUserActivity when the app is running or suspended
                        ios.SceneContinueUserActivity((_, userActivity) => HandleAppLink(userActivity));
                    }
                });
#endif
            });

            builder.Services.TryAddSingleton<IMainThread>(_ => EssentialsMainThread.Current);
            builder.Services.TryAddSingleton<IAppLinkHandler>(_ => AppLinkHandler.Current);
#endif

            return builder;
        }

#if IOS
        private static bool HandleAppLink(NSUserActivity userActivity)
        {
            if (userActivity != null &&
                userActivity.ActivityType == NSUserActivityType.BrowsingWeb &&
                userActivity.WebPageUrl is NSUrl webPageUrl)
            {
                HandleAppLink(webPageUrl);
                return true;
            }

            return false;
        }

        private static bool HandleAppLink(NSUrl url)
        {
            try
            {
                HandleAppLink((Uri)url);
                return true;
            }
            catch
            {
                return false;
            }
        }

#endif

        private static void HandleAppLink(string url)
        {
            if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
            {
                HandleAppLink(uri);
            }
        }

        private static void HandleAppLink(Uri uri)
        {
            Application.Current?.SendOnAppLinkRequestReceived(uri);
            AppLinkHandler.Current.EnqueueAppLink(uri);
        }
    }
}