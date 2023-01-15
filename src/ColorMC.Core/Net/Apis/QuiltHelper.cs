using ColorMC.Core.Objs.Loader;
using ColorMC.Core.Utils;
using Newtonsoft.Json;

namespace ColorMC.Core.Net.Apis;

public static class QuiltHelper
{
    public static async Task<List<string>?> GetSupportVersion(SourceLocal? local = null)
    {
        try
        {
            string url = $"{UrlHelper.QuiltMeta(local)}/game";
            var data = await BaseClient.GetString(url);
            if (string.IsNullOrWhiteSpace(data))
                return null;

            var list = JsonConvert.DeserializeObject<List<QuiltMetaObj.Game>>(data);
            if (list == null)
                return null;

            var list1 = new List<string>();
            foreach (var item in list)
            {
                list1.Add(item.version);
            }

            return list1;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.GetName("Core.Http.Get.Quilt.Error1"), e);
            return null;
        }
    }
    public static async Task<QuiltMetaObj?> GetMeta(SourceLocal? local = null)
    {
        try
        {
            var data = await BaseClient.GetString(UrlHelper.QuiltMeta(local));
            if (string.IsNullOrWhiteSpace(data))
                return null;
            return JsonConvert.DeserializeObject<QuiltMetaObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.GetName("Core.Http.Get.Quilt.Error2"), e);
            return null;
        }
    }

    public static async Task<List<string>?> GetLoaders(string mc, SourceLocal? local = null)
    {
        try
        {
            string url = $"{UrlHelper.FabricMeta(local)}/loader/{mc}";
            var data = await BaseClient.GetString(url);
            if (string.IsNullOrWhiteSpace(data))
                return null;

            var list = JsonConvert.DeserializeObject<List<FabricLoaderObj1>>(data);
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
            Logs.Error(LanguageHelper.GetName("Core.Http.Get.Quilt.Error3"), e);
            return null;
        }
    }

    public static async Task<QuiltLoaderObj?> GetLoader(string mc, string version, SourceLocal? local = null)
    {
        try
        {
            string url = $"{UrlHelper.QuiltMeta(local)}/loader/{mc}/{version}/profile/json";
            var data = await BaseClient.GetString(url);
            if (string.IsNullOrWhiteSpace(data))
                return null;
            return JsonConvert.DeserializeObject<QuiltLoaderObj>(data);
        }
        catch (Exception e)
        {
            Logs.Error(LanguageHelper.GetName("Core.Http.Get.Quilt.Error4"), e);
            return null;
        }
    }
}
