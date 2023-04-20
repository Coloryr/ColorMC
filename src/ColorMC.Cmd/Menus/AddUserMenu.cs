using ColorMC.Core;
using ColorMC.Core.Game;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;

namespace ColorMC.Cmd.Menus;

public static class AddUserMenu
{
    private const string Title = "添加账户";
    private static List<string> Items = new()
    {
        "离线登录(offline)",
        "微软登录(oauth)",
        "统一通行证登录(nide8)",
        "皮肤站(little-skin)",
        "自建皮肤站(blessing-skin-server)",
        "外置登录(authlib-injector)",
        "取消"
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
                ConsoleUtils.Input("添加离线账户");
                var name = ConsoleUtils.ReadLine("用户名");
                if (string.IsNullOrWhiteSpace(name))
                {
                    ConsoleUtils.Error("游戏名不能为空");
                }
                else
                {
                    ConsoleUtils.Info($"正在添加离线账户:{name}");
                    new LoginObj()
                    {
                        UserName = name,
                        ClientToken = Funtcions.NewUUID(),
                        UUID = Funtcions.NewUUID(),
                        AuthType = AuthType.Offline
                    }.Save();
                    ConsoleUtils.Ok("已添加");
                }
                ConsoleUtils.Keep();
                UserMenu.Show();
                return;
            case 1:
                ConsoleUtils.ToEnd();
                ConsoleUtils.Info("添加微软账户");
                ColorMCCore.AuthStateUpdate = StateUp;
                ColorMCCore.LoginOAuthCode = Code;
                var (State, State1, Obj, Message, Ex) = BaseAuth.LoginWithOAuth().Result;
                if (State1 != LoginState.Done)
                {
                    ConsoleUtils.Error($"{State.GetName()}登录错误");
                    ConsoleUtils.Error(Message);
                }
                else
                {
                    AuthDatabase.Save(Obj!);
                    ConsoleUtils.Ok(Message);
                }
                ConsoleUtils.Keep();
                UserMenu.Show();
                return;
            case 2:
                ConsoleUtils.ToEnd();
                ConsoleUtils.Info("添加统一通行证账户");
                ColorMCCore.AuthStateUpdate = StateUp;
                var server = ConsoleUtils.ReadLine("服务器UUID");
                if (string.IsNullOrWhiteSpace(server))
                {
                    ConsoleUtils.Error("服务器UUID不能为空");
                    ConsoleUtils.Keep();
                    break;
                }
                else if (server.Length != 32)
                {
                    ConsoleUtils.Error("服务器UUID错误");
                    ConsoleUtils.Keep();
                    break;
                }
                name = ConsoleUtils.ReadLine("邮箱或用户名");
                if (string.IsNullOrWhiteSpace(name))
                {
                    ConsoleUtils.Error("用户名不能为空");
                    ConsoleUtils.Keep();
                    break;
                }
                var pass = ConsoleUtils.ReadPassword("密码");
                if (string.IsNullOrWhiteSpace(pass))
                {
                    ConsoleUtils.Error("密码不能为空");
                    ConsoleUtils.Keep();
                    break;
                }
                (State, State1, Obj, Message, Ex) = BaseAuth.LoginWithNide8(server, name, pass).Result;
                if (State1 != LoginState.Done)
                {
                    ConsoleUtils.Error($"{State.GetName()}登录错误");
                    ConsoleUtils.Error(Message);
                }
                else
                {
                    AuthDatabase.Save(Obj!);
                    ConsoleUtils.Ok(Message);
                }
                ConsoleUtils.Keep();
                UserMenu.Show();
                return;
            case 3:
                ConsoleUtils.ToEnd();
                ConsoleUtils.Info("添加皮肤站账户");
                ColorMCCore.AuthStateUpdate = StateUp;
                name = ConsoleUtils.ReadLine("邮箱或用户名");
                if (string.IsNullOrWhiteSpace(name))
                {
                    ConsoleUtils.Error("用户名不能为空");
                    ConsoleUtils.Keep();
                    break;
                }
                pass = ConsoleUtils.ReadPassword("密码");
                if (string.IsNullOrWhiteSpace(pass))
                {
                    ConsoleUtils.Error("密码不能为空");
                    ConsoleUtils.Keep();
                    break;
                }
                (State, State1, Obj, Message, Ex) = BaseAuth.LoginWithLittleSkin(name, pass).Result;
                if (State1 != LoginState.Done)
                {
                    ConsoleUtils.Error($"{State.GetName()}登录错误");
                    ConsoleUtils.Error(Message);
                }
                else
                {
                    AuthDatabase.Save(Obj!);
                    ConsoleUtils.Ok(Message);
                }
                ConsoleUtils.Keep();
                UserMenu.Show();
                return;
            case 4:
                ConsoleUtils.ToEnd();
                ConsoleUtils.Info("添加自定义皮肤站账户");
                ColorMCCore.AuthStateUpdate = StateUp;
                server = ConsoleUtils.ReadLine("皮肤站网址");
                if (string.IsNullOrWhiteSpace(server))
                {
                    ConsoleUtils.Error("皮肤站网址不能为空");
                    ConsoleUtils.Keep();
                    break;
                }
                name = ConsoleUtils.ReadLine("邮箱或用户名");
                if (string.IsNullOrWhiteSpace(name))
                {
                    ConsoleUtils.Error("用户名不能为空");
                    ConsoleUtils.Keep();
                    break;
                }
                pass = ConsoleUtils.ReadPassword("密码");
                if (string.IsNullOrWhiteSpace(pass))
                {
                    ConsoleUtils.Error("密码不能为空");
                    ConsoleUtils.Keep();
                    break;
                }
                (State, State1, Obj, Message, Ex) = BaseAuth.LoginWithLittleSkin(name, pass, server).Result;
                if (State1 != LoginState.Done)
                {
                    ConsoleUtils.Error($"{State.GetName()}登录错误");
                    ConsoleUtils.Error(Message);
                }
                else
                {
                    AuthDatabase.Save(Obj!);
                    ConsoleUtils.Ok(Message);
                }
                ConsoleUtils.Keep();
                UserMenu.Show();
                return;
            case 5:
                ConsoleUtils.ToEnd();
                ConsoleUtils.Info("添加外置登录账户");
                ColorMCCore.AuthStateUpdate = StateUp;
                server = ConsoleUtils.ReadLine("验证网址");
                if (string.IsNullOrWhiteSpace(server))
                {
                    ConsoleUtils.Error("验证网址不能为空");
                    ConsoleUtils.Keep();
                    break;
                }
                name = ConsoleUtils.ReadLine("邮箱或用户名");
                if (string.IsNullOrWhiteSpace(name))
                {
                    ConsoleUtils.Error("用户名不能为空");
                    ConsoleUtils.Keep();
                    break;
                }
                pass = ConsoleUtils.ReadPassword("密码");
                if (string.IsNullOrWhiteSpace(pass))
                {
                    ConsoleUtils.Error("密码不能为空");
                    ConsoleUtils.Keep();
                    break;
                }
                (State, State1, Obj, Message, Ex) = BaseAuth.LoginWithAuthlibInjector(name, pass, server).Result;
                if (State1 != LoginState.Done)
                {
                    ConsoleUtils.Error($"{State.GetName()}登录错误");
                    ConsoleUtils.Error(Message);
                }
                else
                {
                    AuthDatabase.Save(Obj!);
                    ConsoleUtils.Ok(Message);
                }
                ConsoleUtils.Keep();
                UserMenu.Show();
                return;
            case 6:
                MainMenu.Show();
                return;
        }

        Show();
    }

    private static void StateUp(AuthState state)
    {
        ConsoleUtils.Info($"登录状态:{state.GetName()}");
    }

    private static void Code(string url, string code)
    {
        ConsoleUtils.Info1($"请用浏览器打开 {url} 并输入代码 {code} 以继续登录账户");
    }
}
