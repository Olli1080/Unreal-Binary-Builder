Unreal Engine Binary Builder
======================

<a href="https://www.buymeacoffee.com/ryanjon2040" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/yellow_img.png" alt="Buy Me A Coffee" style="height: 41px !important;width: 174px !important;box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;-webkit-box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;" ></a>

This is a cross-platform desktop application designed to create binary builds (Installed Builds) of [Unreal Engine](https://www.unrealengine.com/) and plugins directly from [GitHub source](https://github.com/EpicGames/UnrealEngine).

[![Discord](https://img.shields.io/discord/591914197219016707.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2)](https://discord.gg/zBMrKRdwgw)

[<img src="https://img.shields.io/twitter/follow/ryanjon2040.svg?style=popout">](https://twitter.com/ryanjon2040)

![](https://github.com/ryanjon2040/UE4-Binary-Builder/actions/workflows/build-ubb.yml/badge.svg)
![](https://img.shields.io/github/last-commit/ryanjon2040/UE4-Binary-Builder.svg?style=popout) 
![](https://img.shields.io/github/license/ryanjon2040/UE4-Binary-Builder.svg?style=popout) ![](https://img.shields.io/github/downloads/ryanjon2040/UE4-Binary-Builder/total.svg?style=popout) 

### 🚀 v4.0 Avalonia Port Update
Unreal Binary Builder has been completely rewritten using **Avalonia UI** and **.NET 10.0**. This massive architectural upgrade brings the application natively to **Windows, Linux, and macOS** with a modern Fluent UI, improved asynchronous build orchestration (no more UI freezing during long compilations!), and integrated cross-platform updates.

# Features

* **Cross-Platform**: Run on Windows, Linux, or macOS.
* **Full Build Orchestration**: Chain `Setup.bat`, `GenerateProjectFiles`, and `AutomationTool` executions.
* **Plugin Builder**: Build and package Unreal Engine plugins for the Marketplace across multiple platforms.
* **Visual Studio Integration**: Dynamically detects your VS installation, edition, and architecture.
* **Integrated Code Editor**: Edit `.Target.cs` files directly within the app using AvaloniaEdit.
* **Advanced Options**: Configure DDC, signing, custom XML build scripts, Git dependency caching, and more.
* **Real-time Logging**: Monitor build progress, view compiled file counts, and export logs.

# How to use (Compiling Engine)

###### Step I 
- Download the latest release of Unreal Binary Builder for your OS.

###### Step II
- Clone or Download UE4/UE5 source from GitHub.

###### Step III
- Open Unreal Binary Builder.
- Navigate to the **Engine Builder** tab -> **Setup** sub-tab.
- Click *Browse* and select the **root folder** of your downloaded Engine (where **_Setup.bat_** exists).
- Choose your Setup Options and Git Dependency settings, then click **Start Setup Process**.

###### Step IV
- Navigate to the **Compile** sub-tab.
- Select your target platforms, build options (like DDC, Clean Build), and Game Configurations.
- Click **Build Unreal Engine**.

# Troubleshoot

**Access Denied on some files?**</br>
On Windows, just change the ownership to Users then try again. To change ownership on Windows, follow these steps
 - Right click on the UE4 folder, choose Properties
 - Switch to Security tab
 - Click on Advanced
 - Near the top, click on Change User
 - A new dialog will open, in the text box at bottom, type in "Users", then click Check Names
 - OK till the end.

#### Dependencies & Acknowledgements

* [Avalonia UI](https://avaloniaui.net/) - The premier cross-platform UI framework for .NET.
* [FluentAvalonia](https://github.com/amwx/FluentAvalonia) by [amwx](https://github.com/amwx)
* [AvaloniaEdit](https://github.com/AvaloniaUI/AvaloniaEdit)
* [LibGit2Sharp](https://github.com/libgit2/libgit2sharp)
* [GameAnalytics](https://github.com/GameAnalytics/GA-SDK-C-SHARP) by [Game Analytics](https://gameanalytics.com/)
* [Newtonsoft.Json](https://www.newtonsoft.com/json)
* [Sentry.NET](https://github.com/getsentry/sentry-dotnet) by [Sentry](https://sentry.io/)
* [NetSparkle](https://github.com/NetSparkleUpdater/NetSparkle) by [NetSparkleUpdater](https://github.com/NetSparkleUpdater)
* CommunityToolkit.Mvvm
