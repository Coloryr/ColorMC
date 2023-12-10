# ColorMC

[简体中文](./README.md)

A new Minecraft Launcher  

Notie: Translations from other languages are not yet ready. If possible, welcome to do the translation together
https://crowdin.com/project/colormc

![](/image/pic.png)

## Support Platform
- Linux
- Windows
- macOS

Due to the complexity of Linux distribution, everyone's compatibility is different. If it cannot be opened it, you can try modifying `/home/{user}/ColorMC/gui.json`

## Development environment

### Clone code

```
git clone https://github.com/Coloryr/ColorMC.git
cd ColorMC
```

### Insatll .NET 8

- Windows/macOS
[Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- Linux
[Doc](https://learn.microsoft.com/dotnet/core/install/linux)

```
# ubuntu
$ wget https://packages.microsoft.com/config/ubuntu/22.10/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
$ sudo dpkg -i packages-microsoft-prod.deb
$ rm packages-microsoft-prod.deb

# debian
$ wget https://packages.microsoft.com/config/debian/11/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
$ sudo dpkg -i packages-microsoft-prod.deb
$ rm packages-microsoft-prod.deb

$ sudo apt-get update
$ sudo apt-get install -y dotnet-sdk-8.0
```

### Launch

Goto project `ColorMC.Launcher` directory

```
$ dotnet build
```
```
$ dotnet run
```

## Project description
- ColorMC.Core : Core
- ColorMC.Cmd : CLI mode (deprecated)
- ColorMC.Gui : GUI mode
- ColorMC.Launcher : Launcher
- ColorMC.Test : For launcher core testing

## Skin View

![](/image/GIF1.gif)  

## Referenced

[AvaloniaUI](https://github.com/AvaloniaUI/Avalonia) Cross-platform UI framework  
[Heijden.Dns.Portable](https://github.com/softlion/Heijden.Dns) DNS resolution  
[HtmlAgilityPack](https://html-agility-pack.net/) HTML parser  
[Jint](https://github.com/sebastienros/jint) JS interpreter  
[Newtonsoft.Json](https://www.newtonsoft.com/json) JSON parser  
[SharpZipLib](https://github.com/icsharpcode/SharpZipLib) Compressed package processing  
[Tomlyn](https://github.com/xoofx/Tomlyn) TOML parser  
[OpenTK](https://opentk.net/) openal audio  
[SixLabors](https://sixlabors.com/) Image processing  
[CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) MVVM tools  
[NAudio](https://github.com/naudio/NAudio) Windows audio playback

## Use IDE

[Visual Studio Code](https://code.visualstudio.com/)  
[Visual Studio](https://visualstudio.microsoft.com/)
