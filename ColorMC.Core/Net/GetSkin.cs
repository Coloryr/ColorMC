using ColorMC.Core.Game.Auth;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Net.Login;
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

namespace ColorMC.Core.Net;

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

        try
        {
            string file = $"{AssetsPath.BaseDir}/{AssetsPath.Name3}/{obj.UUID[0..2]}/{obj.UUID}";
            file = Path.GetFullPath(file);
            FileInfo info = new(file);
            info.Directory.Create();
            var data2 = await BaseClient.GetBytes(url);
            File.WriteAllBytes(file, data2);
            return file;
        }
        catch (Exception e)
        {
            Logs.Error("获取头像错误", e);
        }

        return null;
    }

    private static async Task<string?> BaseLoad(string uuid, string? url = null)
    {
        try
        {
            var res = await MinecraftAPI.GetUserProfile(uuid, url);
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

    public static Task<string?> LoadFromMinecraft(LoginObj obj)
    {
        return BaseLoad(obj.UUID);
    }

    public static Task<string?> LoadFromNide8(LoginObj obj)
    {
        return BaseLoad(obj.UUID, $"https://auth.mc-user.com:233/{obj.Text1}/sessionserver/session/minecraft/profile/{obj.UUID}");
    }

    public static Task<string?> LoadFromAuthlibInjector(LoginObj obj)
    {
        return BaseLoad(obj.UUID, $"{obj.Text1}/session/minecraft/profile/{obj.UUID}");
    }

    public static Task<string?> LoadFromLittleskin(LoginObj obj)
    {
        return BaseLoad(obj.UUID, $"https://littleskin.cn/api/yggdrasil/session/minecraft/profile/{obj.UUID}");
    }

    public static Task<string?> LoadFromSelfLittleskin(LoginObj obj)
    {
        return BaseLoad(obj.UUID, $"{obj.Text1}/api/yggdrasil/session/minecraft/profile/{obj.UUID}");
    }
}
