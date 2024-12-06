# <img src="docs/images/icon.png" alt="icon" width="24" height="24"> ColorMC
![](https://img.shields.io/badge/license-Apache2.0-green)
![](https://img.shields.io/github/repo-size/Coloryr/ColorMC)
![](https://img.shields.io/github/stars/Coloryr/ColorMC)
![](https://img.shields.io/github/contributors/Coloryr/ColorMC)
![](https://img.shields.io/github/commit-activity/y/Coloryr/ColorMC)

**Warning: the following content is translated by ChatGPT**

A cross-platform Minecraft PC launcher.

Built with .NET8 as the runtime environment, XAML as the frontend language, and C# as the backend language.

QQ Group: 571239090

[User Manual](https://github.com/Coloryr/ColorMC_Pic/blob/master/guide/Main.md) -  
[Changelog](../log.md) -  
[Join Translation Efforts](https://crowdin.com/project/colormc) (Help us!)

## Window Screenshots 🪟
![](/docs/images/run.png)

**Demo Animation**

![](/docs/images/GIF.gif)

## Supported Platforms
- Linux (providing deb, pkg, rpm)
- Windows
- macOS

**Note: ARM64 platform compatibility is not guaranteed.  
Due to the complexity of Linux distributions, compatibility varies between devices. If it doesn’t work, you may need to troubleshoot it yourself. I have only tested the launcher in my own virtual machine. Driver compatibility issues are not within my scope of support.**

## Installation
Download the pre-built compressed files/installers from [Releases](https://github.com/Coloryr/ColorMC/releases) or [Actions](https://github.com/Coloryr/ColorMC/actions).  
Extract (zip)/Install (msi, deb, pkg)/or run directly (appimage).

On Windows, you can use winget for installation:
```
winget install colormc
```
The default installation path is `C:\Program Files\ColorMC`.

## Launch

- After installation:  
  On Windows/macOS, double-click the extracted file to run.  
  On Linux, double-click to run, or use:
```
ColorMC.Launcher
```

- Run from source (requires .NET8 SDK):
```
git clone https://github.com/Coloryr/ColorMC.git
cd ColorMC/src/ColorMC.Launcher
dotnet run
```

## Build from Source

### Build Windows Binary
**Must be built on Windows with git and dotnet-8-sdk installed.**

```cmd
git clone https://github.com/Coloryr/ColorMC.git
cd ColorMC

@REM Update source code
.\build\update.cmd

@REM Build binary
.\build\build-windows.cmd
```

### Build Linux Binary
**Must be built on Linux with git and dotnet-8-sdk installed.**
```bash
git clone https://github.com/Coloryr/ColorMC.git
cd ColorMC
chmod a+x ./build/update.sh
chmod a+x ./build/build-linux.sh

# Update source code
./build/update.sh

# Build binary
./build/build-linux.sh
```

#### Package Images
**Building Ubuntu, rpm, and Arch images requires specific scripts. Run the respective script on the appropriate system.**

### Build macOS Binary
**Must be built on Ubuntu or macOS with git and dotnet-8-sdk installed.**
```bash
git clone https://github.com/Coloryr/ColorMC.git
cd ColorMC
chmod a+x ./build/update.sh
chmod a+x ./build/build-macos.sh

# Update source code
./build/update.sh

# Build binary
./build/build-macos.sh
```

After building, all binaries can be found in the `built_out` folder.

## Development

Clone the repository:
```
git clone https://github.com/Coloryr/ColorMC.git
git submodule update --init --recursive
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
The project uses several dependencies, including UI frameworks (AvaloniaUI), dialog libraries (DialogHost.Avalonia), and more. Full details are in the original Markdown file.

## Open Source License
This project is licensed under Apache 2.0. For detailed terms, see the license at the end of this file.

### Development Tools
Recommended tools:
- Visual Studio Code
- Visual Studio 2022
- dotMemory

---

Let me know if you need further adjustments!