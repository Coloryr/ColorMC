using System.Text.Json;
using System.Xml;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Core.Net.Apis;

/// <summary>
/// Forge网络请求
/// </summary>
public static class ForgeAPI
{
    //支持的版本与加载器版本
    private static readonly HashSet<string> s_forgeSupportVersion = [];
    private static readonly Dictionary<string, HashSet<string>> s_forgeVersion = [];

    private static readonly HashSet<string> s_neoSupportVersion = [];
    private static readonly Dictionary<string, HashSet<string>> s_neoForgeVersion = [];

    /// <summary>
    /// 获取支持的版本
    /// </summary>
    /// <returns></returns>
    public static async Task<HashSet<string>?> GetSupportVersionAsync(bool neo, SourceLocal? local = null)
    {
        if ((neo ? s_neoSupportVersion.Count != 0 : s_forgeSupportVersion.Count != 0))
        {
            return neo ? s_neoSupportVersion : s_forgeSupportVersion;
        }

        if (local == SourceLocal.BMCLAPI)
        {
            if (neo ? await LoadFromSourceAsync(true) : await GetSupportVersionBmclAsync(local) != true)
            {
                return null;
            }
        }
        else
        {
            if (await LoadFromSourceAsync(neo) != true)
            {
                return null;
            }
        }

        return neo ? s_neoSupportVersion : s_forgeSupportVersion;
    }

