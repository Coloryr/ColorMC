using ColorMC.Core.Game.Auth;
using ColorMC.Core.Objs.Game;
using ColorMC.Core.Objs.Minecraft;
using Dapper;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace ColorMC.Core.Utils;

public record QLogin
{
    public string UserName { get; set; }
    public string UUID { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public string ClientToken { get; set; }
    public AuthType AuthType { get; set; }
    public string Properties { get; set; }
    public string Text1 { get; set; }
    public string Text2 { get; set; }
}

public static class AuthDatabase
{
    private static readonly string DB = "Auth.db";
    private static readonly Dictionary<string, LoginObj> Auths = new();

    private static string connStr;
    public static SqliteConnection GetSqliteConnection()
    {
        return new SqliteConnection(connStr);
    }
    public static void Init()
    {
        Logs.Info($"登录数据库初始化");
        connStr = new SqliteConnectionStringBuilder("Data Source=" + DB)
        {
            Mode = SqliteOpenMode.ReadWriteCreate
        }.ToString();

        using var conn = GetSqliteConnection();

        conn.Execute(@"create table if not exists auth (
    `id` integer NOT NULL PRIMARY KEY AUTOINCREMENT,
    `UserName` text,
    `UUID` text,
    `AccessToken` text,
    `RefreshToken` text,
    `ClientToken` text,
    `AuthType` integer,
    `Properties` text,
    `Text1` text,
    `Text2` text
);");

        GetAll();
    }

    public static void SaveAuth(LoginObj obj)
    {
        if (string.IsNullOrWhiteSpace(obj.UUID))
        {
            return;
        }

        if (Auths.ContainsKey(obj.UUID))
        {
            Auths[obj.UUID] = obj;
            Update(obj);
        }
        else
        {
            Auths.Add(obj.UUID, obj);
            Add(obj);
        }
    }

    public static LoginObj? Get(string uuid)
    {
        if (Auths.TryGetValue(uuid, out var item))
        {
            return item;
        }

        return null;
    }

    private static void GetAll()
    {
        using var sql = GetSqliteConnection();
        var list = sql.Query<QLogin>("SELECT UserName,UUID,AccessToken," +
            "RefreshToken,ClientToken," +
            "AuthType,Properties,Text1,Text2 FROM auth");

        foreach (var item in list)
        {
            Auths.Add(item.UUID, item.ToLogin());
        }
    }

    private static void Add(LoginObj obj)
    {
        using var sql = GetSqliteConnection();
        sql.Execute("INSERT INTO auth(`UserName`,`UUID`,`AccessToken`,`RefreshToken`,`ClientToken`," +
            "`AuthType`,`Properties`,`Text1`,`Text2`) " +
            "VALUES (@UserName,@UUID,@AccessToken,@RefreshToken,@ClientToken," +
            "@AuthType,@Properties,@Text1,@Text2)", obj.ToQLogin());
    }

    private static void Update(LoginObj obj)
    {
        using var sql = GetSqliteConnection();
        sql.Execute("UPDATE auth SET UserName=@UserName," +
            "AccessToken=@AccessToken,RefreshToken=@RefreshToken," +
            "ClientToken=@ClientToken,AuthType=@AuthType,Properties=@Properties," +
            "Text1=@Text1,Text2=@Text2 WHERE UUID=@UUID",
            obj.ToQLogin());
    }

    private static LoginObj ToLogin(this QLogin obj)
    {
        return new()
        {
            UserName = obj.UserName,
            UUID = obj.UUID,
            AccessToken = obj.AccessToken,
            RefreshToken = obj.RefreshToken,
            ClientToken = obj.ClientToken,
            AuthType = obj.AuthType,
            Properties = JsonConvert.DeserializeObject<List<UserPropertyObj>>(obj.Properties)!,
            Text1 = obj.Text1,
            Text2 = obj.Text2
        };
    }

    private static QLogin ToQLogin(this LoginObj obj)
    {
        obj.Properties ??= new();
        return new()
        {
            UserName = obj.UserName,
            UUID = obj.UUID,
            AccessToken = obj.AccessToken,
            RefreshToken = obj.RefreshToken,
            ClientToken = obj.ClientToken,
            AuthType = obj.AuthType,
            Properties = JsonConvert.SerializeObject(obj.Properties),
            Text1 = obj.Text1,
            Text2 = obj.Text2
        };
    }
}
