using System.Collections.Concurrent;
using ColorMC.Core.Config;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Config;
using ColorMC.Core.Objs.Login;

namespace ColorMC.Core.Utils;

/// <summary>
/// 账户数据库
/// </summary>
public static class AuthDatabase
{
    /// <summary>
    /// 保存的账户
    /// </summary>
    private static readonly ConcurrentDictionary<UserKeyObj, LoginObj> s_auths = new();

    /// <summary>
    /// 获取账户列表
    /// </summary>
    public static Dictionary<UserKeyObj, LoginObj> Auths => new(s_auths);

    /// <summary>
    /// 账户数据库保存位置
    /// </summary>
    private static string s_local;

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        //存在用户文件夹
        var path = SystemInfo.Os == OsType.MacOS ?
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ColorMC") :
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ColorMC");

        Directory.CreateDirectory(path);

        s_local = Path.Combine(path, Names.NameAuthFile);

        Logs.Info(string.Format(LanguageHelper.Get("Core.Auth.Info1"), s_local));

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
            using var stream = PathHelper.OpenRead(s_local);
            var list = JsonUtils.ToObj(stream, JsonType.ListLoginObj);
            if (list == null)
            {
                return;
            }

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
        ConfigSave.AddItem(ConfigSaveObj.Build(Names.NameAuthFile, s_local, [.. s_auths.Values], JsonType.ListLoginObj));
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
        var data = PathHelper.OpenRead(dir);
        var list = JsonUtils.ToObj(data, JsonType.ListLoginObj);
        if (list == null)
        {
            return false;
        }

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

    /// <summary>
    /// 清空账户
    /// </summary>
    public static void ClearAuths()
    {
        s_auths.Clear();
        Save();
    }
}
