# ColorMC

![](/image/icon.ico)

一个全新的全平台启动器
外观基于[NsisoLauncher](https://github.com/Coloryr/NsisoLauncher-1)

**当前为A测版**

## 支持平台
- Linux
- Windows
- macOs

**需要安装.NET 7环境**

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


## 功能
- 基础启动器功能
- 整合包/Mod 搜索下载
- 导入/导出 MMC/HMCL压缩包 (未完成)
- 全自定义UI (未完成)
- 服务器资源同步 (未完成)
- 多实例/多账户 启动

## 项目
- ColorMC.Core 启动器底层核心
- ColorMC.Cmd CLI模式
- ColorMC.Gui Gui模式
- ColorMC.Test 用于启动器核心测试

## 界面

![](/image/pic1.png)  
![](/image/pic2.png)
![](/image/pic3.png)
![](/image/pic4.png)
![](/image/pic5.png)
![](/image/pic6.png)
