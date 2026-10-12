# <img src="docs/images/icon.png" alt="icon" width="24" height="24"> ColorMC
![](https://img.shields.io/badge/license-Apache2.0-green)
![](https://img.shields.io/github/repo-size/Coloryr/ColorMC)
![](https://img.shields.io/github/stars/Coloryr/ColorMC)
![](https://img.shields.io/github/contributors/Coloryr/ColorMC)
![](https://img.shields.io/github/commit-activity/y/Coloryr/ColorMC)

**本项目停止添加新功能，新功能将会在迭代产品[ModernMinecraftLauncher](https://github.com/Coloryr/ModernMinecraftLauncher)中添加**

一个全平台Minecraft PC启动器

使用.NET10作为运行环境，XAML作为前端语言，使用MVVM模式，C#作为后端语言

QQ交流群: 571239090

More Languages: [English](README_EN.md)

[用户手册](https://github.com/Coloryr/ColorMC_Pic/blob/master/guide/README.md) -
[更新日志](log.md) -
[加入多语言翻译](https://crowdin.com/project/colormc)(帮帮忙)

## 窗口截图 🪟
![](/docs/images/image.png)  

## 支持平台
- Windows(zip)
- Linux(提供deb pkg rpm，同时可以在[星火商店](https://www.spark-app.store/)或者[AUR](https://aur.archlinux.org/)上安装)
- macOs(zip dmg)

**注意：ARM64平台不能保证其兼容性  
由于Linux发行版过于复杂，每个人的电脑兼容性都不一样，如果打不开可以需要自行解决，我只在自己的虚拟机内测试启动，若有驱动兼容性问题不在我的考虑范围内**

## 安装 
在[Releases-正式发布版](https://github.com/Coloryr/ColorMC/releases)或者[Actions-测试版](https://github.com/Coloryr/ColorMC/actions)里面下载构建好的压缩包/安装包  
解压(zip)\安装(msi,deb,pkg)\或直接运行(appimage)即可

## 启动

- 安装完成后启动  
在Windows/MacOS下解压后双击启动  
Linux下安装后可以双击启动，也可以控制台输入指令
```
$ ColorMC.Launcher
```

- 从源码启动（需要安装.NET10 SDK）
```
$ git clone https://github.com/Coloryr/ColorMC.git
$ cd ColorMC
$ git submodule update --init --recursive
$ cd src/ColorMC.Launcher
$ dotnet run
```

## 从源码构建

你可以从源码构建ColorMC并运行  
构建完成后可以在`built_out`文件夹获取所有二进制文件

### 构建`windows`的二进制文件  
**需要在Windows系统中构建，并安装git与dotnet-10-sdk**

```
git clone https://github.com/Coloryr/ColorMC.git
cd ColorMC

@REM 更新源码
.\build\update.cmd

@REM 构建
.\build\build-windows.cmd
```

### 构建`linux`的二进制文件  
**需要在Linux系统中构建，并安装git与dotnet-10-sdk**
```
$ git clone https://github.com/Coloryr/ColorMC.git
$ cd ColorMC
$ chmod a+x ./build/update.sh
$ chmod a+x ./build/build-linux.sh
```
更新源码
```
$ ./build/update.sh
```
构建
```
$ ./build/build-linux.sh
```

### 打包Linux相关安装镜像

- 打包Ubuntu镜像  
**需要在Ubuntu系统中操作**
```
$ chmod a+x ./build/build-ubuntu.sh
$ ./build/build-ubuntu.sh
```
- 打包rpm镜像  
**需要在Ubuntu系统中操作**
```
$ chmod a+x ./build/build-rpm.sh
$ ./build/build-rpm.sh
```
- 打包Arch镜像  
**需要在Arch系统中操作**
```
$ chmod a+x ./build/build-arch.sh
$ ./build/build-arch.sh
```

### 构建`macos`的二进制文件  
**需要在MacOS系统中构建，并安装git与dotnet-10-sdk**
```
$ git clone https://github.com/Coloryr/ColorMC.git
$ cd ColorMC
$ chmod a+x ./build/update.sh
$ chmod a+x ./build/build-macos.sh
```
更新源码
```
$ ./build/update.sh
```
构建
```
$ ./build/build-macos.sh
```
- 打包Dmg镜像
**需要在MacOS系统中操作**
```
$ ./build/build-dmg.sh
```

## 二次开发

首先克隆代码
```
$ git clone https://github.com/Coloryr/ColorMC.git
$ git submodule update --init --recursive
```

`./src/ColorMC.sln`为根工程

### 使用ColorMC启动器核心

[使用ColorMC启动器核心来开发自己的启动器](docs/Core.md)

### 项目说明
| 模块                | 说明                               |
|-------------------|----------------------------------|
| ColorMC.Core      | 启动器核心                            |
| ColorMC.CustomGui | 自定义启动器界面 [教程](docs/CustomGui.md) |
| ColorMC.Cmd       | 命令行模式 (已弃用)                      |
| ColorMC.Gui       | Gui模式                            |
| ColorMC.Launcher  | 启动器本体                            |
| ColorMC.Test      | 用于启动器测试                          |
| ColorMC.Setup.Wix | 用于构建windows的msi安装包               |

## 依赖/引用的项目
| 名称                    | 描述              | 链接                                                             |
|-----------------------|-----------------|----------------------------------------------------------------|
| AvaloniaUI            | 跨平台UI框架         | [GitHub](https://github.com/AvaloniaUI/Avalonia)               |
| Ae.Dns                | DNS客户端           | [GitHub](https://github.com/alanedwardes/Ae.Dns)  |
| HtmlAgilityPack       | HTML解析器         | [GitHub](https://github.com/zzzprojects/html-agility-pack)               |
| Jint                  | JS解析执行器         | [GitHub](https://github.com/sebastienros/jint)                 |
| DialogHost.Avalonia   | 弹窗库             | [GitHub](https://github.com/AvaloniaUtils/DialogHost.Avalonia) |
| CommunityToolkit.Mvvm | MVVM工具          | [GitHub](https://github.com/CommunityToolkit/dotnet)           |
| Svg.Skia              | Svg图像显示         | [GitHub](https://github.com/wieslawsoltes/Svg.Skia)            |
| SkiaSharp             | Skia图像库         | [GitHub](https://github.com/mono/SkiaSharp)                    |
| Silk.NET              | 高性能底层库接口        | [GitHub](https://github.com/dotnet/Silk.NET)                   |              |
| DotNetty              | 异步通信框架          | [GitHub](https://github.com/Azure/DotNetty)                    |
| Tomlyn                | TOML解析器         | [GitHub](https://github.com/xoofx/Tomlyn)                      |
| ForgeWrapper          | Forge启动器        | [GitHub](https://github.com/PrismLauncher/ForgeWrapper)              |      |
| OptifineWrapper       | Optifine启动器     | [GitHub](https://github.com/coloryr/OptifineWrapper)           |
| ColorMCASM            | 用于ColorMC与游戏内通信 | [GitHub](https://github.com/Coloryr/ColorMCASM)                |
| K4os.Compression.LZ4  | LZ4解压缩           | [GitHub](https://github.com/MiloszKrajewski/K4os.Compression.LZ4)  |
| sharpcompress         | 压缩包解压处理   | [GitHub](https://github.com/adamhathcock/sharpcompress)                |
| Markdig               | MarkDown处理工具   | [GitHub](https://github.com/xoofx/markdig)                |
| MinecraftSkinRender   | Minecraft皮肤渲染器   | [GitHub](https://github.com/Coloryr/MinecraftSkinRender)                |

## 开源协议
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

附属的开源协议: MIT, BSD

## 使用的IDE开发工具
- [Visual Studio Code](https://code.visualstudio.com/)  
- [Visual Studio 2022](https://visualstudio.microsoft.com/)  
- ![dotMemory logo](https://resources.jetbrains.com/storage/products/company/brand/logos/dotMemory_icon.svg)
