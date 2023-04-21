using ColorMC.Core.Helpers;
using ColorMC.Core.Utils;

namespace ColorMC.Cmd.Menus;

public static class SelectUserMenu
{
    private const string Title = "选择启动账户";
    private static List<string> Items;

    public static void Show()
    {
        var list = AuthDatabase.Auths;
        Items = new List<string>();
        ConsoleUtils.Reset();
        ConsoleUtils.ShowTitle(Title);

        foreach (var item in list)
        {
            Items.Add("[" + item.Key + "|" + item.Value.UserName + "|" + item.Value.AuthType.GetName() + "]");
        }

        ConsoleUtils.SetItems(Items, Select);
    }

    private static void Select(int index)
    {
        LaunchMenu.SelectItem(AuthDatabase.Auths.ToArray()[index].Value);
    }
}
