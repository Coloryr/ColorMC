using System.Collections.Concurrent;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using Newtonsoft.Json;

namespace ColorMC.Core.Utils;

/// <summary>
/// 账户数据库
/// </summary>
public static class AuthDatabase
{
    private static readonly ConcurrentDictionary<UserKeyObj, LoginObj> s_auths = new();

    public static Dictionary<UserKeyObj, LoginObj> Auths => new(s_auths);

    public const string Name = "auth.json";
    private static string s_local;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行的路径</param>
    public static void Init()
    {
        Logs.Info(LanguageHelper.Get("Core.Auth.Info1"));

        var path = (SystemInfo.Os == OsType.MacOS ?
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.ColorMC/" :
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)) + "/ColorMC/";

        Logs.Info(path);

        Directory.CreateDirectory(path);

        s_local = Path.GetFullPath(path + Name);

        if (SystemInfo.Os == OsType.Windows)
        {
            var path1 = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/ColorMC/" + Name);
            if (File.Exists(path1) && !File.Exists(s_local))
            {
                File.Move(path1, s_local);
            }
        }

        if (File.Exists(s_local))
        {
            Load();
        }
        else
        {
            Save();
        }
    }

    /// <summary>
    /// 加载保存的账户
    /// </summary>
    private static void Load()
    {
        try
        {
            var data = PathHelper.ReadText(s_local)!;
            var list = JsonConvert.DeserializeObject<List<LoginObj>>(data);
            if (list == null)
                return;

            foreach (var item in list)
            {
                s_auths.TryAdd(item.GetKey(), item);
            }
        }
        catch
        {
            Save();
        }
    }

    /// <summary>
    /// 保存
    /// </summary>
    public static void Save()
    {
        ConfigSave.AddItem(new()
        {
            Name = "auth.json",
            Obj = s_auths.Values,
            Local = s_local
        });
    }

    /// <summary>
    /// 保存
    /// </summary>
    /// <param name="obj">保存的账户</param>
    public static void Save(this LoginObj obj)
    {
        if (string.IsNullOrWhiteSpace(obj.UUID))
        {
            return;
        }

        var key = obj.GetKey();

        if (s_auths.ContainsKey(key))
        {
            s_auths[key] = obj;
        }
        else
        {
            s_auths.TryAdd(key, obj);
        }

        Save();
    }

    /// <summary>
    /// 获取账户
    /// </summary>
    public static LoginObj? Get(string uuid, AuthType type)
    {
        if (s_auths.TryGetValue(new() { UUID = uuid, Type = type }, out var item))
        {
            return item;
        }

        return null;
    }

    /// <summary>
    /// 删除账户
    /// </summary>
    public static void Delete(this LoginObj obj)
    {
        s_auths.TryRemove(obj.GetKey(), out _);
        Save();
    }

    /// <summary>
    /// 导入账户
    /// </summary>
    public static bool LoadData(string dir)
    {
        var data = PathHelper.ReadText(dir);
        if (data == null)
            return false;
        var list = JsonConvert.DeserializeObject<List<LoginObj>>(data);
        if (list == null)
            return false;

        foreach (var item in list)
        {
            var key = item.GetKey();

            if (s_auths.ContainsKey(key))
            {
                s_auths[key] = item;
            }
            else
            {
                s_auths.TryAdd(key, item);
            }
        }

        return true;
    }

    public static void ClearAuths()
    {
        s_auths.Clear();
        Save();
    }
}
