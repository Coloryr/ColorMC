using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Net.Download;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;

namespace ColorMC.Cmd.Menus;

public static class AddGameMenu
{
    private const string Title = "创建实例";
    private static List<string>? Items = new();
    private static GameSettingObj? Game;

    public static void Show()
    {
        ConsoleUtils.Reset();
        ConsoleUtils.ShowTitle(Title);
        ConsoleUtils.ShowTitle1("选择游戏版本");

        if (VersionPath.Versions == null)
        {
            ConsoleUtils.Error("没有版本信息，重启尝试");
            ConsoleUtils.Keep();
            Show();
            return;
        }

        Items.Clear();
        Items.Add("返回");
        foreach (var item in VersionPath.Versions.versions)
        {
            Items.Add(item.id);
        }

        ConsoleUtils.SetItems(Items, Select);
    }

    private static void Select(int index)
    {
        if (index == 0)
        {
            MainMenu.Show();
            return;
        }
        var item = Items[index];
        ConsoleUtils.Reset();
        ConsoleUtils.ShowTitle(Title);
        ConsoleUtils.ShowTitle1($"游戏版本:{item}");

        var name = ConsoleUtils.Edit("实例名字", item);
        if (string.IsNullOrWhiteSpace(name))
        {
            ConsoleUtils.Error("请输入实例名字");
            ConsoleUtils.Keep();
            Show();
        }

        ConsoleUtils.Info1("正在检查Mod加载器支持");
        Game = new()
        {
            Name = name,
            Version = item
        };
        Items = ForgeAPI.GetSupportVersion(BaseClient.Source).Result;
        if (Items?.Contains(item) == true)
        {
            var loader = ConsoleUtils.YesNo("启用Forge");
            if (loader)
            {
                ConsoleUtils.Info1("正在获取Forge版本");
                Items = ForgeAPI.GetVersionList(item, BaseClient.Source).Result;
                if (Items == null)
                {
                    ConsoleUtils.Error("Forge列表获取失败");
                    ConsoleUtils.Keep();
                    Show();
                    return;
                }
                ConsoleUtils.Reset();
                ConsoleUtils.ShowTitle("创建实例");
                ConsoleUtils.ShowTitle1($"选择Forge版本");
                ConsoleUtils.SetItems(Items, ForgeInstall);
                return;
            }
        }

        Items = FabricAPI.GetSupportVersion().Result;
        if (Items?.Contains(item) == true)
        {
            var loader = ConsoleUtils.YesNo("启用Fabric");
            if (loader)
            {
                ConsoleUtils.Info1("正在获取Fabric版本");
                Items = FabricAPI.GetLoaders(item).Result;
                if (Items == null)
                {
                    ConsoleUtils.Error("Fabric列表获取失败");
                    ConsoleUtils.Keep();
                    Show();
                    return;
                }
                ConsoleUtils.Reset();
                ConsoleUtils.ShowTitle("创建实例");
                ConsoleUtils.ShowTitle1($"选择Fabric版本");
                ConsoleUtils.SetItems(Items, FabricInstall);
                return;
            }
        }

        Install();
    }

    private static async void Install()
    {
        ConsoleUtils.Reset();
        ConsoleUtils.ShowTitle("创建实例");
        ConsoleUtils.ShowTitle1($"安装游戏:{Game.Name}");

        ConsoleUtils.Info("正在创建实例");

        Game.Loader = Loaders.Normal;
        Game.LoaderVersion = null;
        Game.ModPack = false;

        Game = await InstancesPath.CreateGame(Game);

        if (Game == null)
        {
            ConsoleUtils.Error("创建实例错误");
            ConsoleUtils.Keep();
            Show();
            return;
        }

        ConsoleUtils.Ok("创建实例完成");

        ConsoleUtils.Info("正在安装游戏");

        var items = new List<DownloadItemObj>();

        var list = GameDownload.Download(VersionPath.Versions!
            .versions.Where(a => a.id == Game.Version).First()).Result;

        if (list.State != GetDownloadState.End)
        {
            ConsoleUtils.Error("获取游戏安装信息失败");
            ConsoleUtils.Error(list.State.GetName());
            ConsoleUtils.Keep();
            MainMenu.Show();
            return;
        }

        items.AddRange(list.List!);

        var download = DownloadManager.Start(items).Result;
        if (!download)
        {
            ConsoleUtils.Error("游戏下载失败");
            ConsoleUtils.Error(list.State.GetName());
        }
        else
        {
            ConsoleUtils.Ok("游戏下载完成");
        }

        ConsoleUtils.Keep();
        MainMenu.Show();
    }

