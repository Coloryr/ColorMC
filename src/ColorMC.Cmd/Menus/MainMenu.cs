namespace ColorMC.Cmd.Menus;

public static class MainMenu
{
    private const string Title = "主菜单";
    private static List<string> Items = new()
    {
        "启动游戏",
        "账户管理",
        "创建实例",
        "实例管理",
        "Jvm设置",
        "启动器设置"
    };

    public static void Show()
    {
        ConsoleUtils.Reset();
        ConsoleUtils.ShowTitle(Title);
        ConsoleUtils.SetItems(Items, Select);
    }

    private static void Select(int index)
    {
        switch (index)
        {
            case 0:
                LaunchMenu.Show();
                break;
            case 1:
                UserMenu.Show();
                break;
            case 2:
                AddGameMenu.Show();
                break;
            case 3:

                break;
            case 4:
                JvmMenu.Show();
                break;
        }
    }
}
