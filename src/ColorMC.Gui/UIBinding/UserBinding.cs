using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.Utils;
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
    private readonly static List<(AuthType, string)> LockUser = new();
    public static Image<Rgba32>? SkinImage { get; set; }
    public static Image<Rgba32>? CapeIamge { get; set; }
    public static Bitmap HeadBitmap { get; private set; }
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

    public static async Task<(bool, string?)> AddUser(AuthType type, string? input1,
        string? input2 = null, string? input3 = null)
    {
        if (type == AuthType.Offline)
        {
            AuthDatabase.Save(new()
            {
                UserName = input1!,
                ClientToken = Funtcions.NewUUID(),
                UUID = Funtcions.GenMd5(Encoding.UTF8.GetBytes(input1!.ToLower())),
                AuthType = AuthType.Offline
            });
            return (true, null);
        }
        var (State, State1, Obj, Message, Ex) = type switch
        {
            AuthType.OAuth => await BaseAuth.LoginWithOAuth(),
            AuthType.Nide8 => await BaseAuth.LoginWithNide8(input1!, input2!, input3!),
            AuthType.AuthlibInjector => await BaseAuth.LoginWithAuthlibInjector(input1!, input2!, input3!),
            AuthType.LittleSkin => await BaseAuth.LoginWithLittleSkin(input1!, input2!),
            AuthType.SelfLittleSkin => await BaseAuth.LoginWithLittleSkin(input1!, input2!, input3!),
            _ => (AuthState.Profile, LoginState.Error, null, null, null)
        };

        if (State1 != LoginState.Done)
        {
            if (Ex != null)
            {
                App.ShowError(Message!, Ex);
                return (false, App.GetLanguage("Gui.Error4"));
            }
            else
            {
                return (false, Message);
            }
        }
        if (string.IsNullOrWhiteSpace(Obj?.UUID))
        {
            BaseBinding.OpUrl("https://minecraft.net");
            return (false, App.GetLanguage("UserBinding.Info3"));
        }
        AuthDatabase.Save(Obj!);
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

        return (await obj.RefreshToken()).State1 == LoginState.Done;
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
        if (!LockUser.Contains((obj.AuthType, obj.UUID)))
        {
            LockUser.Add((obj.AuthType, obj.UUID));
        }
    }

    public static void UnLockUser(LoginObj obj)
    {
        LockUser.Remove((obj.AuthType, obj.UUID));
    }

    public static bool IsLock(LoginObj obj)
    {
        return LockUser.Contains((obj.AuthType, obj.UUID));
    }

    public static async Task LoadSkin()
    {
        var obj = GetLastUser();

        SkinImage?.Dispose();
        CapeIamge?.Dispose();
        HeadBitmap?.Dispose();

        SkinImage = null;
        CapeIamge = null;

        var uri = new Uri($"resm:ColorMC.Gui.Resource.Pic.user.png");
        using var asset = AssetLoader.Open(uri);

        if (obj == null)
        {
            HeadBitmap = new Bitmap(asset);
            App.OnSkinLoad();
            return;
        }

        string? file = null, file1 = null;
        (bool, bool) temp;
        if (obj.AuthType == AuthType.Offline)
        {
            file = AssetsPath.GetSkinFile(obj);
            if (!File.Exists(file))
            {
                file = null;
            }
        }
        else
        {
            temp = await PlayerSkinAPI.DownloadSkin(obj);
            if (temp.Item1)
            {
                file = AssetsPath.GetSkinFile(obj);
            }
            if (temp.Item2)
            {
                file1 = AssetsPath.GetCapeFile(obj);
            }
        }

        if (file == null)
        {
            HeadBitmap = new Bitmap(asset);
        }
        else
        {
            SkinImage = await Image.LoadAsync<Rgba32>(file);
            var data = await ImageUtils.MakeHeadImage(file);
            if (file == null)
            {
                HeadBitmap = new Bitmap(asset);
            }
            else
            {
                data.Seek(0, SeekOrigin.Begin);
                HeadBitmap = new Bitmap(data);
                data.Close();
            }
        }

        if (file1 != null)
        {
            CapeIamge = await Image.LoadAsync<Rgba32>(file1);
        }

        App.OnSkinLoad();
    }

    public static void EditSkin(IBaseWindow window)
    {
        if (window is TopLevel top)
        {
            EditSkin(top);
        }
    }

    public static async void EditSkin(TopLevel window)
    {
        var obj = GetLastUser();
        if (obj == null)
            return;

        switch (obj.AuthType)
        {
            case AuthType.Offline:
                var file = await BaseBinding.OpFile(window,
                    App.GetLanguage("UserBinding.Info1"),
                    new string[] { "*.png" },
                    App.GetLanguage("UserBinding.Info2"));
                if (file?.Any() == true)
                {
                    var item = file[0];
                    obj.SaveSkin(item.GetPath());
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
            var item = AuthDatabase.Auths.First();
            SetLastUser(item.Key.Item1, item.Key.Item2);
        }
    }

    public static void OAuthCancel()
    {
        BaseAuth.CancelWithOAuth();
    }
}
