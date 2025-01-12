# <img src="docs/images/icon.png" alt="icône" width="24" height="24"> ColorMC
![](https://img.shields.io/badge/license-Apache2.0-green)
![](https://img.shields.io/github/repo-size/Coloryr/ColorMC)
![](https://img.shields.io/github/stars/Coloryr/ColorMC)
![](https://img.shields.io/github/contributors/Coloryr/ColorMC)
![](https://img.shields.io/github/commit-activity/y/Coloryr/ColorMC)

**Avertissement : le contenu suivant est traduit par ChatGPT**.

Un lanceur Minecraft PC multiplateforme.

Utilise .NET8 comme environnement d'exécution, XAML comme langage frontend, et C# comme langage backend.

Groupe QQ: 571239090

Plus de langues:
[简体中文](README_EN.md)
[English](README_EN.md)
[Svenska](README_SW.md)
[Русский](README_RU.md)

[Manuel de l'utilisateur](https://github.com/Coloryr/ColorMC_Pic/blob/master/guide/Main.md) -  
[Changelog](log.md) -  
[Rejoignez la traduction](https://crowdin.com/project/colormc) (Aidez-nous!)

## Captures d'écran 🪟
![](docs/images/run.png)

**Vidéo de démonstration**

![](docs/images/GIF.gif)

## Plateformes supportées
- Linux (fournit deb, pkg, rpm)
- Windows
- macOS

**Note : La compatibilité avec la plateforme ARM64 ne peut pas être garantie.  
En raison de la complexité des distributions Linux, la compatibilité peut varier. Si cela ne fonctionne pas, vous devrez peut-être résoudre le problème vous-même. J'ai testé le lanceur uniquement sur ma propre machine virtuelle. Les problèmes de compatibilité des pilotes ne sont pas dans le cadre de mon support.**

## Installation
Téléchargez les fichiers compressés/installateurs préconstruits depuis [Releases](https://github.com/Coloryr/ColorMC/releases) ou [Actions](https://github.com/Coloryr/ColorMC/actions).  
Extrayez (zip)/installez (msi, deb, pkg)/ou exécutez directement (appimage).

Sur Windows, vous pouvez utiliser winget pour installer :
```
winget install colormc
```
Le chemin d'installation par défaut est `C:\Program Files\ColorMC`.

## Lancement

- Après l'installation :  
  Sous Windows/macOS, double-cliquez sur le fichier extrait pour lancer.  
  Sous Linux, double-cliquez pour lancer, ou utilisez :
```
ColorMC.Launcher
```

- Lancer à partir du code source (nécessite .NET8 SDK) :
```
git clone https://github.com/Coloryr/ColorMC.git
cd ColorMC/src/ColorMC.Launcher
dotnet run
```

## Construction à partir du code source

### Construire le binaire Windows
**Doit être construit sur Windows avec git et dotnet-8-sdk installés.**

```cmd
git clone https://github.com/Coloryr/ColorMC.git
cd ColorMC

@REM Mettre à jour le code source
.\build\update.cmd

@REM Construire le binaire
.\build\build-windows.cmd
```

### Construire le binaire Linux
**Doit être construit sur Linux avec git et dotnet-8-sdk installés.**
```bash
git clone https://github.com/Coloryr/ColorMC.git
cd ColorMC
chmod a+x ./build/update.sh
chmod a+x ./build/build-linux.sh

# Mettre à jour le code source
./build/update.sh

# Construire le binaire
./build/build-linux.sh
```

#### Emballage des images
**Pour construire les images Ubuntu, rpm et Arch, utilisez les scripts appropriés pour chaque plateforme.**

### Construire le binaire macOS
**Doit être construit sur Ubuntu ou macOS avec git et dotnet-8-sdk installés.**
```bash
git clone https://github.com/Coloryr/ColorMC.git
cd ColorMC
chmod a+x ./build/update.sh
chmod a+x ./build/build-macos.sh

# Mettre à jour le code source
./build/update.sh

# Construire le binaire
./build/build-macos.sh
```

Après la construction, tous les binaires se trouveront dans le dossier `built_out`.

## Développement

Clonez le dépôt :
```
git clone https://github.com/Coloryr/ColorMC.git
git submodule update --init --recursive
```

Le fichier de solution principal est `./src/ColorMC.sln`.

### Utilisation du noyau ColorMC
[Comment développer votre propre lanceur avec le noyau ColorMC](docs/Core.md)

### Description du projet
| Module              | Description                             |
|---------------------|-----------------------------------------|
| ColorMC.Core        | Noyau du lanceur                       |
| ColorMC.CustomGui   | Interface personnalisée du lanceur [Tutoriel](docs/CustomGui.md) |
| ColorMC.Cmd         | Mode ligne de commande (obsolète)      |
| ColorMC.Gui         | Mode GUI                               |
| ColorMC.Launcher    | Application principale du lanceur      |
| ColorMC.Test        | Pour tester le lanceur                 |
| ColorMC.Setup       | Pour construire le msi pour Windows    |

## Dépendances / Projets référencés
Le projet utilise plusieurs dépendances, notamment des frameworks UI (AvaloniaUI), des bibliothèques de boîtes de dialogue (DialogHost.Avalonia), etc. Pour plus de détails, consultez le fichier Markdown original.

## Licence Open Source
Ce projet est sous licence Apache 2.0. Pour les termes détaillés, consultez la licence à la fin de ce fichier.

### Outils de développement
Outils recommandés :
- Visual Studio Code
- Visual Studio 2022
- dotMemory