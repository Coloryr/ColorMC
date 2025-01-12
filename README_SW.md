# <img src="docs/images/icon.png" alt="ikon" width="24" height="24"> ColorMC
![](https://img.shields.io/badge/license-Apache2.0-green)
![](https://img.shields.io/github/repo-size/Coloryr/ColorMC)
![](https://img.shields.io/github/stars/Coloryr/ColorMC)
![](https://img.shields.io/github/contributors/Coloryr/ColorMC)
![](https://img.shields.io/github/commit-activity/y/Coloryr/ColorMC)

**Varning: följande innehåll är översatt av ChatGPT**

En plattformsoberoende Minecraft PC-launcher.

Byggd med .NET8 som runtime-miljö, XAML som frontend-språk och C# som backend-språk.

QQ-grupp: 571239090

Fler språk:
[简体中文](README_EN.md)
[English](README_EN.md)
[Русский](README_RU.md)
[Français](README_FR.md)

[Användarmanual](https://github.com/Coloryr/ColorMC_Pic/blob/master/guide/Main.md) -  
[Changelog](log.md) -  
[Gå med i översättningsarbetet](https://crowdin.com/project/colormc) (Hjälp oss!)

## Fönsterskärmdumpar 🪟
![](docs/images/run.png)

**Demovideo**

![](docs/images/GIF.gif)

## Stödda plattformar
- Linux (tillhandahåller deb, pkg, rpm)
- Windows
- macOS

**Not: ARM64-plattformens kompatibilitet kan inte garanteras.  
På grund av Linux-distributionernas komplexitet kan kompatibiliteten variera mellan enheter. Om det inte fungerar kan du behöva felsöka det själv. Jag har endast testat startaren på min egen virtuella maskin. Drivrutinskompatibilitetsproblem ligger utanför mitt supportområde.**

## Installation
Ladda ner de förbyggda komprimerade filerna/installatörerna från [Releases](https://github.com/Coloryr/ColorMC/releases) eller [Actions](https://github.com/Coloryr/ColorMC/actions).  
Extrahera (zip)/Installera (msi, deb, pkg)/eller kör direkt (appimage).

På Windows kan du använda winget för att installera:
```
winget install colormc
```
Standardinstallationsvägen är `C:\Program Files\ColorMC`.

## Starta

- Efter installation:  
  På Windows/macOS, dubbelklicka på den extraherade filen för att köra.  
  På Linux, dubbelklicka för att köra, eller använd:
```
ColorMC.Launcher
```

- Kör från källkod (kräver .NET8 SDK):
```
git clone https://github.com/Coloryr/ColorMC.git
cd ColorMC/src/ColorMC.Launcher
dotnet run
```

## Bygg från källkod

### Bygg Windows-binärfil
**Måste byggas på Windows med git och dotnet-8-sdk installerade.**

```cmd
git clone https://github.com/Coloryr/ColorMC.git
cd ColorMC

@REM Uppdatera källkod
.\build\update.cmd

@REM Bygg binärfil
.\build\build-windows.cmd
```

### Bygg Linux-binärfil
**Måste byggas på Linux med git och dotnet-8-sdk installerade.**
```bash
git clone https://github.com/Coloryr/ColorMC.git
cd ColorMC
chmod a+x ./build/update.sh
chmod a+x ./build/build-linux.sh

# Uppdatera källkod
./build/update.sh

# Bygg binärfil
./build/build-linux.sh
```

#### Packa bilder
**För att bygga Ubuntu, rpm och Arch-images, kör respektive skript på lämplig plattform.**

### Bygg macOS-binärfil
**Måste byggas på Ubuntu eller macOS med git och dotnet-8-sdk installerade.**
```bash
git clone https://github.com/Coloryr/ColorMC.git
cd ColorMC
chmod a+x ./build/update.sh
chmod a+x ./build/build-macos.sh

# Uppdatera källkod
./build/update.sh

# Bygg binärfil
./build/build-macos.sh
```

Efter att ha byggt, kommer alla binärer att finnas i `built_out`-mappen.

## Utveckling

Klonad kod:
```
git clone https://github.com/Coloryr/ColorMC.git
git submodule update --init --recursive
```

Huvudlösningen är `./src/ColorMC.sln`.

### Använd ColorMC Launcher Core
[Hur man utvecklar en egen launcher med ColorMC Core](docs/Core.md)

### Projektmoduler
| Modul              | Beskrivning                             |
|--------------------|-----------------------------------------|
| ColorMC.Core       | Launcher Core                          |
| ColorMC.CustomGui  | Anpassad launcher UI [Tutorial](docs/CustomGui.md) |
| ColorMC.Cmd        | Kommandoradsläge (deprecated)          |
| ColorMC.Gui        | GUI-läge                               |
| ColorMC.Launcher   | Huvudapplikation för startaren         |
| ColorMC.Test       | För tester av startaren                |
| ColorMC.Setup      | För att bygga Windows msi-installatör  |

## Beroenden / Refererade Projekt
Projektet använder flera beroenden, inklusive UI-ramverk (AvaloniaUI), dialogbibliotek (DialogHost.Avalonia) och fler. Fullständiga detaljer finns i den ursprungliga Markdown-filen.

## Open Source License
Detta projekt är licensierat under Apache 2.0. För detaljerade villkor, se licensen i slutet av denna fil.

### Utvecklingsverktyg
Rekommenderade verktyg:
- Visual Studio Code
- Visual Studio 2022
- dotMemory