using ColorMC.Core.Game.Auth;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Windows;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColorMC.Gui.UIBinding;

public static class UserBinding
{
    private readonly static List<LoginObj> LockUser = new();
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

    public static Dictionary<(string, AuthType), LoginObj> GetAllUser()
    {
        return AuthDatabase.Auths;
    }

    public static void Remove(string uuid, AuthType type)
    {
        if (GuiConfigUtils.Config.LastUser != null
            && type == GuiConfigUtils.Config.LastUser.Type
            && uuid == GuiConfigUtils.Config.LastUser.UUID)
        {
            ClearLastUser();
        }
        var item = AuthDatabase.Get(uuid, type);
        if (item != null)
            AuthDatabase.Delete(item);

        MainWindow.OnUserEdit();
    }

    public static LoginObj? GetLastUser()
    {
        var obj = GuiConfigUtils.Config?.LastUser;
        if (obj == null)
            return null;
        return AuthDatabase.Get(obj.UUID, obj.Type);
    }

    public static void SetLastUser(string uuid, AuthType type)
    {
        GuiConfigUtils.Config.LastUser = new()
        {
            Type = type,
            UUID = uuid
        };

        GuiConfigUtils.Save();

        MainWindow.OnUserEdit();
    }

    public static void ClearLastUser()
    {
        GuiConfigUtils.Config.LastUser = null;
        GuiConfigUtils.Save();
    }

    public static void AddLockUser(LoginObj obj)
    {
        if (!LockUser.Contains(obj))
        {
            LockUser.Add(obj);
        }
    }

    public static void RemoveLockUser(LoginObj obj) 
    {
        LockUser.Remove(obj);
    }

    public static bool IsLock(LoginObj obj) 
    {
        return LockUser.Contains(obj);
    }
}
