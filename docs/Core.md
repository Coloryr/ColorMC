# ColorMC.Core

ColorMC启动器的核心 

使用dotnet8作为基础框架，不能完全支持AOT但是也可以用

## 支持功能

- 账户登录  
  - 支持 微软账户/统一通行证/外置登录/皮肤站 账户登录  
  - 自带账户管理
  - 不储存密码，只储存Token
  - 支持自动登录
- 游戏启动
  - 支持大部分游戏版本启动  
  - 支持模组加载器Forge/Fabric/NeoForge/Quilt/Optifine
  - 支持自定义模组加载器
  - 完整的游戏启动参数控制
- 游戏资源
  - 支持自动下载整合包 从 CurseForge/Modrinth 平台
  - 支持升级整合包 从 CurseForge/Modrinth 平台
  - 支持下载各类资源 模组/材质包/世界/数据包/光影包 从 CurseForge/Modrinth 平台
  - 支持模组升级 从 CurseForge/Modrinth 平台
  - 内置Mcmod搜索源中文搜索模组
- 游戏管理
  - 支持导入 CurseForge/Modrinth 平台整合包 MMC实例 Minecraft原版游戏版本
  - 支持导出 CurseForge/Modrinth 平台整合包 ColorMC整合包
  - 支持游戏实例 模组/世界/材质包/光影包/结构文件/服务器列表 解析
  - 支持各类资源导入与删除
  - 支持获取 游戏内截图列表/游戏日志
- 内置网络API
  - OAuth 账户登录API
  - Nide8 账户登录API
  - Littleskin 账户登录API
  - Adoptium/Dragonwell/OpenJ9/Zulu Java实例获取下载API
  - ColorMC Mcmod中文搜索API
  - CurseForge/Modrinth 搜索/下载资源API
  - Forge/NeoForge/Fabric/Quilt 获取版本API
  - BMCLAPI 游戏源
  - Mclo 日志上传API
  - Minecraft 皮肤获取API
- NBT标签读写
  - 支持NBT文件读写
  - 支持地图区块文件读写
  - 支持所有NBT标签类型
- 多线程异步下载器
  - 内置可设置下载线程数的下载器
  - 自动文件判断，校验
  - 异步方式下载阻塞
- 其他功能
  - Java压缩包解压校验
  - 游戏内时间统计
  - 各类实用函数
  - 压缩包处理
  - 多语言 目前只有中文和英文
  - 获取用户系统版本
  - 内置Minecraft工具下载链接

## 快速开始

首先新建一个VS项目

然后克隆项目
```
git clone https://github.com/Coloryr/ColorMC.git
```

导入项目
```
src/ColorMC.Core/ColorMC.Core.csproj
```

初始化启动器核心
```c#
using ColorMC.Core;

//初始化阶段1
ColorMCCore.Init(new()
{
    Local = "核心工作的路径",
    OAuthKey = "OAuth密钥",
    CurseForgeKey = "CurseForge密钥"
});
//初始化阶段2
ColorMCCore.Init1();
```

配置下载源
```c#
using ColorMC.Core.Net;
using ColorMC.Core.Objs;

//使用官方源
BaseClient.Source = SourceLocal.Offical;
//使用BMCLAPI
BaseClient.Source = SourceLocal.BMCLAPI;
```

登录账户
```c#
using ColorMC.Core.Game;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
//登录微软账户

void Code(string url, string code)
{
    //url 为登录网址
    //code为登录码
}

var res = await GameAuth.LoginOAuthAsync(Code);
if(res.LoginState != LoginState.Done)
{
    //登录失败
    Console.WriteLine(res.Message);
}
else
{
    //账户
    var auth = res.Auth!;
    //保存账户到数据库
    auth.Save();
}
```

启动游戏
```c#
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Utils;
using ColorMC.Core.Objs;

//创建游戏实例
var game = await InstancesPath.CreateGame(new()
{
    Game = new()
    {
        Name = "游戏实例",              //实例名字
        GroupName = "游戏分组",         //空为默认组
        Version = "1.21",              //游戏版本
        Loader = Loaders.NeoForge,     //模组加载器类型
        LoaderVersion = "21.0.16-beta" //模组加载器版本
    }
});

//上一步登录的账户
var auth = AuthDatabase.Auths.First().Value;
//启动游戏
var handel = await game.StartGameAsync(new()
{
    Auth = auth
});
```