using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.MinecraftAPI;
using ColorMC.Core.Utils;
using Newtonsoft.Json;
using System.Text;

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
    public static async Task<(bool, bool)> DownloadSkin(LoginObj obj)
    {
        if (obj.AuthType == AuthType.Offline)
            return (false, false);

        TexturesObj? url = obj.AuthType switch
        {
            AuthType.OAuth => await LoadFromMinecraft(obj),
            AuthType.Nide8 => await LoadFromNide8(obj),
            AuthType.AuthlibInjector => await LoadFromAuthlibInjector(obj),
            AuthType.LittleSkin => await LoadFromLittleskin(obj),
            AuthType.SelfLittleSkin => await LoadFromSelfLittleskin(obj),
            _ => null
        };

        if (url == null)
            return (false, false);

        bool skin = false, cape = false;
        if (!string.IsNullOrWhiteSpace(url.textures.SKIN?.url))
        {
            try
            {
                var file = Path.GetFullPath(obj.GetSkinFile());
                FileInfo info = new(file);
                info.Directory?.Create();
                var data2 = await BaseClient.GetBytes(url.textures.SKIN.url);
                File.WriteAllBytes(file, data2);
                skin = true;
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.GetName("Core.Http.Error2"), e);
            }
        }

        if (!string.IsNullOrWhiteSpace(url.textures.CAPE?.url))
        {
            try
            {
                var file = Path.GetFullPath(obj.GetCapeFile());
                FileInfo info = new(file);
                info.Directory?.Create();
                var data2 = await BaseClient.GetBytes(url.textures.CAPE.url);
                File.WriteAllBytes(file, data2);
                cape = true;
            }
            catch (Exception e)
            {
                Logs.Error(LanguageHelper.GetName("Core.Http.Error2"), e);
            }
        }

        return (skin, cape);
    }

    /// <summary>
    /// 加载皮肤
    /// </summary>
    /// <param name="uuid">玩家UUID</param>
    /// <param name="url">网址</param>
    /// <returns></returns>
    private static async Task<TexturesObj?> BaseLoad(string uuid, string? url = null)
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
            return JsonConvert.DeserializeObject<TexturesObj>(data1);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.GetName("Core.Http.Error3"), e);
            return null;
        }
    }

    /// <summary>
    /// 从Mojang加载皮肤
    /// </summary>
    /// <param name="obj">账户</param>
    /// <returns></returns>
    private static Task<TexturesObj?> LoadFromMinecraft(LoginObj obj)
    {
        return BaseLoad(obj.UUID);
    }
    /// <summary>
    /// 从Nide8加载皮肤
    /// </summary>
    /// <param name="obj">账户</param>
    /// <returns></returns>
    private static Task<TexturesObj?> LoadFromNide8(LoginObj obj)
    {
        return BaseLoad(obj.UUID, $"{UrlHelper.Nide8}{obj.Text1}/sessionserver/session/minecraft/profile/{obj.UUID}");
    }
    /// <summary>
    /// 从外置登录加载皮肤
    /// </summary>
    /// <param name="obj">账户</param>
    /// <returns></returns>
    private static Task<TexturesObj?> LoadFromAuthlibInjector(LoginObj obj)
    {
        return BaseLoad(obj.UUID, $"{obj.Text1}/sessionserver/session/minecraft/profile/{obj.UUID}");
    }
    /// <summary>
    /// 从皮肤站加载皮肤
    /// </summary>
    /// <param name="obj">账户</param>
    /// <returns></returns>
    private static Task<TexturesObj?> LoadFromLittleskin(LoginObj obj)
    {
        return BaseLoad(obj.UUID, $"{UrlHelper.LittleSkin}api/yggdrasil/sessionserver/session/minecraft/profile/{obj.UUID}");
    }
    /// <summary>
    /// 从自建皮肤站加载皮肤
    /// </summary>
    /// <param name="obj">账户</param>
    /// <returns></returns>
    private static Task<TexturesObj?> LoadFromSelfLittleskin(LoginObj obj)
    {
        return BaseLoad(obj.UUID, $"{obj.Text1}/api/yggdrasil/sessionserver/session/minecraft/profile/{obj.UUID}");
    }
}
