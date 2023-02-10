using ColorMC.Core.Game.Auth;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Minecraft;
using Newtonsoft.Json;

namespace ColorMC.Core.Utils;

public static class AuthDatabase
{
    public static readonly Dictionary<(string, AuthType), LoginObj> Auths = new();

    private const string Name = "auth.json";
    private static string Dir;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行的路径</param>
    public static void Init(string dir)
    {
        Logs.Info(LanguageHelper.GetName("Core.Auth"));

        Dir = Path.GetFullPath(dir + Name);
        var data = File.ReadAllText(Dir);
        var list = JsonConvert.DeserializeObject<List<LoginObj>>(data);
        if (list == null)
            return;

        foreach (var item in list)
        {
            Auths.Add((item.UUID, item.AuthType), item);
        }
    }

    /// <summary>
    /// 保存
    /// </summary>
    private static void Save()
    {
        Task.Run(() =>
        {
            var file = JsonConvert.SerializeObject(Auths.Values);
            File.WriteAllText(Dir, file);
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
            Auths.Add((obj.UUID, obj.AuthType), obj);
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
        Auths.Remove((obj.UUID, obj.AuthType));
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
                Auths.Add((item.UUID, item.AuthType), item);
            }
        }

        return true;
    }
}
