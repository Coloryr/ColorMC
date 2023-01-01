using ColorMC.Core.Game.Auth;
using ColorMC.Core.Objs.Login;
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
    public static readonly Dictionary<(string, AuthType), LoginObj> Auths = new();

    private static readonly string DB = "Auth.db";
    private static string connStr;

    public static SqliteConnection GetSqliteConnection()
    {
        return new SqliteConnection(connStr);
    }
    public static void Init()
    {
        Logs.Info($"登录数据库初始化");
        connStr = new SqliteConnectionStringBuilder("Data Source=" + AppContext.BaseDirectory + DB)
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

    public static bool LoadData(string dir)
    {
        using var conn = new SqliteConnection(new SqliteConnectionStringBuilder("Data Source=" + dir)
        {
            Mode = SqliteOpenMode.ReadOnly
        }.ToString());

        try
        {
            var list = conn.Query<QLogin>("SELECT UserName,UUID,AccessToken," +
               "RefreshToken,ClientToken," +
               "AuthType,Properties,Text1,Text2 FROM auth");

            Auths.Clear();

            foreach (var item in list)
            {
                Auths.Add((item.UUID, item.AuthType), item.ToLogin());
            }

            using var sql = GetSqliteConnection();

            foreach (var item in Auths)
            {
                sql.Execute("INSERT INTO auth(`UserName`,`UUID`,`AccessToken`,`RefreshToken`,`ClientToken`," +
                    "`AuthType`,`Properties`,`Text1`,`Text2`) " +
                    "VALUES (@UserName,@UUID,@AccessToken,@RefreshToken,@ClientToken," +
                    "@AuthType,@Properties,@Text1,@Text2)", item.Value.ToQLogin());
            }

            return true;
        }
        catch (Exception e)
        {
            Logs.Error("数据库读取错误", e);
            return false;
        }
    }

    public static void SaveAuth(LoginObj obj)
    {
        if (string.IsNullOrWhiteSpace(obj.UUID))
        {
            return;
        }

        if (Auths.ContainsKey((obj.UUID, obj.AuthType)))
        {
            Auths[(obj.UUID, obj.AuthType)] = obj;
            Update(obj);
        }
        else
        {
            Auths.Add((obj.UUID, obj.AuthType), obj);
            Add(obj);
        }
    }

    public static LoginObj? Get(string uuid, AuthType type)
    {
        if (Auths.TryGetValue((uuid, type), out var item))
        {
            return item;
        }

        return null;
    }

    private static void GetAll()
    {
        Logs.Info($"读取所有账户");
        using var sql = GetSqliteConnection();
        var list = sql.Query<QLogin>("SELECT UserName,UUID,AccessToken," +
            "RefreshToken,ClientToken," +
            "AuthType,Properties,Text1,Text2 FROM auth");

        foreach (var item in list)
        {
            Auths.Add((item.UUID, item.AuthType), item.ToLogin());
        }
    }

    public static void Delete(LoginObj obj)
    {
        Auths.Remove((obj.UUID, obj.AuthType));
        using var sql = GetSqliteConnection();
        sql.Execute("DELETE FROM auth WHERE UUID=@UUID AND AuthType=@AuthType", new { obj.UUID, obj.AuthType });
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
            "ClientToken=@ClientToken,Properties=@Properties," +
            "Text1=@Text1,Text2=@Text2 WHERE UUID=@UUID AND AuthType=@AuthType",
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
