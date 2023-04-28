using ColorMC.Core.LaunchPath;

namespace ColorMC.Cmd.Menus;

public static class JvmMenu
{
    private const string Title = "Jvm设置";
    private static List<string> Items;

    public static void Show()
    {
        var list = JvmPath.Jvms;
        Items = new List<string>();
        ConsoleUtils.Reset();
        ConsoleUtils.ShowTitle(Title);
        Items.Add("返回");
        Items.Add("添加");
        if (list.Count == 0)
        {
            ConsoleUtils.ShowTitle1("没有Jvm");
        }

        foreach (var item in list)
        {
            Items.Add("[" + item.Key + "|" + item.Value.Version + "]");
        }

        ConsoleUtils.SetItems(Items, Select);
    }

    private static void Select(int index)
    {
        switch (index)
        {
            case 0:
                MainMenu.Show();
                break;
            case 1:
                ConsoleUtils.Input("添加Jvm");
                var name = ConsoleUtils.ReadLine("Jvm名字");
                var path = ConsoleUtils.ReadLine("Jvm路径");
                ConsoleUtils.Info("正在检测...");
                var (Res, Msg) = JvmPath.AddItem(name, path);
                if (!Res)
                {
                    ConsoleUtils.Error(Msg);
                }
                else
                {
                    ConsoleUtils.Ok("已添加");
                }
                ConsoleUtils.Keep();
                Show();
                break;
            default:
                break;
        }
    }
}
