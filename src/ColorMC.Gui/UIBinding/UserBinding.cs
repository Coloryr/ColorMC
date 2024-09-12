using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
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
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UIBinding;

public static class UserBinding
{
    public static event Action? UserEdit;

    private static readonly List<UserKeyObj> s_lockUser = [];
    private static bool s_notDisplayUserLock;

    /// <summary>
    /// ����˻�
    /// </summary>
    /// <param name="type"></param>
    /// <param name="loginOAuth"></param>
    /// <param name="input1"></param>
    /// <param name="input2"></param>
    /// <param name="input3"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static async Task<MessageRes> AddUser(AuthType type, ColorMCCore.LoginOAuthCode loginOAuth,
        string? input1, string? input2 = null, string? input3 = null)
    {
        if (type == AuthType.Offline)
        {
            var user = new LoginObj()
            {
                UserName = input1!,
                ClientToken = FuntionUtils.NewUUID(),
                UUID = HashHelper.GenMd5(Encoding.UTF8.GetBytes(input1!.ToLower())),
                AuthType = AuthType.Offline
            };
            user.Save();
            SetSelectUser(user.UUID, user.AuthType);
            return new() { State = true };
        }
        var res1 = type switch
        {
            AuthType.OAuth => await GameAuth.LoginOAuthAsync(loginOAuth),
            AuthType.Nide8 => await GameAuth.LoginNide8Async(input1!, input2!, input3!),
            AuthType.AuthlibInjector => await GameAuth.LoginAuthlibInjectorAsync(input1!, input2!, input3!),
            AuthType.LittleSkin => await GameAuth.LoginLittleSkinAsync(input1!, input2!),
            AuthType.SelfLittleSkin => await GameAuth.LoginLittleSkinAsync(input1!, input2!, input3!),
            _ => throw new Exception(App.Lang("UserBinding.Error2"))
        };

        if (res1.LoginState != LoginState.Done)
        {
            if (res1.Ex != null)
            {
                WindowManager.ShowError(res1.Message!, res1.Ex);
                return new() { Message = App.Lang("UserBinding.Error1") };
            }
            else
            {
                return new() { Message = res1.Message };
            }
        }
        if (string.IsNullOrWhiteSpace(res1.Auth?.UUID))
        {
            WebBinding.OpenWeb(WebType.Minecraft);
            return new() { Message = App.Lang("UserBinding.Error3") };
        }
        res1.Auth!.Save();
        SetSelectUser(res1.Auth.UUID, res1.Auth.AuthType);
        return new() { State = true };
    }

    /// <summary>
    /// ��ȡ�����˻�
    /// </summary>
    /// <returns></returns>
    public static Dictionary<UserKeyObj, LoginObj> GetAllUser()
    {
        return AuthDatabase.Auths;
    }

    /// <summary>
    /// ɾ���˻�
    /// </summary>
    /// <param name="uuid"></param>
    /// <param name="type"></param>
    public static void Remove(string uuid, AuthType type)
    {
        if (GuiConfigUtils.Config.LastUser is { } last
            && type == last.Type && uuid == last.UUID)
        {
            ClearLastUser();
        }
        AuthDatabase.Get(uuid, type)?.Delete();

        OnUserEdit();
    }

    /// <summary>
    /// ��ȡ�����˻�
    /// </summary>
    /// <returns></returns>
    public static LoginObj? GetLastUser()
    {
        var obj = GuiConfigUtils.Config?.LastUser;
        if (obj == null)
        {
            return null;
        }
        return AuthDatabase.Get(obj.UUID, obj.Type);
    }

    /// <summary>
    /// ���µ�¼�˻�
    /// </summary>
    /// <param name="uuid"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static async Task<bool> ReLogin(string uuid, AuthType type)
    {
        var obj = AuthDatabase.Get(uuid, type);
        if (obj == null)
        {
            return false;
        }

        //obj.AccessToken = "";
        return (await obj.RefreshTokenAsync()).LoginState == LoginState.Done;
    }

    /// <summary>
    /// ����ѡ���˻�
    /// </summary>
    /// <param name="uuid"></param>
    /// <param name="type"></param>
    public static void SetSelectUser(string uuid, AuthType type)
    {
        GuiConfigUtils.Config.LastUser = new()
        {
            Type = type,
            UUID = uuid
        };

        GuiConfigUtils.Save();

        OnUserEdit();
    }

    /// <summary>
    /// ɾ��ѡ���˻�
    /// </summary>
    public static void ClearLastUser()
    {
        GuiConfigUtils.Config.LastUser = null;
        GuiConfigUtils.Save();
    }

    /// <summary>
    /// �����˻�
    /// </summary>
    /// <param name="obj"></param>
    public static void AddLockUser(LoginObj obj)
    {
        var key = obj.GetKey();
        if (!s_lockUser.Contains(key))
        {
            s_lockUser.Add(key);
        }
    }

    /// <summary>
    /// �����˻�
    /// </summary>
    /// <param name="obj"></param>
    public static void UnLockUser(LoginObj obj)
    {
        s_lockUser.Remove(obj.GetKey());
    }

    /// <summary>
    /// �˻��Ƿ�����
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool IsLock(LoginObj obj)
    {
        return s_lockUser.Contains(obj.GetKey());
    }

    /// <summary>
    /// �����˻�Ƥ��
    /// </summary>
    /// <returns></returns>
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
    }

    /// <summary>
    /// ���»����˻�Ƥ��
    /// </summary>
    public static void ReloadSkin()
    {
        var obj = GetLastUser();

        if (obj == null)
        {
            return;
        }

        ImageManager.ReloadSkinHead();
    }

    /// <summary>
    /// �༭Ƥ��
    /// </summary>
    public static async void EditSkin(TopLevel top)
    {
        var obj = GetLastUser();
        if (obj == null)
        {
            return;
        }

        switch (obj.AuthType)
        {
            case AuthType.Offline:
                var file = await PathBinding.SelectFile(top, FileType.Head);
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

    /// <summary>
    /// ��������˻�
    /// </summary>
    public static void ClearAllUser()
    {
        AuthDatabase.ClearAuths();

        GuiConfigUtils.Config.LastUser = null;
        GuiConfigUtils.Save();

        OnUserEdit();
    }

    /// <summary>
    /// ȡ��΢���¼
    /// </summary>
    public static void OAuthCancel()
    {
        GameAuth.CancelWithOAuth();
    }

    /// <summary>
    /// �༭�˻�
    /// </summary>
    /// <param name="name"></param>
    /// <param name="uuid"></param>
    /// <param name="text1"></param>
    /// <param name="text2"></param>
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

    /// <summary>
    /// �Ƿ���΢���˻�
    /// </summary>
    /// <returns></returns>
    public static bool HaveOnline()
    {
        return AuthDatabase.Auths.Keys.Any(a => a.Type == AuthType.OAuth);
    }

    /// <summary>
    /// ���Ե�¼
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static async Task<bool> TestLogin(LoginObj user)
    {
        return (await user.RefreshTokenAsync()).LoginState == LoginState.Done;
    }

    /// <summary>
    /// ���˻��༭��
    /// </summary>
    public static void OnUserEdit()
    {
        UserEdit?.Invoke();
    }

    /// <summary>
    /// ��ȡѡ�е��˻�
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public static async Task<GameLaunchUserRes> GetLaunchUser(BaseModel model)
    {
        var login = GetLastUser();
        if (login == null)
        {
            return new() { Message = App.Lang("GameBinding.Error3") };
        }
        if (login.AuthType == AuthType.Offline)
        {
            var have = AuthDatabase.Auths.Keys.Any(a => a.Type == AuthType.OAuth);
            if (!have)
            {
                WebBinding.OpenWeb(WebType.Minecraft);
                return new() { Message = App.Lang("GameBinding.Error4") };
            }
        }

        if (IsLock(login) && !s_notDisplayUserLock)
        {
            var res = await model.ShowWait(App.Lang("GameBinding.Error1"));
            if (!res)
            {
                return new() { Message = App.Lang("GameBinding.Error5") };
            }

            Dispatcher.UIThread.Post(async () =>
            {
                var res1 = await model.ShowWait(App.Lang("GameBinding.Info18"));
                if (res1)
                {
                    s_notDisplayUserLock = true;
                }
            });
        }

        return new() { User = login };
    }
}
