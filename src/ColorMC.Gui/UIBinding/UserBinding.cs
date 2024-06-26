using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UIBinding;

public static class UserBinding
{
    private static readonly List<(AuthType, string)> s_lockUser = [];

    public static List<string> GetLoginType()
    {
        var list = new List<string>()
        {
            AuthType.OAuth.GetName(),
            AuthType.Nide8.GetName(),
            AuthType.AuthlibInjector.GetName(),
            AuthType.LittleSkin.GetName(),
            AuthType.SelfLittleSkin.GetName()
        };
        return list;
    }

    public static List<string> GetUserTypes()
    {
        var list = new List<string>()
        {
            AuthType.Offline.GetName(),
            AuthType.OAuth.GetName(),
            AuthType.Nide8.GetName(),
            AuthType.AuthlibInjector.GetName(),
            AuthType.LittleSkin.GetName(),
            AuthType.SelfLittleSkin.GetName()
        };
        return list;
    }

    public static List<string> GetDisplayUserTypes()
    {
        var list = new List<string>()
        {
            "",
            AuthType.Offline.GetName(),
            AuthType.OAuth.GetName(),
            AuthType.Nide8.GetName(),
            AuthType.AuthlibInjector.GetName(),
            AuthType.LittleSkin.GetName(),
            AuthType.SelfLittleSkin.GetName()
        };
        return list;
    }

    public static int ToInt(this AuthType type)
    {
        return type switch
        {
            AuthType.Offline => 0,
            AuthType.OAuth => 1,
            AuthType.Nide8 => 2,
            AuthType.AuthlibInjector => 3,
            AuthType.LittleSkin => 4,
            AuthType.SelfLittleSkin => 5,
            _ => -1
        };
    }

    public static async Task<(bool, string?)> AddUser(AuthType type, ColorMCCore.LoginOAuthCode loginOAuth,
        string? input1, string? input2 = null, string? input3 = null)
    {
        if (type == AuthType.Offline)
        {
            new LoginObj()
            {
                UserName = input1!,
                ClientToken = FuntionUtils.NewUUID(),
                UUID = HashHelper.GenMd5(Encoding.UTF8.GetBytes(input1!.ToLower())),
                AuthType = AuthType.Offline
            }.Save();
            return (true, null);
        }
        var res1 = type switch
        {
            AuthType.OAuth => await GameAuth.LoginOAuthAsync(loginOAuth),
            AuthType.Nide8 => await GameAuth.LoginNide8Async(input1!, input2!, input3!),
            AuthType.AuthlibInjector => await GameAuth.LoginAuthlibInjectorAsync(input1!, input2!, input3!),
            AuthType.LittleSkin => await GameAuth.LoginLittleSkinAsync(input1!, input2!),
            AuthType.SelfLittleSkin => await GameAuth.LoginLittleSkinAsync(input1!, input2!, input3!),
            _ => throw new Exception("Type Error")
        };

        if (res1.LoginState != LoginState.Done)
        {
            if (res1.Ex != null)
            {
                WindowManager.ShowError(res1.Message!, res1.Ex);
                return (false, App.Lang("Gui.Error4"));
            }
            else
            {
                return (false, res1.Message);
            }
        }
        if (string.IsNullOrWhiteSpace(res1.Auth?.UUID))
        {
            WebBinding.OpenWeb(WebType.Minecraft);
            return (false, App.Lang("Gui.Error47"));
        }
        AuthDatabase.Save(res1.Auth!);
        return (true, null);
    }

    public static Dictionary<(string, AuthType), LoginObj> GetAllUser()
    {
        return new(AuthDatabase.Auths);
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

        App.OnUserEdit();
    }

    public static LoginObj? GetLastUser()
    {
        var obj = GuiConfigUtils.Config?.LastUser;
        if (obj == null)
            return null;
        return AuthDatabase.Get(obj.UUID, obj.Type);
    }

    public static async Task<bool> ReLogin(string uuid, AuthType type)
    {
        var obj = AuthDatabase.Get(uuid, type);
        if (obj == null)
        {
            return false;
        }

        obj.AccessToken = "";

        return (await obj.RefreshTokenAsync()).LoginState == LoginState.Done;
    }

