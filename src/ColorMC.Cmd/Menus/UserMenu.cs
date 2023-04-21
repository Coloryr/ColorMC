using ColorMC.Core.Helpers;
using ColorMC.Core.Utils;

namespace ColorMC.Cmd.Menus;

public static class UserMenu
{
    private const string Title = "账户管理";
    private static List<string> Items;

    public static void Show()
    {
        var list = AuthDatabase.Auths;
        Items = new List<string>();
        ConsoleUtils.Reset();
        ConsoleUtils.ShowTitle(Title);
        Items.Add("返回");
        Items.Add("添加");
        if (list.Count == 0)
        {
            ConsoleUtils.ShowTitle1("没有账户");
        }

        foreach (var item in list)
        {
            Items.Add("[" + item.Key + "|" + item.Value.UserName + "|" + item.Value.AuthType.GetName() + "]");
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
                AddUserMenu.Show();
                break;
            default:
                var item = AuthDatabase.Auths.ToArray()[index - 2];
                var confirm = ConsoleUtils.YesNo($"删除账户 {item.Value.UserName}");
                if (confirm)
                {
                    ConsoleUtils.Info("正在删除账户");
                    AuthDatabase.Delete(item.Value);
                    ConsoleUtils.Ok("已删除账户");
                    ConsoleUtils.Keep();
                }

                Show();
                break;
        }
    }
}
