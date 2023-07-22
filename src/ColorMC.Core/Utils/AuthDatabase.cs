using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Login;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace ColorMC.Core.Utils;

/// <summary>
/// 账户数据库
/// </summary>
public static class AuthDatabase
{
    public static readonly ConcurrentDictionary<(string, AuthType), LoginObj> Auths = new();

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
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) :
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))
            + "/ColorMC/";

        Logs.Info(path);

        Directory.CreateDirectory(path);

        s_local = Path.GetFullPath(path + Name);
        if (File.Exists(s_local))
        {
            Load();
        }
        else
        {
            string file1 = AppContext.BaseDirectory + Name;
            if (File.Exists(file1))
            {
                File.Move(file1, s_local);
            }
            else
            {
                Save();
            }
        }
    }

    /// <summary>
    /// 加载保存的账户
    /// </summary>
    private static void Load()
    {
        try
        {
            var data = File.ReadAllText(s_local);
            var list = JsonConvert.DeserializeObject<List<LoginObj>>(data);
            if (list == null)
                return;

            foreach (var item in list)
            {
                Auths.TryAdd((item.UUID, item.AuthType), item);
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
            Obj = Auths.Values,
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

        if (Auths.ContainsKey((obj.UUID, obj.AuthType)))
        {
            Auths[(obj.UUID, obj.AuthType)] = obj;
        }
        else
        {
            Auths.TryAdd((obj.UUID, obj.AuthType), obj);
        }

        Save();
    }

    /// <summary>
    /// 获取账户
    /// </summary>
    public static LoginObj? Get(string uuid, AuthType type)
    {
        if (Auths.TryGetValue((uuid, type), out var item))
        {
            return item;
        }

        return null;
    }

    /// <summary>
    /// 删除账户
    /// </summary>
    public static void Delete(LoginObj obj)
    {
        Auths.TryRemove((obj.UUID, obj.AuthType), out _);
        Save();
    }

    /// <summary>
    /// 导入账户
    /// </summary>
    public static bool LoadData(string dir)
    {
        var list = JsonConvert.DeserializeObject<List<LoginObj>>(dir);
        if (list == null)
            return false;

        foreach (var item in list)
        {
            if (Auths.ContainsKey((item.UUID, item.AuthType)))
            {
                Auths[(item.UUID, item.AuthType)] = item;
            }
            else
            {
                Auths.TryAdd((item.UUID, item.AuthType), item);
            }
        }

        return true;
    }
}