    public static void SetLastUser(string uuid, AuthType type)
    {
        GuiConfigUtils.Config.LastUser = new()
        {
            Type = type,
            UUID = uuid
        };

        GuiConfigUtils.Save();

        App.OnUserEdit();
    }

    public static void ClearLastUser()
    {
        GuiConfigUtils.Config.LastUser = null;
        GuiConfigUtils.Save();
    }

    public static LoginObj? GetUser(string uuid, AuthType type)
    {
        return AuthDatabase.Get(uuid, type);
    }

    public static void AddLockUser(LoginObj obj)
    {
        if (!s_lockUser.Contains((obj.AuthType, obj.UUID)))
        {
            s_lockUser.Add((obj.AuthType, obj.UUID));
        }
    }

    public static void UnLockUser(LoginObj obj)
    {
        s_lockUser.Remove((obj.AuthType, obj.UUID));
    }

    public static bool IsLock(LoginObj obj)
    {
        return s_lockUser.Contains((obj.AuthType, obj.UUID));
    }

    public static async Task LoadSkin()
    {
        var obj = GetLastUser();

        if (obj == null)
        {
            ImageManager.SetDefaultHead();
            return;
        }

        string? file = null, file1 = null;
        var temp = await PlayerSkinAPI.DownloadSkin(obj);
        if (temp.Item1)
        {
            file = AssetsPath.GetSkinFile(obj);
        }
        if (temp.Item2)
        {
            file1 = AssetsPath.GetCapeFile(obj);
        }

        ImageManager.LoadSkinHead(file, file1);
        App.OnSkinLoad();
    }

    public static async void EditSkin()
    {
        var obj = GetLastUser();
        if (obj == null)
        {
            return;
        }

        switch (obj.AuthType)
        {
            case AuthType.Offline:
                var file = await PathBinding.SelectFile(FileType.Head);
                if (file.Item1 != null)
                {
                    obj.SaveSkin(file.Item1);
                }
                break;
            case AuthType.OAuth:
                WebBinding.OpenWeb(WebType.EditSkin);
                break;
            case AuthType.Nide8:
                BaseBinding.OpUrl($"https://login.mc-user.com:233/{obj.Text1}/skin");
                break;
            //case AuthType.AuthlibInjector:
            //BaseBinding.OpUrl($"https://login.mc-user.com:233/{obj.Text1}/skin");
            //break;
            case AuthType.LittleSkin:
                WebBinding.OpenWeb(WebType.LittleSkinEditSkin);
                break;
            case AuthType.SelfLittleSkin:
                BaseBinding.OpUrl($"{obj.Text1}/user/closet");
                break;
        }
    }

    public static void ClearAllUser()
    {
        AuthDatabase.Auths.Clear();

        AuthDatabase.Save();

        GuiConfigUtils.Config.LastUser = null;

        GuiConfigUtils.Save();

        App.OnUserEdit();
    }

    public static void UserLastUser()
    {
        if (AuthDatabase.Auths.Count == 1)
        {
            var item = AuthDatabase.Auths.ToList()[0];
            SetLastUser(item.Key.Item1, item.Key.Item2);
        }
    }

    public static void OAuthCancel()
    {
        GameAuth.CancelWithOAuth();
    }

    public static void EditUser(string name, string uuid, string text1, string text2)
    {
        foreach (var item in AuthDatabase.Auths.Values)
        {
            if (item.UserName == name && item.UUID == uuid && item.AuthType == AuthType.Offline)
            {
                item.UserName = text1;
                item.UUID = text2;
                AuthDatabase.Save();
                break;
            }
        }
    }

    public static bool HaveOnline()
    {
        return AuthDatabase.Auths.Keys.Any(a => a.Item2 == AuthType.OAuth);
    }

    public static async Task<bool> TestLogin(LoginObj user)
    {
        return (await user.RefreshTokenAsync()).LoginState == LoginState.Done;
    }
}
