# ColorMC 全新的MC启动器

交流QQ群：571239090

[English](./README_EN.md)

使用dotnet8作为运行环境，XAML作为前端语言，C#作为后端语言

[用户手册](https://github.com/Coloryr/ColorMC_Pic/blob/master/guide/Main.md)

![](/image/run.png)  

**演示动画**

![](/image/GIF.gif)  

## 支持平台
- Linux
- Windows
- macOs

注意：ARM64平台不能保证其兼容性  
Windows ARM64 可以运行，渲染有问题  
Linux ARM64 在`xx派`上可以运行，运行缓慢  
Mac ARM64 不能运行，可以运行x64版本  

Linux由于发行版过于复杂，每个人的电脑兼容性都不一样，如果打不开可以需要自行解决

## 安装 
在Releases或者Actions里面下载构建好的压缩包/安装包  
解压(zip)\安装(exe,deb,pkg)\或直接运行(appimage)即可

Windows下，可以使用winget安装（现在应该还没好）
```
winget install colormc
```
默认安装在`D:\ColorMC`

## 启动

- 安装完成后启动  
在windows/macos下解压后双击启动  
linux下可以双击启动，也可以
```
ColorMC.Launcher
```

- 从源码启动（需要安装.net8 sdk）
```
git clone https://github.com/Coloryr/ColorMC.git
cd ColorMC/src/ColorMC.Launcher
dotnet run
```

## 从源码构建

- 构建`windows`, `ubuntu`, `macos`的二进制文件  
**需要在Ubuntu系统中构建，并安装git与dotnet-8-sdk**
```
git clone https://github.com/Coloryr/ColorMC.git
cd ColorMC
chmod a+x ./build/update.sh
chmod a+x ./build/build-macos.sh
chmod a+x ./build/build-ubuntu.sh
chmod a+x ./build/build-windows.sh

# 更新源码
./build/update.sh

# 构建
./build/build-windows.sh
./build/build-macos.sh
./build/build-ubuntu.sh
```

- 构建`arch`的二进制文件  
**需要在Arch系统中构建，并安装git与dotnet-8-sdk**
```
git clone https://github.com/Coloryr/ColorMC.git
cd ColorMC
chmod a+x ./build/update.sh
chmod a+x ./build/build-arch.sh

# 更新源码
./build/update.sh

# 构建
./build/build-arch.sh
```

此时可以在`built_out`文件夹获取所有二进制文件

## 二次开发

```
git clone https://github.com/Coloryr/ColorMC.git
```

使用IDE打开`./src/ColorMC.sln`项目

## 项目说明
- ColorMC.Core 启动器底层核心
- ColorMC.Cmd CLI模式 (已放弃)
- ColorMC.Gui Gui模式
- ColorMC.Launcher 启动器
- ColorMC.Test 用于启动器核心测试

## 依赖/引用的项目
[AvaloniaUI](https://github.com/AvaloniaUI/Avalonia) 跨平台UI框架  
[CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) MVVM工具  
[Svg.Skia](https://github.com/wieslawsoltes/Svg.Skia) Svg图像显示  
[SkiaSharp](https://github.com/mono/SkiaSharp) Skia图像库
[Heijden.Dns.Portable](https://github.com/softlion/Heijden.Dns) DNS解析  
[HtmlAgilityPack](https://html-agility-pack.net/) HTML解析器  
[Jint](https://github.com/sebastienros/jint) JS解析执行器  
[NAudio](https://github.com/naudio/NAudio) Windows音频播放  
[Newtonsoft.Json](https://www.newtonsoft.com/json) JSON解析器  
[OpenTK.OpenAL](https://opentk.net/) openal音频  
[SharpZipLib](https://github.com/icsharpcode/SharpZipLib) 压缩包处理  
[Tomlyn](https://github.com/xoofx/Tomlyn) TOML解析器  
[ForgeWrapper](https://github.com/Coloryr/ForgeWrapper) Forge启动器  
[Live2DCSharpSDK](https://github.com/coloryr/Live2DCSharpSDK) Live2d渲染框架  
[OptifineWrapper](https://github.com/coloryr/OptifineWrapper) Optifine启动器  

## 开源协议
Apache 2.0  
MIT  
BSD

## 使用的IDE开发工具
[Visual Studio Code](https://code.visualstudio.com/)  
[Visual Studio 2022](https://visualstudio.microsoft.com/)  
![dotMemory logo](https://resources.jetbrains.com/storage/products/company/brand/logos/dotMemory_icon.svg)