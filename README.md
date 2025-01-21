# AppLinks.MAUI
[![Version](https://img.shields.io/nuget/v/AppLinks.MAUI.svg)](https://www.nuget.org/packages/AppLinks.MAUI)  [![Downloads](https://img.shields.io/nuget/dt/AppLinks.MAUI.svg)](https://www.nuget.org/packages/AppLinks.MAUI)

This library offers camera preview and barcode scanning functionality for .NET MAUI apps using native platform APIs with **Google ML Kit** and **Apple VisionKit**.

### Download and Install AppLinks.MAUI
This library is available on NuGet: https://www.nuget.org/packages/AppLinks.MAUI
Use the following command to install AppLinks.MAUI using NuGet package manager console:

    PM> Install-Package AppLinks.MAUI

You can use this library in any .NET MAUI project compatible to .NET 8 and higher.

#### App Setup
1. This plugin provides an extension method for MauiAppBuilder `UseAppLinks` which ensure proper startup and initialization.
   Call this method within your `MauiProgram` just as demonstrated in the [AppLinksDemoApp](https://github.com/thomasgalliker/AppLinks.MAUI/tree/develop/Samples):
   ```csharp
   var builder = MauiApp.CreateBuilder()
       .UseMauiApp<App>()
       .UseAppLinks();
   ```
2. tbd

### API Usage
tbd

### Contribution
Contributors welcome! If you find a bug or you want to propose a new feature, feel free to do so by opening a new issue on github.com.

### Links
- tbd
- tbd