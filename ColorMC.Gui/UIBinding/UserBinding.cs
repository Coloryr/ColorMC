using ColorMC.Core.Game.Auth;
using System;
using System.Collections.Generic;
using ColorMC.Core.Utils;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Http.Login;
using ColorMC.Core.Objs.Login;
using System.Security.AccessControl;
using ColorMC.Core;

namespace ColorMC.Gui.UIBinding;

public record UserObj
{
    public string Name { get; set; }
    public string Info { get; set; }
    public AuthType Type { get; set; }
}

public static class UserBinding
{
    public static List<string> GetUserTypes()
    {
        var list = new List<string>();
        Array values = Enum.GetValues(typeof(AuthType));
        foreach (AuthType value in values)
        {
            list.Add(value.GetName());
        }

        return list;
    }

    public static async Task<(bool, string?)> AddUser(int type, string? input, string? input1 = null, string? input2 = null)
    {
        switch (type)
        {
            case 0:
                AuthDatabase.SaveAuth(new()
                {
                    UserName = input!,
                    ClientToken = Funtcions.NewUUID(),
                    UUID = Funtcions.NewUUID(),
                    AuthType = AuthType.Offline
                });
                return (true, null);
            case 1:
                var (State, State1, Obj, Message) = await BaseAuth.LoginWithOAuth();
                if (State1 != LoginState.Done)
                {
                    return (false, Message);
                }
                AuthDatabase.SaveAuth(Obj!);
                return (true, null);
            case 2:
                (State, State1, Obj, Message) = await BaseAuth.LoginWithNide8(input!, input1!, input2!);
                if (State1 != LoginState.Done)
                {
                    return (false, Message);
                }
                AuthDatabase.SaveAuth(Obj!);
                return (true, null);
            case 3:
                (State, State1, Obj, Message) = await BaseAuth.LoginWithAuthlibInjector(input!, input1!, input2!);
                if (State1 != LoginState.Done)
                {
                    return (false, Message);
                }
                AuthDatabase.SaveAuth(Obj!);
                return (true, null);
            case 4:
                (State, State1, Obj, Message) = await BaseAuth.LoginWithLittleSkin(input1!, input2!);
                if (State1 != LoginState.Done)
                {
                    return (false, Message);
                }
                AuthDatabase.SaveAuth(Obj!);
                return (true, null);
            case 5:
                (State, State1, Obj, Message) = await BaseAuth.LoginWithLittleSkin(input1!, input2!, input!);
                if (State1 != LoginState.Done)
                {
                    return (false, Message);
                }
                AuthDatabase.SaveAuth(Obj!);
                return (true, null);
        }

        return (false, "登录类型错误");
    }

    public static List<UserObj> GetAllUser()
    {
        var list = new List<UserObj>();
        foreach (var item in AuthDatabase.Auths)
        {
            list.Add(new()
            {
                Name = item.Key.Item1,
                Info = item.Value.UserName + " " + item.Key.Item2.GetName(),
                Type = item.Key.Item2
            });
        }

        return list;
    }

    public static void Remove(UserObj name)
    {
        var item = AuthDatabase.Get(name.Name, name.Type);
        if (item != null)
            AuthDatabase.Delete(item);
    }

    public static LoginObj? GetLastUser()
    {
        //var obj = ConfigUtils.Config.LastUser;
        //if (obj == null)
        //    return null;
        //return AuthDatabase.Get(obj.UUID, obj.Type);

        var item = GetAllUser().Where(a => a.Type == AuthType.OAuth).First();

        return AuthDatabase.Get(item.Name, item.Type);
    }
}
