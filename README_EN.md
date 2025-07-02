# <img src="docs/images/icon.png" alt="icon" width="24" height="24"> ColorMC
![](https://img.shields.io/badge/license-Apache2.0-green)
![](https://img.shields.io/github/repo-size/Coloryr/ColorMC)
![](https://img.shields.io/github/stars/Coloryr/ColorMC)
![](https://img.shields.io/github/contributors/Coloryr/ColorMC)
![](https://img.shields.io/github/commit-activity/y/Coloryr/ColorMC)

**Warning: the following content is translated by ChatGPT**

A cross-platform Minecraft PC launcher.

Built with .NET8, UI use the XAML with MVVM, and C# as the backend language.

QQ Group: 571239090

More Languages: [Chinese](README_EN.md)

[User Manual](https://github.com/Coloryr/ColorMC_Pic/blob/master/guide/Main.md) -  
[Changelog](log.md) -  
[Join Translation Efforts](https://crowdin.com/project/colormc) (Need Help!)

## Window Screenshots 🪟
![](/docs/images/run.png)

**Demo Animation**

![](/docs/images/GIF.gif)

## Supported Platforms
- Linux (deb, pkg, rpm, also can be install with [spark-store](https://www.spark-app.store/)or[AUR](https://aur.archlinux.org/))
- Windows
- macOS

**Note: ARM64 platform compatibility is not guaranteed.  
Due to the complexity of Linux distributions, compatibility varies between devices. If it doesn’t work, you may need to troubleshoot it yourself. I have only tested the launcher in my own virtual machine. Driver compatibility issues are not within my scope of support.**

## Installation
Download the pre-built compressed files/installers from [Releases](https://github.com/Coloryr/ColorMC/releases) or [Actions-Beta](https://github.com/Coloryr/ColorMC/actions).  
Extract (zip)/Install (msi, deb, pkg)/or run directly (appimage).

## Launch

- After installation:  
  On Windows/macOS, double-click the extracted file to run.  
  On Linux, double-click to run, or use this command in terminal:
```
$ ColorMC.Launcher
```

- Run from source (requires .NET8 SDK):
```
$ git clone https://github.com/Coloryr/ColorMC.git
$ cd ColorMC/src/ColorMC.Launcher
$ dotnet run
```

## Build from Source

You can build ColorMC from the source code and run it.
After the construction is completed, all binary files can be obtained in the `built_out` folder

### Build Windows Binary
**Must be built on Windows with git and dotnet-8-sdk installed.**

```
git clone https://github.com/Coloryr/ColorMC.git
cd ColorMC

@REM Update source code
.\build\update.cmd

@REM Build binary
.\build\build-windows.cmd
```

### Build Linux Binary
**Must be built on Linux with git and dotnet-8-sdk installed.**
```
$ git clone https://github.com/Coloryr/ColorMC.git
$ cd ColorMC
$ chmod a+x ./build/update.sh
$ chmod a+x ./build/build-linux.sh
```
Update source code
```
./build/update.sh
```
Build binary
```
./build/build-linux.sh
```

### Package Linux Images

- Packaging Ubuntu Images  
**Need to operate on Ubuntu system**
```
$ chmod a+x ./build/build-ubuntu.sh
$ ./build/build-ubuntu.sh
```
- Packaging RPM Images  
**Need to operate on Ubuntu system**
```
$ chmod a+x ./build/build-rpm.sh
$ ./build/build-rpm.sh
```
- Packaging Arch Images  
**Need to operate on Arch system**
```
$ chmod a+x ./build/build-arch.sh
$ ./build/build-arch.sh
```

### Build macOS Binary
**Must be built on Ubuntu or macOS with git and dotnet-8-sdk installed.**
```
$ git clone https://github.com/Coloryr/ColorMC.git
$ cd ColorMC
$ chmod a+x ./build/update.sh
$ chmod a+x ./build/build-macos.sh
```
Update source code
```
./build/update.sh
```
Build binary
```
./build/build-macos.sh
```

## Development

Clone the repository:
```
$ git clone https://github.com/Coloryr/ColorMC.git
$ git submodule update --init --recursive
```

The main solution file is `./src/ColorMC.sln`.

### Using ColorMC Launcher Core

[How to develop your own launcher using ColorMC Core](Core.md)

### Project Modules Overview
| Module            | Description                                 |
|-------------------|---------------------------------------------|
| ColorMC.Core      | Launcher core                               |
| ColorMC.CustomGui | Custom launcher UI [Tutorial](CustomGui.md) |
| ColorMC.Cmd       | Command-line mode (Deprecated)              |
| ColorMC.Gui       | GUI mode                                    |
| ColorMC.Launcher  | Launcher main application                   |
| ColorMC.Test      | For launcher testing                        |
| ColorMC.Setup     | For building Windows msi installer          |

## Dependencies/Referenced Projects
| 名称                    | 描述              | 链接                                                             |
|-----------------------|-----------------|----------------------------------------------------------------|
| AvaloniaUI            | .NET UI          | [GitHub](https://github.com/AvaloniaUI/Avalonia)               |
| DialogHost.Avalonia   | AvaloniaUI dialog control             | [GitHub](https://github.com/AvaloniaUtils/DialogHost.Avalonia) |
| CommunityToolkit.Mvvm | .NET Community Toolkit          | [GitHub](https://github.com/CommunityToolkit/dotnet)           |
| Svg.Skia              | An SVG rendering library         | [GitHub](https://github.com/wieslawsoltes/Svg.Skia)            |
| SkiaSharp             | 2D graphics API         | [GitHub](https://github.com/mono/SkiaSharp)                    |
| Silk.NET              | bindings library        | [GitHub](https://github.com/dotnet/Silk.NET)                   |              |
| HtmlAgilityPack       | HTML parser         | [GitHub](https://github.com/zzzprojects/html-agility-pack)                           |
| Jint                  | Javascript Interpreter for .NET         | [GitHub](https://github.com/sebastienros/jint)                 |
| DotNetty              | a port of netty          | [GitHub](https://github.com/Azure/DotNetty)                    |
| Newtonsoft.Json       | JSON framework for .NET         | [GitHub](https://github.com/JamesNK/Newtonsoft.Json)                          |
| SharpZipLib           | Zip Tool           | [GitHub](https://github.com/icsharpcode/SharpZipLib)           |
| Tomlyn                | TOML parser         | [GitHub](https://github.com/xoofx/Tomlyn)                      |
| ForgeWrapper          | Forge Launcher for Minecraft       | [GitHub](https://github.com/Coloryr/ForgeWrapper)              |         |
| OptifineWrapper       | Optifine Launcher for Minecraft     | [GitHub](https://github.com/coloryr/OptifineWrapper)           |
| ColorMCASM            | ColorMC with game channel | [GitHub](https://github.com/Coloryr/ColorMCASM)                |
| K4os.Compression.LZ4  | LZ4/LH4HC compression           | [GitHub](https://github.com/MiloszKrajewski/K4os.Compression.LZ4)  |
| Ae.Dns                | DNS clients           | [GitHub](https://github.com/alanedwardes/Ae.Dns)  |

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

### Development Tools
- [Visual Studio Code](https://code.visualstudio.com/)  
- [Visual Studio 2022](https://visualstudio.microsoft.com/)  
- ![dotMemory logo](https://resources.jetbrains.com/storage/products/company/brand/logos/dotMemory_icon.svg)