    private static void ForgeInstall(int index)
    {
        var fabric = Items[index];

        ConsoleUtils.Reset();
        ConsoleUtils.ShowTitle("创建实例");
        ConsoleUtils.ShowTitle1($"安装游戏:{Game.Name}");

        ConsoleUtils.Info("正在创建实例");

        Game.Loader = Loaders.Fabric;
        Game.LoaderVersion = fabric;
        Game.ModPack = false;

        if (Game == null)
        {
            ConsoleUtils.Error("创建实例错误");
            ConsoleUtils.Keep();
            Show();
            return;
        }

        ConsoleUtils.Ok("创建实例完成");

        ConsoleUtils.Info("正在安装游戏");

        var items = new List<DownloadItemObj>();

        var list = GameDownload.Download(VersionPath.Versions!
            .versions.Where(a => a.id == Game.Version).First()).Result;

        if (list.State != GetDownloadState.End)
        {
            ConsoleUtils.Error("获取游戏安装信息失败");
            ConsoleUtils.Error(list.State.GetName());
            ConsoleUtils.Keep();
            MainMenu.Show();
            return;
        }

        items.AddRange(list.List!);

        list = GameDownload.DownloadFabric(Game).Result;
        if (list.State != GetDownloadState.End)
        {
            ConsoleUtils.Error("获取mod加载器信息失败");
            ConsoleUtils.Error(list.State.GetName());
            ConsoleUtils.Keep();
            MainMenu.Show();
            return;
        }

        items.AddRange(list.List!);

        var download = DownloadManager.Start(items).Result;
        if (!download)
        {
            ConsoleUtils.Error("游戏下载失败");
            ConsoleUtils.Error(list.State.GetName());
        }
        else
        {
            ConsoleUtils.Ok("游戏下载完成");
        }

        ConsoleUtils.Keep();
        MainMenu.Show();
    }

    private static void FabricInstall(int index)
    {
        var forge = Items[index];

        ConsoleUtils.Reset();
        ConsoleUtils.ShowTitle("创建实例");
        ConsoleUtils.ShowTitle1($"安装游戏:{Game.Name}");

        ConsoleUtils.Info("正在创建实例");

        Game.Loader = Loaders.Forge;
        Game.LoaderVersion = forge;
        Game.ModPack = false;

        if (Game == null)
        {
            ConsoleUtils.Error("创建实例错误");
            ConsoleUtils.Keep();
            Show();
            return;
        }

        ConsoleUtils.Ok("创建实例完成");

        ConsoleUtils.Info("正在安装游戏");

        var items = new List<DownloadItemObj>();

        var list = GameDownload.Download(VersionPath.Versions!
            .versions.Where(a => a.id == Game.Version).First()).Result;

        if (list.State != GetDownloadState.End)
        {
            ConsoleUtils.Error("获取游戏安装信息失败");
            ConsoleUtils.Error(list.State.GetName());
            ConsoleUtils.Keep();
            MainMenu.Show();
            return;
        }

        items.AddRange(list.List!);

        list = GameDownload.DownloadFabric(Game).Result;
        if (list.State != GetDownloadState.End)
        {
            ConsoleUtils.Error("获取mod加载器信息失败");
            ConsoleUtils.Error(list.State.GetName());
            ConsoleUtils.Keep();
            MainMenu.Show();
            return;
        }

        items.AddRange(list.List!);

        var download = DownloadManager.Start(items).Result;
        if (!download)
        {
            ConsoleUtils.Error("游戏下载失败");
            ConsoleUtils.Error(list.State.GetName());
        }
        else
        {
            ConsoleUtils.Ok("游戏下载完成");
        }

        ConsoleUtils.Keep();
        MainMenu.Show();
    }
}
