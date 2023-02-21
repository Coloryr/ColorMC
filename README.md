# ColorMC

## 支持平台
- Linux
- Windows
- macOs

**需要安装.NET 7环境**

Linux由于发行版过于复杂，每个人的电脑兼容性都不一样，如果打不开可以尝试修改`/usr/share/ColorMC/gui.json`

## 安装/启动

### 安装.Net7

- Windows/macOs
[下载](https://dotnet.microsoft.com/zh-cn/download/dotnet/7.0)里面的SDK安装包安装即可
- Linux
[教程](https://learn.microsoft.com/zh-cn/dotnet/core/install/linux?WT.mc_id=dotnet-35129-website)

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
$ sudo apt-get install -y dotnet-sdk-7.0
```

### 启动

解压压缩包
- Windows
双击`ColorMC.Gui.exe`即可
或
```
$ dotnet ColorMC.Gui.dll
```
- macOs  
打开一个终端
```
$ ./ColorMC.Gui
```
或
```
$ dotnet ColorMC.Gui.dll
```
- Linux
双击`ColorMC.Gui`即可
或
```
$ dotnet ColorMC.Gui.dll
```

## 项目
- ColorMC.Core 启动器底层核心
- ColorMC.Cmd CLI模式 (已放弃)
- ColorMC.Gui Gui模式
- ColorMC.Test 用于启动器核心测试

## 皮肤预览

![](/image/GIF1.gif)  

## 引用的项目

[AvaloniaUI](https://github.com/AvaloniaUI/Avalonia) 跨平台UI框架  
[Heijden.Dns.Portable]() DNS解析  
[HtmlAgilityPack](https://html-agility-pack.net/) HTML解析器  
[Jint](https://github.com/sebastienros/jint) JS解析执行器  
[MiNET.fnbt]() NBT解析器  
[Newtonsoft.Json](https://www.newtonsoft.com/json) JSON解析器  
[SharpZipLib](https://github.com/icsharpcode/SharpZipLib) 压缩包处理  
[Tomlyn](https://github.com/xoofx/Tomlyn) TOML解析器  
[OpenTK](https://opentk.net/) Opengl渲染  
[ReactiveUI](https://github.com/reactiveui/ReactiveUI) MVVM 框架  
[SixLabors](https://sixlabors.com/) 图片处理

## 使用的IDE

[Visual Studio Code](https://code.visualstudio.com/)

[Visual Studio](https://visualstudio.microsoft.com/)

![](/Img/pic.png)