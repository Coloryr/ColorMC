using ColorMC.Core.LaunchPath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Cmd.Menus;

public static class MainMenu
{
    private static string Title = "主菜单";
    private static string[] Items = new string[] { "启动游戏", "创建实例", "实例管理", "账户管理", "Jvm设置", "启动器设置" };

    public static void Show()
    {
        ConsoleUtils.Reset();
        ConsoleUtils.ShowTitle(Title);
        ConsoleUtils.ShowItems(Items, Select);
    }

    private static void Select(int index)
    {
        switch (index)
        {
            case 0:
                LaunchMenu.Show();
                break;
            case 1:
                
                break;
            case 2:
                break;
            case 3:

                break;
            case 4:
                JvmMenu.Show();
                break;
        }
    }
}
