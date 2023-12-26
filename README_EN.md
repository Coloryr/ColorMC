# ColorMC A new Minecraft Launcher  

[简体中文](./README.md)

**The following content is machine translation**  
**Notie: Translations from other languages are not yet ready. If possible, welcome to do the translation together**  
https://crowdin.com/project/colormc

Using dotnet8 as the runtime environment, XAML as the `UI Front` language, and C # as the `Back-End` language

[User's manual](https://github.com/Coloryr/ColorMC_Pic/blob/master/guide/Main.md)

![](/image/run.png)  

**Running animation**

![](/image/GIF.gif)  

## Supporting platforms
- Linux
- Windows
- macOs

Note: ARM64 platform cannot guarantee its compatibility  
Windows ARM64 can run, but there are rendering issues  
Linux ARM64 can run on `Linux arm development board`, but it runs slowly  
Mac ARM64 cannot run, it can run x64 version  

Due to the complexity of Linux distributions, everyone's computer compatibility is different. If it cannot be opened, it can be resolved on its own

## Install
Download the built compressed/installation package from Releases or Actions  
Extract (zip) \ install (exe, deb, pkg) \ or run (appimage) directly

Under Windows, you can use Winget installation (it should not be ready yet)
```
winget install colormc
```
Default installation on `D:\ColorMC`

## Start program

- Start after installation is completed  
Double click to start after decompressing in Windows/macos  
Under Linux, you can double-click to start or
```
ColorMC.Launcher
```

- Starting from source code
```
git clone https://github.com/Coloryr/ColorMC.git
cd ColorMC/src/ColorMC.Launcher
dotnet run
```

## Build from source code

- Build binary files for `windows`, `ubuntu`, and`macos`  
**Need to build in Ubuntu system, with git and dotnet-8-sdk**
```
git clone https://github.com/Coloryr/ColorMC.git
cd ColorMC
chmod a+x ./build/update.sh
chmod a+x ./build/build-macos.sh
chmod a+x ./build/build-ubuntu.sh
chmod a+x ./build/build-windows.sh

# Update source code
./build/update.sh

# Build
./build/build-windows.sh
./build/build-macos.sh
./build/build-ubuntu.sh
```

- Build binary files for `arch`  
**Need to build in Arch system, with git and dotnet-8-sdk**
```
git clone https://github.com/Coloryr/ColorMC.git
cd ColorMC
chmod a+x ./build/update.sh
chmod a+x ./build/build-arch.sh

# Update source code
./build/update.sh

# Build
./build/build-arch.sh
```

At this point, you can click on the `build_out` folder retrieves all binary files

## Development

```
git clone https://github.com/Coloryr/ColorMC.git
```

Open`./src/ColorMC.sln`using IDE

## DESCRIPTION
- ColorMC.Core Core
- ColorMC.Cmd CLI (已放弃)
- ColorMC.Gui Gui
- ColorMC.Launcher Boot
- ColorMC.Test Core Test's

## Dependent/Referenced Projects
[AvaloniaUI](https://github.com/AvaloniaUI/Avalonia)  
[CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)  
[Heijden.Dns.Portable](https://github.com/softlion/Heijden.Dns)  
[HtmlAgilityPack](https://html-agility-pack.net/)  
[Jint](https://github.com/sebastienros/jint)
[NAudio](https://github.com/naudio/NAudio)  
[Newtonsoft.Json](https://www.newtonsoft.com/json)  
[OpenTK.OpenAL](https://opentk.net/)  
[SharpZipLib](https://github.com/icsharpcode/SharpZipLib)  
[Tomlyn](https://github.com/xoofx/Tomlyn)  
[ForgeWrapper](https://github.com/ZekerZhayard/ForgeWrapper)  
[Live2DCSharpSDK](https://github.com/coloryr/Live2DCSharpSDK)  
[OptifineWrapper](https://github.com/coloryr/OptifineWrapper) 

## License
Apache 2.0  
MIT  
BSD

## Tools and IDE
[Visual Studio Code](https://code.visualstudio.com/)  
[Visual Studio 2022](https://visualstudio.microsoft.com/)  
![dotMemory logo](https://resources.jetbrains.com/storage/products/company/brand/logos/dotMemory_icon.svg)