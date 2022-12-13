using ColorMC.Core.Http.Download;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Utils;
using ColorMC.Core.Http.Downloader;
using ColorMC.Core;

namespace ColorMC.Cmd.Menus;

public static class AddGameMenu
{
    private const string Title = "创建实例";
    private static List<string> Items = new();
    private static GameSettingObj? Game;
    private static DownloadItem[] Items1;
    private static ProgressBar Bar;

    public static void Show()
    {
        ConsoleUtils.Reset();
        ConsoleUtils.ShowTitle(Title);
        ConsoleUtils.ShowTitle1("选择游戏版本");

        Items.Clear();
        foreach (var item in VersionPath.Versions.versions)
        {
            Items.Add(item.id);
        }

        ConsoleUtils.SetItems(Items, Select);
    }

    private static void Select(int index)
    {
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

        ConsoleUtils.Info1("正在检查mod加载器支持");
        Game = new()
        {
            Name = name,
            Version = item
        };
        Items = ForgeHelper.GetSupportVersion().Result;
        if (Items?.Contains(item) == true)
        {
            var loader = ConsoleUtils.YesNo("启用forge");
            if (loader)
            {
                ConsoleUtils.Info1("正在获取forge版本");
                Items = ForgeHelper.GetForgeList(item).Result;
                if (Items == null)
                {
                    ConsoleUtils.Error("Forge列表获取失败");
                    ConsoleUtils.Keep();
                    Show();
                    return;
                }
                ConsoleUtils.Reset();
                ConsoleUtils.ShowTitle("创建实例");
                ConsoleUtils.ShowTitle1($"选择forge版本");
                ConsoleUtils.SetItems(Items, ForgeInstall);
                return;
            }
        }

        Install();
    }

    private static void Install()
    {
        ConsoleUtils.Reset();
        ConsoleUtils.ShowTitle("创建实例");
        ConsoleUtils.ShowTitle1($"安装游戏:{Game.Name}");

        ConsoleUtils.Info("正在创建实例");

        Game = InstancesPath.CreateVersion(Game!.Name, Game.Version, false, Loaders.Normal, null);

        if (Game == null)
        {
            ConsoleUtils.Error("创建实例错误");
            ConsoleUtils.Keep();
            Show();
            return;
        }

        ConsoleUtils.Ok("创建实例完成");

        ConsoleUtils.Info("正在安装游戏");

        var items = new List<DownloadItem>();

        var list = GameDownload.Download(VersionPath.Versions.versions.Where(a => a.id == Game.Version).First()).Result;

        if (list.State != DownloadState.End)
        {
            ConsoleUtils.Error("获取游戏安装信息失败");
            ConsoleUtils.Error(list.State.GetName());
            ConsoleUtils.Keep();
            MainMenu.Show();
            return;
        }

        items.AddRange(list.List);

        CoreMain.DownloadItemStateUpdate = DownloadUpdate;
        CoreMain.DownloaderUpdate = DownloaderUpdate;

        DownloadManager.FillAll(items);
        var download = DownloadManager.Start().Result;
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
        var forge = Items[index];

        ConsoleUtils.Reset();
        ConsoleUtils.ShowTitle("创建实例");
        ConsoleUtils.ShowTitle1($"安装游戏:{Game.Name}");

        ConsoleUtils.Info("正在创建实例");

        Game = InstancesPath.CreateVersion(Game!.Name, Game.Version, false, Loaders.Forge, new LoaderInfoObj()
        {
            Name = "forge",
            Version = forge
        });

        if (Game == null)
        {
            ConsoleUtils.Error("创建实例错误");
            ConsoleUtils.Keep();
            Show();
            return;
        }

        ConsoleUtils.Ok("创建实例完成");

        ConsoleUtils.Info("正在安装游戏");

        var items = new List<DownloadItem>();

        //var list = GameDownload.Download(VersionPath.Versions.versions.Where(a => a.id == Game.Version).First()).Result;

        //if (list.State != DownloadState.End)
        //{
        //    ConsoleUtils.Error("获取游戏安装信息失败");
        //    ConsoleUtils.Error(list.State.GetName());
        //    ConsoleUtils.Keep();
        //    MainMenu.Show();
        //    return;
        //}

        //items.AddRange(list.List);

        var list = GameDownload.DownloadForge(Game).Result;
        if (list.State != DownloadState.End)
        {
            ConsoleUtils.Error("获取mod加载器信息失败");
            ConsoleUtils.Error(list.State.GetName());
            ConsoleUtils.Keep();
            MainMenu.Show();
            return;
        }

        items.AddRange(list.List);

        CoreMain.DownloadItemStateUpdate = DownloadUpdate;
        CoreMain.DownloaderUpdate = DownloaderUpdate;

        DownloadManager.FillAll(items);
        var download = DownloadManager.Start().Result;
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

    public static void DownloaderUpdate(CoreRunState state)
    {
        if (state == CoreRunState.Start)
        {
            ConsoleUtils.Info("开始下载文件");
            Console.ForegroundColor = ConsoleColor.White;
            Items1 = new DownloadItem[ConfigUtils.Config.Http.DownloadThread];
            Bar = new ProgressBar(ConfigUtils.Config.Http.DownloadThread);
        }
        else
        {
            Bar.Dispose();
        }
    }

    public static void DownloadUpdate(int index, DownloadItem item)
    {
        if (item.State == DownloadItemState.Done)
        {
            Items1[index] = null;
            Bar.Done(index, $"{item.Name} 下载完成");
        }
        else if (item.State != DownloadItemState.Init)
        {
            Items1[index] = item;
            Bar.SetName(index, item.Name);
            Bar.SetAllSize(index, item.AllSize);
            Bar.SetValue(index, item.NowSize);
        }
    }
}
