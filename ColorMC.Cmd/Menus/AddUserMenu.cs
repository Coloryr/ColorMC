using ColorMC.Core;
using ColorMC.Core.Game.Auth;
using ColorMC.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Cmd.Menus;

public static class AddUserMenu
{
    private static string Title = "添加账户";
    private static string[] Items = new string[] 
    { 
        "离线登录", 
        "微软登录", 
        "统一通行证登录", 
        "皮肤站(LittleSkin)", 
        "自建皮肤站(blessing-skin-server)", 
        "取消" 
    };

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
                ConsoleUtils.Input("添加离线账户");
                var name = ConsoleUtils.ReadLine("游戏名");
                if (string.IsNullOrWhiteSpace(name))
                {
                    ConsoleUtils.Error("游戏名不能为空");
                }
                else
                {
                    ConsoleUtils.Info($"正在添加离线账户:{name}");
                    AuthDatabase.SaveAuth(new()
                    { 
                        UserName = name,
                        ClientToken = Funtcions.NewUUID(),
                        UUID = Funtcions.NewUUID(),
                        AuthType = AuthType.Offline
                    });
                    ConsoleUtils.Ok("已添加");
                }
                ConsoleUtils.Keep();
                UserMenu.Show();
                break;
            case 1:
                ConsoleUtils.ToEnd();
                ConsoleUtils.Info("添加微软账户");
                CoreMain.AuthStateUpdate = StateUp;
                CoreMain.LoginOAuthCode = Code;
                var (State, State1, Obj, Message) = BaseAuth.LoginWithOAuth().Result;
                if (State1 != LoginState.Done)
                {
                    ConsoleUtils.Error($"{State.GetName()}登录错误");
                    ConsoleUtils.Error(Message);
                }
                else
                {
                    AuthDatabase.SaveAuth(Obj!);
                    ConsoleUtils.Ok(Message);
                }
                ConsoleUtils.Keep();
                break;
            case 2:
                break;
            case 3:

                break;
            case 4:
                break;
            case 5:
                
                break;
        }

        UserMenu.Show();
    }

    private static void StateUp(AuthState state)
    {
        ConsoleUtils.Info($"登录状态:{state.GetName()}");
    }

    private static void Code(string url, string code)
    {
        ConsoleUtils.Info1($"请用浏览器打开{url} 并输入代码{code} 以继续登录账户");
    }
}
