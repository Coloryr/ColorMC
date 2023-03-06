using ColorMC.Core.Objs.FTB;
using Newtonsoft.Json;
using System.Text;

namespace ColorMC.Core.Net.Apis;

public static class FTBHelper
{
    private const string All = "https://api.modpacks.ch/public/modpack/all";
    private const string Featured = "https://api.modpacks.ch/public/modpack/featured/50";
    private const string Popular = "https://api.modpacks.ch/public/modpack/popular/plays/50";
    private const string Installs = "https://api.modpacks.ch/public/modpack/popular/installs/50";
    private const string Search = "https://api.modpacks.ch/public/modpack/search/50?term=";
    private const string ModPack = "https://api.modpacks.ch/public/modpack/";

    public static async Task<FTBPackListObj?> GetAll()
    {
        try
        {
            var data = await BaseClient.GetString(All);
            var obj = JsonConvert.DeserializeObject<FTBPackListObj>(data)!;

            return obj;
        }
        catch (Exception e)
        {
            ColorMCCore.OnError?.Invoke("获取FTB列表错误", e, false);
        }
        return null;
    }

    public static async Task<FTBPackListObj?> GetFeatured()
    {
        try
        {
            var data = await BaseClient.GetString(Featured);
            var obj = JsonConvert.DeserializeObject<FTBPackListObj>(data)!;

            return obj;
        }
        catch (Exception e)
        {
            ColorMCCore.OnError?.Invoke("获取FTB列表错误", e, false);
        }
        return null;
    }

    public static async Task<FTBPackListObj?> GetPopular()
    {
        try
        {
            var data = await BaseClient.GetString(Popular);
            var obj = JsonConvert.DeserializeObject<FTBPackListObj>(data)!;

            return obj;
        }
        catch (Exception e)
        {
            ColorMCCore.OnError?.Invoke("获取FTB列表错误", e, false);
        }
        return null;
    }

    public static async Task<FTBPackListObj?> GetInstalls()
    {
        try
        {
            var data = await BaseClient.GetString(Installs);
            var obj = JsonConvert.DeserializeObject<FTBPackListObj>(data)!;

            return obj;
        }
        catch (Exception e)
        {
            ColorMCCore.OnError?.Invoke("获取FTB列表错误", e, false);
        }
        return null;
    }

    public static async Task<FTBPackListObj?> GetSearch(string temp)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(temp) || temp.Length < 3)
                return null;

            var data = await BaseClient.GetString(Search + temp);
            var obj = JsonConvert.DeserializeObject<FTBPackListObj>(data)!;

            return obj;
        }
        catch (Exception e)
        {
            ColorMCCore.OnError?.Invoke("获取FTB列表错误", e, false);
        }
        return null;
    }

    public static async Task<FTBModpackObj?> GetModpack(int id)
    {
        try
        {
            var data = await BaseClient.GetString(ModPack + id);
            var obj = JsonConvert.DeserializeObject<FTBModpackObj>(data)!;

            return obj;
        }
        catch (Exception e)
        {
            ColorMCCore.OnError?.Invoke("获取FTB列表错误", e, false);
        }
        return null;
    }

    public static async Task<FTBFilesObj?> GetFiles(int id, int fid)
    {
        try
        {
            var data = await BaseClient.GetString(ModPack + id + "/" + fid);
            var obj = JsonConvert.DeserializeObject<FTBFilesObj>(data)!;

            return obj;
        }
        catch (Exception e)
        {
            ColorMCCore.OnError?.Invoke("获取FTB列表错误", e, false);
        }
        return null;
    }

    public static string GetString(this List<FTBModpackObj.Authors> list)
    {
        if (list == null)
            return "";
        var str = new StringBuilder();
        list.ForEach(item => str.Append(item.name).Append(","));
        if (str.Length > 1)
            str.Remove(str.Length - 1, 1);
        return str.ToString();
    }

    public static async void PostIntall(int id, int file)
    {
        var analytics = ModPack + id + "/" + file + "/install";

        try
        {
            var data = await BaseClient.GetString(analytics);
        }
        catch (Exception e)
        {
            ColorMCCore.OnError?.Invoke("FTB POST错误", e, false);
        }
    }
}