    private static async Task<bool> GetSupportVersionBmclAsync(SourceLocal? local = null)
    {
        string url = UrlHelper.ForgeVersion(local);
        try
        {
            using var stream = await CoreHttpClient.GetStreamAsync(url);
            var obj = JsonUtils.ToObj(stream, JsonType.HashSetString);
            if (obj == null)
            {
                return false;
            }

            s_forgeSupportVersion.Clear();

            StringHelper.VersionSort(obj);

            foreach (var item in obj)
            {
                s_forgeSupportVersion.Add(item);
            }

            return true;
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(url, e));
            return false;
        }
    }

    /// <summary>
    /// 获取版本列表
    /// </summary>
    /// <param name="mc">游戏版本</param>
    /// <param name="source">下载源</param>
    /// <returns>版本列表</returns>
    public static async Task<HashSet<string>?> GetVersionListAsync(bool neo, string mc, SourceLocal? source = null)
    {
        source ??= CoreHttpClient.Source;

        if (source == SourceLocal.BMCLAPI)
        {
            string url = neo
                    ? UrlHelper.NeoForgeVersions(mc, source)
                    : UrlHelper.ForgeVersions(mc, source);
            try
            {
                using var stream = await CoreHttpClient.GetStreamAsync(url);

                var list1 = new HashSet<string>();
                if (neo)
                {
                    var obj = JsonUtils.ToObj(stream, JsonType.ListNeoForgeVersionBmclApiObj);
                    if (obj == null)
                        return null;

                    foreach (var item in obj)
                    {
                        list1.Add(item.Version.Replace("neoforge-", ""));
                    }
                }
                else
                {
                    var obj = JsonUtils.ToObj(stream, JsonType.ListForgeVersionBmclApiObj);
                    if (obj == null)
                        return null;

                    foreach (var item in obj)
                    {
                        list1.Add(item.Version);
                    }
                }

                StringHelper.VersionSort(list1);

                return list1;
            }
            catch (Exception e)
            {
                ColorMCCore.OnError(new ApiRequestErrorEventArgs(url, e));
                return null;
            }
        }
        else
        {
            if (!neo && s_forgeVersion.TryGetValue(mc, out var list1))
            {
                var list2 = new HashSet<string>();
                foreach (var item in list1)
                {
                    list2.Add(item.ToString());
                }
                return list2;
            }
            else if (neo && s_neoForgeVersion.TryGetValue(mc, out list1))
            {
                var list2 = new HashSet<string>();
                foreach (var item in list1)
                {
                    list2.Add(item.ToString());
                }
                return list2;
            }

            await LoadFromSourceAsync(neo);

            if (neo)
            {
                if (s_neoForgeVersion.TryGetValue(mc, out var list4))
                {
                    var list2 = new HashSet<string>();
                    foreach (var item in list4)
                    {
                        list2.Add(item.ToString());
                    }
                    return list2;
                }
            }
            else
            {
                if (s_forgeVersion.TryGetValue(mc, out var list4))
                {
                    var list2 = new HashSet<string>();
                    foreach (var item in list4)
                    {
                        list2.Add(item.ToString());
                    }
                    return list2;
                }
            }
            return null;
        }
    }

    /// <summary>
    /// 从xml获取信息
    /// </summary>
    /// <param name="neo">是否为NeoForge</param>
    /// <returns></returns>
    private static async Task<bool> LoadFromSourceAsync(bool neo)
    {
        var url = neo ? UrlHelper.NeoForgeVersions("", SourceLocal.Offical) :
                    UrlHelper.ForgeVersion(SourceLocal.Offical);
        //旧版neoforge与forge
        try
        {
            if (!neo)
            {
                using var stream = await CoreHttpClient.GetStreamAsync(url);
                if (stream == null)
                {
                    return false;
                }
                var xml = new XmlDocument();
                xml.Load(stream);

                var node = xml.SelectNodes("//metadata/versioning/versions/version");
                if (node == null)
                {
                    return false;
                }

                s_forgeSupportVersion.Clear();
                s_forgeVersion.Clear();
                foreach (XmlNode item in node)
                {
                    var str = item.InnerText;
                    var args = str.Split('-');
                    if (args.Length < 2)
                    {
                        continue;
                    }
                    var mc1 = args[0];
                    var version = args[1];

                    s_forgeSupportVersion.Add(mc1);

                    if (s_forgeVersion.TryGetValue(mc1, out var list2))
                    {
                        list2.Add(new(version));
                    }
                    else
                    {
                        var list3 = new HashSet<string>() { version };
                        s_forgeVersion.Add(mc1, list3);
                    }
                }
                foreach (var item in s_forgeVersion.Values)
                {
                    StringHelper.VersionSort(item);
                }

                return true;
            }
            else
            {
                using var stream1 = await CoreHttpClient.GetStreamAsync(url);
                if (stream1 == null)
                {
                    return false;
                }

                var json = await JsonDocument.ParseAsync(stream1);

                if (json.RootElement.TryGetProperty("versions", out var versions) && versions.ValueKind == JsonValueKind.Array)
                {
                    s_neoForgeVersion.Clear();
                    s_neoSupportVersion.Clear();

                    string GetFirstTwoVersionNumbers(string versionString)
                    {
                        var splitVersion = versionString.Split('.');
                        return $"{splitVersion[0]}.{splitVersion[1]}";
                    }

                    string GetMcVersionFromNeoForgeVersion(string versionString)
                    {
                        var spl = versionString.Split('.');
                        // Handle the new versioning scheme first
                        if (int.Parse(spl[0]) >= 26)
                        {
                            // 26.1.0.X -> 26.1
                            var mcVersion = spl[0] + '.' + spl[1];
                            // 26.1.1.X -> 26.1.1
                            if (spl[2] != "0")
                            {
                                mcVersion += '.' + spl[2];
                            }

                            // 26.1.0.0-alpha+snapshot-1
                            var splitBySnapshotIdentifier = versionString.Split('+');
                            if (splitBySnapshotIdentifier.Length == 2)
                            {
                                mcVersion += '-' + splitBySnapshotIdentifier[1];
                            }
                            return mcVersion;
                        }
                        return "1." + GetFirstTwoVersionNumbers(versionString);
                    }

                    foreach (var item in versions.EnumerateArray())
                    {
                        var neoVersion = item.GetString() ?? "";
                        var mcVersion = GetMcVersionFromNeoForgeVersion(neoVersion);

                        // Remove 0.25w14craftmine and other april fools versions
                        if (neoVersion.StartsWith('0')) continue;

                        // Get and push version lists
                        s_neoSupportVersion.Add(mcVersion);

                        if (s_neoForgeVersion.TryGetValue(mcVersion, out var list2))
                        {
                            list2.Add(neoVersion);
                        }
                        else
                        {
                            var list3 = new HashSet<string>() { neoVersion };
                            s_neoForgeVersion.Add(mcVersion, list3);
                        }
                    }

                    foreach (var item in s_neoForgeVersion.Values)
                    {
                        StringHelper.VersionSort(item);
                    }
                }

                return true;
            }
        }
        catch (Exception e)
        {
            ColorMCCore.OnError(new ApiRequestErrorEventArgs(url, e));
            return false;
        }
    }
}
