using ColorMC.Core.Game.Auth;
using ColorMC.Core.Http.Apis;
using ColorMC.Core.Http.Login;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.MinecraftAPI;
using Heijden.DNS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Http.Skin;

public static class GetSkin
{
    public static async Task<string?> DownloadSkin(LoginObj obj)
    {
        if (obj.AuthType == AuthType.Offline)
            return null;

        string? url = obj.AuthType switch
        {
            AuthType.OAuth => await LoadFromMinecraft(obj),
            AuthType.Nide8 => await LoadFromNide8(obj),
            AuthType.AuthlibInjector => await LoadFromAuthlibInjector(obj),
            AuthType.LittleSkin => await LoadFromLittleskin(obj),
            AuthType.SelfLittleSkin => await LoadFromSelfLittleskin(obj),
            _ => null
        };

        if (url == null)
            return null;

        string file = $"{AssetsPath.BaseDir}/{AssetsPath.Name3}/{obj.UUID[0..2]}/{obj.UUID}";
        file = Path.GetFullPath(file);
        FileInfo info = new(file);
        info.Directory.Create();
        var data2 = await BaseClient.GetBytes(url);
        File.WriteAllBytes(file, data2);

        return file;
    }
    public static async Task<string?> LoadFromMinecraft(LoginObj obj)
    {
        try
        {
            var res = await MinecraftAPI.GetUserProfile(obj.UUID);
            if (res == null)
                return null;
            if (res.properties.Count == 0)
                return null;
            var data = Convert.FromBase64String(res.properties[0].value);
            var data1 = Encoding.UTF8.GetString(data);
            var obj1 = JsonConvert.DeserializeObject<TexturesObj>(data1);
            if (obj1 == null)
                return null;
            return obj1.textures.SKIN.url;
        }
        catch (Exception e)
        {
            Logs.Error("皮肤读取失败", e);
            return null;
        }
    }

    public static async Task<string?> LoadFromNide8(LoginObj obj)
    {
        try
        {
            var res = await MinecraftAPI.GetUserProfile(null, $"https://login.mc-user.com:233/{obj.Text1}/session/minecraft/profile/{obj.UUID}");
            if (res == null)
                return null;
            if (res.properties.Count == 0)
                return null;
            var data = Convert.FromBase64String(res.properties[0].value);
            var data1 = Encoding.UTF8.GetString(data);
            var obj1 = JsonConvert.DeserializeObject<TexturesObj>(data1);
            if (obj1 == null)
                return null;
            return obj1.textures.SKIN.url;
        }
        catch (Exception e)
        {
            Logs.Error("皮肤读取失败", e);
            return null;
        }
    }

    public static async Task<string?> LoadFromAuthlibInjector(LoginObj obj)
    {
        try
        {
            var res = await MinecraftAPI.GetUserProfile(null, $"{obj.Text1}/session/minecraft/profile/{obj.UUID}");
            if (res == null)
                return null;
            if (res.properties.Count == 0)
                return null;
            var data = Convert.FromBase64String(res.properties[0].value);
            var data1 = Encoding.UTF8.GetString(data);
            var obj1 = JsonConvert.DeserializeObject<TexturesObj>(data1);
            if (obj1 == null)
                return null;
            return obj1.textures.SKIN.url;
        }
        catch (Exception e)
        {
            Logs.Error("皮肤读取失败", e);
            return null;
        }
    }

    public static async Task<string?> LoadFromLittleskin(LoginObj obj)
    {
        try
        {
            var res = await MinecraftAPI.GetUserProfile(null, $"https://littleskin.cn/api/yggdrasil/session/minecraft/profile/{obj.UUID}");
            if (res == null)
                return null;
            if (res.properties.Count == 0)
                return null;
            var data = Convert.FromBase64String(res.properties[0].value);
            var data1 = Encoding.UTF8.GetString(data);
            var obj1 = JsonConvert.DeserializeObject<TexturesObj>(data1);
            if (obj1 == null)
                return null;
            return obj1.textures.SKIN.url;
        }
        catch (Exception e)
        {
            Logs.Error("皮肤读取失败", e);
            return null;
        }
    }

    public static async Task<string?> LoadFromSelfLittleskin(LoginObj obj)
    {
        try
        {
            var res = await MinecraftAPI.GetUserProfile(null, $"{obj.Text1}/api/yggdrasil/session/minecraft/profile/{obj.UUID}");
            if (res == null)
                return null;
            if (res.properties.Count == 0)
                return null;
            var data = Convert.FromBase64String(res.properties[0].value);
            var data1 = Encoding.UTF8.GetString(data);
            var obj1 = JsonConvert.DeserializeObject<TexturesObj>(data1);
            if (obj1 == null)
                return null;
            return obj1.textures.SKIN.url;
        }
        catch (Exception e)
        {
            Logs.Error("皮肤读取失败", e);
            return null;
        }
    }
}
