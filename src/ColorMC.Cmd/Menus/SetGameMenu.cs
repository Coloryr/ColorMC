using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;

namespace ColorMC.Cmd.Menus;

public static class SetGameMenu
{
    private const string Title = "编辑实例";
    private static List<string> Items = new();
    private static GameSettingObj? Game;
    private static DownloadItem[] Items1;

    public static void Show()
    {
        ConsoleUtils.Reset();
        ConsoleUtils.ShowTitle(Title);
        ConsoleUtils.ShowTitle1("选择实例");

        Items.Clear();
        Items.Add("返回");
        foreach (var item in InstancesPath.Games)
        {
            Items.Add(item.Name);
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


    }
}
