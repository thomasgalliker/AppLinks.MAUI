# AppLinks.MAUI
[![Version](https://img.shields.io/nuget/v/AppLinks.MAUI.svg)](https://www.nuget.org/packages/AppLinks.MAUI)  [![Downloads](https://img.shields.io/nuget/dt/AppLinks.MAUI.svg)](https://www.nuget.org/packages/AppLinks.MAUI)

This library offers camera preview and barcode scanning functionality for .NET MAUI apps using native platform APIs with **Google ML Kit** and **Apple VisionKit**.

### Download and Install AppLinks.MAUI
This library is available on NuGet: https://www.nuget.org/packages/AppLinks.MAUI
Use the following command to install AppLinks.MAUI using NuGet package manager console:

    PM> Install-Package AppLinks.MAUI

You can use this library in any .NET MAUI project compatible to .NET 8 and higher.

#### App Link Setup for Android Apps
1. Register app link host in `MainActivity` by creating one or more `IntentFilter` with `DataScheme` and `DataHost`.
2. Create `assetlinks.json` file which contains Android package name(s) and sha256_fingerprints used to sign the package.
2. Deploy `assetlinks.json` file to root web folder `.well-known`.
3. Verify the app links for each package name:
   `adb shell pm verify-app-links --re-verify {package_name}`
4. Check if verification was successful:
   `adb shell pm get-app-links {package_name}`

#### App Link Setup for iOS Apps
1. Login to https://developer.apple.com, go to "Certificates, Identifiers & Profiles", select tab "Identifiers".
2. Select the app identifier which should support app links.
3. Enable the option "Associated Domains" and save the changes.
4. Update and download all dependent provisioning profiles.
5. Create `Entitlements.plist` file under Platforms/iOS and add associated domains (plist key: com.apple.developer.associated-domains).
2. Create `apple-app-site-associate` file which contains app identifiers and target URL paths.
3. Deploy `apple-app-site-associate` file to root web folder `.well-known`. 

#### App Setup in .NET MAUI
1. This plugin provides an extension method for MauiAppBuilder `UseAppLinks` which ensure proper startup and initialization.
   Call this method within your `MauiProgram` just as demonstrated in the [AppLinksDemoApp](https://github.com/thomasgalliker/AppLinks.MAUI/tree/develop/Samples):
   ```csharp
   var builder = MauiApp.CreateBuilder()
       .UseMauiApp<App>()
       .UseAppLinks();
   ```

### API Usage
Inject `IAppLinkHandler` or use the static singleton instance `IAppLinkHandler.Current` in your code to get access to the main features of this library.
- `IAppLinkHandler.AppLinkReceived`: This event is fired as soon as an app link URL is received.
- `IAppLinkHandler.ResetCache()`: Clear any cached app link data.

> [!NOTE]
> If an app link is received before the `AppLinkReceived` event is subscribed, the received app link URL is cached and delivered as soon as the event is subscribed.

> [!WARNING]
> App links offer a potential attack vector into your app, so ensure you validate all URI parameters and discard any malformed URIs.

### Contribution
Contributors welcome! If you find a bug or you want to propose a new feature, feel free to do so by opening a new issue on github.com.

### Links
- https://developer.apple.com
- https://developer.apple.com/documentation/xcode/supporting-associated-domains
- https://developer.android.com/training/app-links
- https://developers.google.com/digital-asset-links/tools/generator
- https://learn.microsoft.com/en-us/dotnet/maui/android/app-links
- https://learn.microsoft.com/en-us/dotnet/maui/macios/universal-links
- https://branch.io/resources/aasa-validator