using System.Text;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.MinecraftAPI;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// 获取皮肤
/// </summary>
public static class PlayerSkinAPI
{
    /// <summary>
    /// 下载皮肤和披风
    /// </summary>
    /// <param name="obj">保存的账户</param>
    /// <returns>皮肤路径, 披风路径</returns>
    public static async Task<DownloadSkinRes> DownloadSkin(LoginObj obj)
    {
        var url = obj.AuthType switch
        {
            AuthType.Offline => await LoadFromMinecraft(obj),
            AuthType.OAuth => await LoadFromMinecraft(obj),
            AuthType.Nide8 => await LoadFromNide8(obj),
            AuthType.AuthlibInjector => await LoadFromAuthlibInjector(obj),
            AuthType.LittleSkin => await LoadFromLittleskin(obj),
            AuthType.SelfLittleSkin => await LoadFromSelfLittleskin(obj),
            _ => null
        };

        if (url == null)
        {
            return new();
        }

        string? skin = null, cape = null;
        bool isslim = false;
        if (!string.IsNullOrWhiteSpace(url.Textures.Skin?.Url))
        {
            try
            {
                isslim = url.Textures.Skin.Metadata?.Model == "slim";
                var file = AssetsPath.GetSkinFile(url.Textures.Skin.Url);
                var data2 = await CoreHttpClient.GetBytesAsync(url.Textures.Skin.Url);

                if (data2.State)
                {
                    PathHelper.WriteBytes(file, data2.Data!);
                    skin = file;
                }
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.Get("Core.Http.Error2"), e);
            }
        }

        if (!string.IsNullOrWhiteSpace(url.Textures.Cape?.Url))
        {
            try
            {
                var file = AssetsPath.GetSkinFile(url.Textures.Cape.Url);
                var data2 = await CoreHttpClient.GetBytesAsync(url.Textures.Cape.Url);
                if (data2.State)
                {
                    PathHelper.WriteBytes(file, data2.Data!);
                    cape = file;
                }
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.Get("Core.Http.Error2"), e);
            }
        }

        return new() { Skin = skin, Cape = cape, IsNewSlim = isslim };
    }

    /// <summary>
    /// 加载皮肤
    /// </summary>
    /// <param name="uuid">玩家UUID</param>
    /// <param name="url">网址</param>
    /// <returns>皮肤内容</returns>
    private static async Task<MinecraftTexturesObj?> BaseLoad(string uuid, string? url = null)
    {
        try
        {
            var res = await MinecraftAPI.GetUserProfile(uuid, url);
            if (res == null)
                return null;
            if (res.Properties.Count == 0)
                return null;
            var data = Convert.FromBase64String(res.Properties[0].Value);
            var data1 = Encoding.UTF8.GetString(data);
            return JsonUtils.ToObj(data1, JsonType.MinecraftTexturesObj);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.Get("Core.Http.Error3"), e);
            return null;
        }
    }

    /// <summary>
    /// 从Mojang加载皮肤
    /// </summary>
    /// <param name="obj">账户</param>
    /// <returns></returns>
    private static Task<MinecraftTexturesObj?> LoadFromMinecraft(LoginObj obj)
    {
        return BaseLoad(obj.UUID);
    }
    /// <summary>
    /// 从Nide8加载皮肤
    /// </summary>
    /// <param name="obj">账户</param>
    /// <returns></returns>
    private static Task<MinecraftTexturesObj?> LoadFromNide8(LoginObj obj)
    {
        return BaseLoad(obj.UUID, $"{UrlHelper.Nide8}{obj.Text1}/sessionserver/session/minecraft/profile/{obj.UUID}");
    }
    /// <summary>
    /// 从外置登录加载皮肤
    /// </summary>
    /// <param name="obj">账户</param>
    /// <returns></returns>
    private static Task<MinecraftTexturesObj?> LoadFromAuthlibInjector(LoginObj obj)
    {
        return BaseLoad(obj.UUID, $"{obj.Text1}/sessionserver/session/minecraft/profile/{obj.UUID}");
    }
    /// <summary>
    /// 从皮肤站加载皮肤
    /// </summary>
    /// <param name="obj">账户</param>
    /// <returns></returns>
    private static Task<MinecraftTexturesObj?> LoadFromLittleskin(LoginObj obj)
    {
        return BaseLoad(obj.UUID, $"{UrlHelper.LittleSkin}api/yggdrasil/sessionserver/session/minecraft/profile/{obj.UUID}");
    }
    /// <summary>
    /// 从自建皮肤站加载皮肤
    /// </summary>
    /// <param name="obj">账户</param>
    /// <returns></returns>
    private static Task<MinecraftTexturesObj?> LoadFromSelfLittleskin(LoginObj obj)
    {
        return BaseLoad(obj.UUID, $"{obj.Text1}/api/yggdrasil/sessionserver/session/minecraft/profile/{obj.UUID}");
    }
}
