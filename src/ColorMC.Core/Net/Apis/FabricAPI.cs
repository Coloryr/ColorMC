using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Loader;
using Newtonsoft.Json;

namespace ColorMC.Core.Net.Apis;

public static class FabricAPI
{
    private static List<string>? SupportVersion;

    /// <summary>
    /// 获取元数据
    /// </summary>
    public static async Task<FabricMetaObj?> GetMeta(SourceLocal? local = null)
    {
        try
        {
            string url = UrlHelper.FabricMeta(local);
            var data = await BaseClient.GetString(url);
            if (data.Item1 == false)
            {
                ColorMCCore.OnError?.Invoke(LanguageHelper.GetName("Core.Http.Error7"),
                    new Exception(url), false);
                return null;
            }
            return JsonConvert.DeserializeObject<FabricMetaObj>(data.Item2!);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.GetName("Core.Http.Download.Fabric.Error1"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取加载器数据
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="version">fabric版本</param>
    public static async Task<FabricLoaderObj?> GetLoader(string mc, string version, SourceLocal? local = null)
    {
        try
        {
            string url = $"{UrlHelper.FabricMeta(local)}/loader/{mc}/{version}/profile/json";
            var data = await BaseClient.GetString(url);
            if (data.Item1 == false)
            {
                ColorMCCore.OnError?.Invoke(LanguageHelper.GetName("Core.Http.Error7"),
                    new Exception(url), false);
                return null;
            }
            return JsonConvert.DeserializeObject<FabricLoaderObj>(data.Item2!);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.GetName("Core.Http.Fabric.Error1"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取加载器版本
    /// </summary>
    /// <param name="mc">游戏版本</param>
    public static async Task<List<string>?> GetLoaders(string mc, SourceLocal? local = null)
    {
        try
        {
            string url = $"{UrlHelper.FabricMeta(local)}/loader/{mc}";
            var data = await BaseClient.GetString(url);
            if (data.Item1 == false)
            {
                ColorMCCore.OnError?.Invoke(LanguageHelper.GetName("Core.Http.Error7"),
                    new Exception(url), false);
                return null;
            }

            var list = JsonConvert.DeserializeObject<List<FabricLoaderObj1>>(data.Item2!);
            if (list == null)
                return null;

            var list1 = new List<string>();
            foreach (var item in list)
            {
                list1.Add(item.loader.version);
            }
            return list1;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.GetName("Core.Http.Fabric.Error2"), e);
            return null;
        }
    }

    /// <summary>
    /// 获取支持的版本
    /// </summary>
    public static async Task<List<string>?> GetSupportVersion(SourceLocal? local = null)
    {
        try
        {
            if (SupportVersion != null)
                return SupportVersion;

            string url = $"{UrlHelper.FabricMeta(local)}/game";
            var data = await BaseClient.GetString(url);
            if (data.Item1 == false)
            {
                ColorMCCore.OnError?.Invoke(LanguageHelper.GetName("Core.Http.Error7"),
                    new Exception(url), false);
                return null;
            }
            var list = JsonConvert.DeserializeObject<List<FabricMetaObj.Game>>(data.Item2!);
            if (list == null)
                return null;

            var list1 = new List<string>();
            foreach (var item in list)
            {
                list1.Add(item.version);
            }

            SupportVersion = list1;

            return list1;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.GetName("Core.Http.Fabric.Error3"), e);
            return null;
        }
    }
}
