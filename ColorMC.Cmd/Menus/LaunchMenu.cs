using ColorMC.Core.LaunchPath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Cmd.Menus;

public static class LaunchMenu
{
    private static string Title = "启动游戏";
    private static string Select1 = "选择实例";

    public static void Show()
    {
        var list = InstancesPath.Games;
        var items = new List<string>();
        ConsoleUtils.Reset();
        ConsoleUtils.ShowTitle(Title);
        items.Add("返回");
        if (list.Count == 0)
        {
            ConsoleUtils.ShowTitle1("没有实例");
        }
        else
        {
            ConsoleUtils.ShowTitle1(Select1);
            list.ForEach(item => items.Add("[" + item.Name + "|" + item.Version + "]"));
        }
        
        ConsoleUtils.ShowItems(items, Select);
    }

    private static void Select(int index)
    {
        switch (index)
        {
            case 0:
                MainMenu.Show();
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:

                break;
        }
    }
}
