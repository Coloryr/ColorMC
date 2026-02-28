# <img src="docs/images/icon.png" alt="icon" width="24" height="24"> ColorMC
![](https://img.shields.io/badge/license-Apache2.0-green)
![](https://img.shields.io/github/repo-size/Coloryr/ColorMC)
![](https://img.shields.io/github/stars/Coloryr/ColorMC)
![](https://img.shields.io/github/contributors/Coloryr/ColorMC)
![](https://img.shields.io/github/commit-activity/y/Coloryr/ColorMC)

A cross-platform Minecraft PC launcher

Built with .NET 10 as the runtime environment, XAML as the frontend language using MVVM pattern, and C# as the backend language

QQ Group: 571239090

More Languages: [中文](README.md)

[User Manual](https://github.com/Coloryr/ColorMC_Pic/blob/master/guide/README.md) -
[Changelog](log.md) -
[Join Multi-language Translation](https://crowdin.com/project/colormc)(Please help)

## Window Screenshots 🪟
![](/docs/images/image.png)  

**Demo Animation**

https://github.com/user-attachments/assets/4b3b1207-58a9-46e8-a99c-b9f1a64761be

## Supported Platforms
- Windows (zip)
- Linux (provides deb, pkg, rpm packages, also available on [Spark Store](https://www.spark-app.store/) or [AUR](https://aur.archlinux.org/))
- macOS (zip, dmg)

**Note: ARM64 platform compatibility is not guaranteed  
Due to the complexity of Linux distributions, compatibility varies across different computers. If the launcher fails to start, you may need to resolve issues yourself. I only test startup in my own virtual machine, and driver compatibility issues are beyond my consideration scope.**

## Installation
Download the pre-built packages/installers from [Releases - Official Releases](https://github.com/Coloryr/ColorMC/releases) or [Actions - Beta Releases](https://github.com/Coloryr/ColorMC/actions)  
Extract (zip) / Install (msi, deb, pkg) / or run directly (appimage)

## Launching

- Launch after installation  
On Windows/MacOS, extract and double-click to launch  
On Linux, after installation, you can double-click to launch or use the terminal command:
```
$ ColorMC.Launcher
```

- Launch from source code (requires .NET 10 SDK installation)
```
$ git clone https://github.com/Coloryr/ColorMC.git
$ cd ColorMC
$ git submodule update --init --recursive
$ cd src/ColorMC.Launcher
$ dotnet run
```

## Building from Source

You can build ColorMC from source code and run it  
After building, you can get all binary files in the `built_out` folder

### Building `windows` binaries  
**Requires Windows system with git and dotnet-10-sdk installed**

```
git clone https://github.com/Coloryr/ColorMC.git
cd ColorMC

@REM Update source code
.\build\update.cmd

@REM Build
.\build\build-windows.cmd
```

### Building `linux` binaries  
**Requires Linux system with git and dotnet-10-sdk installed**
```
$ git clone https://github.com/Coloryr/ColorMC.git
$ cd ColorMC
$ chmod a+x ./build/update.sh
$ chmod a+x ./build/build-linux.sh
```
Update source code
```
$ ./build/update.sh
```
Build
```
$ ./build/build-linux.sh
```

### Packaging Linux-related installation images

- Package Ubuntu image  
**Requires Ubuntu system**
```
$ chmod a+x ./build/build-ubuntu.sh
$ ./build/build-ubuntu.sh
```
- Package rpm image  
**Requires Ubuntu system**
```
$ chmod a+x ./build/build-rpm.sh
$ ./build/build-rpm.sh
```
- Package Arch image  
**Requires Arch system**
```
$ chmod a+x ./build/build-arch.sh
$ ./build/build-arch.sh
```

### Building `macos` binaries  
**Requires macOS system with git and dotnet-10-sdk installed**
```
$ git clone https://github.com/Coloryr/ColorMC.git
$ cd ColorMC
$ chmod a+x ./build/update.sh
$ chmod a+x ./build/build-macos.sh
```
Update source code
```
$ ./build/update.sh
```
Build
```
$ ./build/build-macos.sh
```
- Package Dmg image
**Requires macOS system**
```
$ ./build/build-dmg.sh
```

## Secondary Development

First clone the code
```
$ git clone https://github.com/Coloryr/ColorMC.git
$ git submodule update --init --recursive
```

`./src/ColorMC.sln` is the root project

### Using ColorMC Launcher Core

[Use ColorMC Launcher Core to develop your own launcher](docs/Core.md)

### Project Description
| Module               | Description                          |
|----------------------|--------------------------------------|
| ColorMC.Core         | Launcher core                        |
| ColorMC.CustomGui    | Custom launcher interface [Tutorial](docs/CustomGui.md) |
| ColorMC.Cmd          | Command line mode (deprecated)       |
| ColorMC.Gui          | GUI mode                            |
| ColorMC.Launcher     | Launcher main program                |
| ColorMC.Test         | For launcher testing                 |
| ColorMC.Setup.Wix    | For building Windows msi installer   |

## Dependencies/Referenced Projects
| Name                    | Description           | Link                                                             |
|-------------------------|-----------------------|------------------------------------------------------------------|
| AvaloniaUI              | Cross-platform UI framework | [GitHub](https://github.com/AvaloniaUI/Avalonia)               |
| Ae.Dns                  | DNS client            | [GitHub](https://github.com/alanedwardes/Ae.Dns)                |
| HtmlAgilityPack         | HTML parser           | [GitHub](https://github.com/zzzprojects/html-agility-pack)      |
| Jint                    | JS parser/executor    | [GitHub](https://github.com/sebastienros/jint)                  |
| DialogHost.Avalonia     | Dialog library        | [GitHub](https://github.com/AvaloniaUtils/DialogHost.Avalonia)  |
| CommunityToolkit.Mvvm   | MVVM tools            | [GitHub](https://github.com/CommunityToolkit/dotnet)            |
| Svg.Skia                | Svg image display     | [GitHub](https://github.com/wieslawsoltes/Svg.Skia)             |
| SkiaSharp               | Skia image library    | [GitHub](https://github.com/mono/SkiaSharp)                     |
| Silk.NET                | High-performance low-level library interface | [GitHub](https://github.com/dotnet/Silk.NET) |
| DotNetty                | Asynchronous communication framework | [GitHub](https://github.com/Azure/DotNetty) |
| Tomlyn                  | TOML parser           | [GitHub](https://github.com/xoofx/Tomlyn)                       |
| ForgeWrapper            | Forge launcher        | [GitHub](https://github.com/PrismLauncher/ForgeWrapper)         |
| OptifineWrapper         | Optifine launcher     | [GitHub](https://github.com/coloryr/OptifineWrapper)            |
| ColorMCASM              | For ColorMC to communicate with in-game | [GitHub](https://github.com/Coloryr/ColorMCASM) |
| K4os.Compression.LZ4    | LZ4 decompression     | [GitHub](https://github.com/MiloszKrajewski/K4os.Compression.LZ4) |
| sharpcompress           | Archive decompression | [GitHub](https://github.com/adamhathcock/sharpcompress)         |
| Markdig                 | MarkDown processing tool | [GitHub](https://github.com/xoofx/markdig)                   |
| MinecraftSkinRender     | Minecraft skin renderer | [GitHub](https://github.com/Coloryr/MinecraftSkinRender)      |

## Open Source License
Apache 2.0  

```
Copyright 2024 coloryr

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
```

Subsidiary open source licenses: MIT, BSD

## IDE Development Tools Used
- [Visual Studio Code](https://code.visualstudio.com/)  
- [Visual Studio 2022](https://visualstudio.microsoft.com/)  
- ![dotMemory logo](https://resources.jetbrains.com/storage/products/company/brand/logos/dotMemory_icon.svg)