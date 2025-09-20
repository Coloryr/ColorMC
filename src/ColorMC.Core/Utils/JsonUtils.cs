using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

namespace ColorMC.Core.Utils;

/// <summary>
/// Json处理
/// </summary>
public static class JsonUtils
{
    /// <summary>
    /// 将字符串转为对象
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="data">json字符串</param>
    /// <param name="info">类型信息</param>
    /// <returns>对象</returns>
    public static T? ToObj<T>(string data, JsonTypeInfo<T> info)
    {
        return JsonSerializer.Deserialize(data, info);
    }
    /// <summary>
    /// 将流转换为对象
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="data">json流</param>
    /// <param name="info">类型信息</param>
    /// <returns>对象</returns>
    public static T? ToObj<T>(Stream? data, JsonTypeInfo<T> info)
    {
        if (data == null)
        {
            return default;
        }
        return JsonSerializer.Deserialize(data, info);
    }
    /// <summary>
    /// 将对象转换为json字符串
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="data">对象</param>
    /// <param name="info">类型信息</param>
    /// <returns>json字符串</returns>
    public static string ToString<T>(T data, JsonTypeInfo<T> info)
    {
        return JsonSerializer.Serialize(data, info);
    }
    /// <summary>
    /// 将对象转换为字符串写进流
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="stream">json流</param>
    /// <param name="data">对象</param>
    /// <param name="info">类型信息</param>
    public static void ToString<T>(Stream stream, T data, JsonTypeInfo<T> info)
    {
        JsonSerializer.Serialize(stream, data, info);
    }
    /// <summary>
    /// 异步读取json数据
    /// </summary>
    /// <param name="stream">流</param>
    /// <returns>json对象</returns>
    public static async Task<JsonObject?> ReadAsObjAsync(Stream stream)
    {
        var data = await JsonNode.ParseAsync(stream);
        return data?.AsObject();
    }
    /// <summary>
    /// 读取json数据
    /// </summary>
    /// <param name="str">json字符串</param>
    /// <returns>json对象</returns>
    public static JsonObject? ReadObj(string str)
    {
        return JsonNode.Parse(str)?.AsObject();
    }
    /// <summary>
    /// 读取json数据
    /// </summary>
    /// <param name="stream">流</param>
    /// <returns>json对象</returns>
    public static JsonObject? ReadObj(Stream stream)
    {
        return JsonNode.Parse(stream)?.AsObject();
    }
    /// <summary>
    /// 从json对象中获取对象
    /// </summary>
    /// <param name="obj">json对象</param>
    /// <param name="key">键</param>
    /// <returns>json对象</returns>
    public static JsonObject? GetObj(this JsonObject obj, string key)
    {
        if (obj.TryGetPropertyValue(key, out var obj1) && obj1?.AsObject() is { } obj2)
        {
            return obj2;
        }

        return null;
    }
    /// <summary>
    /// 从json对象中获取列表
    /// </summary>
    /// <param name="obj">json对象</param>
    /// <param name="key">键</param>
    /// <returns>json列表</returns>
    public static JsonArray? GetArray(this JsonObject obj, string key)
    {
        if (obj.TryGetPropertyValue(key, out var obj1) && obj1?.AsArray() is { } obj2)
        {
            return obj2;
        }

        return null;
    }
    /// <summary>
    /// 从json对象中获取字符串
    /// </summary>
    /// <param name="obj">json对象</param>
    /// <param name="key">键</param>
    /// <returns>字符串</returns>
    public static string? GetString(this JsonObject obj, string key)
    {
        if (obj.TryGetPropertyValue(key, out var obj1) && obj1 is { })
        {
            return obj1.GetValue<string>();
        }

        return null;
    }
    /// <summary>
    /// 从json对象中获取数字
    /// </summary>
    /// <param name="obj">json对象</param>
    /// <param name="key">键</param>
    /// <returns>数字</returns>
    public static int? GetInt(this JsonObject obj, string key)
    {
        if (obj.TryGetPropertyValue(key, out var obj1) && obj1 is { })
        {
            return obj1.GetValue<int>();
        }

        return null;
    }
}
