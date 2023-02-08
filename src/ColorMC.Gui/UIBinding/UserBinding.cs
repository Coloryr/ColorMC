using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ColorMC.Core.Game.Auth;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using ColorMC.Gui.Skin;
using ColorMC.Gui.Utils.LaunchSetting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Image = SixLabors.ImageSharp.Image;

namespace ColorMC.Gui.UIBinding;

public static class UserBinding
{
    private readonly static List<LoginObj> LockUser = new();
    public static Image<Rgba32>? SkinImage { get; set; }
    public static Bitmap? HeadBitmap { get; private set; }
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

    public static async Task<(bool, string?)> AddUser(int type, string? input1, string? input2 = null, string? input3 = null)
    {
        if (type == 0)
        {
            await AuthDatabase.Save(new()
            {
                UserName = input1!,
                ClientToken = Funtcions.NewUUID(),
                UUID = Funtcions.GenMd5(Encoding.UTF8.GetBytes(input1!.ToLower())),
                AuthType = AuthType.Offline
            });
            return (true, null);
        }
        else if (type <= 5)
        {
            var (State, State1, Obj, Message, Ex) = type switch
            {
                1 => await BaseAuth.LoginWithOAuth(),
                2 => await BaseAuth.LoginWithNide8(input1!, input2!, input3!),
                3 => await BaseAuth.LoginWithAuthlibInjector(input1!, input2!, input3!),
                4 => await BaseAuth.LoginWithLittleSkin(input1!, input2!),
                5 => await BaseAuth.LoginWithLittleSkin(input1!, input2!, input3!),
                _ => (AuthState.Profile, LoginState.Error, null, null, null)
            };

            if (State1 != LoginState.Done)
            {
                if (Ex != null)
                {
                    App.ShowError(Message!, Ex);
                    return (false, Localizer.Instance["Error4"]);
                }
                else
                {
                    return (false, Message);
                }
            }
            await AuthDatabase.Save(Obj!);
            return (true, null);
        }

        return (false, Localizer.Instance["UserBinding.Error1"]);
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

        var res = await obj.RefreshToken();

        return res.State1 == LoginState.Done;
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

    public static async Task LoadSkin()
    {
        var obj = GetLastUser();

        SkinImage?.Dispose();
        HeadBitmap?.Dispose();

        SkinImage = null;

        var uri = new Uri($"resm:ColorMC.Gui.Resource.Pic.user.png");
        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
        using var asset = assets!.Open(uri);

        if (obj == null)
        {
            HeadBitmap = new Bitmap(asset);
            App.OnSkinLoad();
            return;
        }

        string? file;
        if (obj.AuthType == AuthType.Offline)
        {
            file = AssetsPath.GetSkin(obj);
        }
        else
        {
            file = await GetSkin.DownloadSkin(obj);
        }

        if (file == null)
        {
            HeadBitmap = new Bitmap(asset);
            App.OnSkinLoad();
            return;
        }

        SkinImage = await Image.LoadAsync<Rgba32>(file);

        var data = await ImageUtils.MakeHeadImage(file);
        if (file == null)
        {
            HeadBitmap = new Bitmap(asset);
            App.OnSkinLoad();
            return;
        }

        data.Seek(0, SeekOrigin.Begin);
        HeadBitmap = new Bitmap(data);
        data.Close();

        App.OnSkinLoad();
    }

    public static List<string> GetSkinType()
    {
        var list = new List<string>();
        Array values = Enum.GetValues(typeof(SkinType));
        foreach (SkinType value in values)
        {
            list.Add(value.GetName());
        }

        return list;
    }

    public static async void EditSkin(Window window)
    {
        var obj = GetLastUser();
        if (obj == null)
            return;

        switch (obj.AuthType)
        {
            case AuthType.Offline:
                var file = await BaseBinding.OpFile(window, "选择皮肤导入", "*.png", "皮肤文件");
                if (file?.Any() == true)
                {
                    var item = file[0];
                    AssetsPath.SaveSkin(obj, item.GetPath());
                }
                break;
            case AuthType.OAuth:
                BaseBinding.OpUrl("https://www.minecraft.net/en-us/msaprofile/mygames/editskin");
                break;
            case AuthType.Nide8:
                BaseBinding.OpUrl($"https://login.mc-user.com:233/{obj.Text1}/skin");
                break;
            case AuthType.AuthlibInjector:
                //BaseBinding.OpUrl($"https://login.mc-user.com:233/{obj.Text1}/skin");
                break;
            case AuthType.LittleSkin:
                BaseBinding.OpUrl("https://littleskin.cn/user/closet");
                break;
            case AuthType.SelfLittleSkin:
                BaseBinding.OpUrl($"{obj.Text1}/user/closet");
                break;
        }
    }
}
