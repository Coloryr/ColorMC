using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace ColorMC.Core.Utils;

public static class JsonUtils
{
    public static T? ToObj<T>(string data, JsonTypeInfo<T> info)
    {
        return JsonSerializer.Deserialize(data, info);
    }

    public static T? ToObj<T>(Stream? data, JsonTypeInfo<T> info)
    {
        if (data == null)
        {
            return default;
        }
        return JsonSerializer.Deserialize(data, info);
    }

    public static string ToString<T>(T data, JsonTypeInfo<T> info)
    {
        return JsonSerializer.Serialize(data, info);
    }

    public static void ToString<T>(Stream stream, T data, JsonTypeInfo<T> info)
    {
        JsonSerializer.Serialize(stream, data, info);
    }

    public static async Task<JsonObject?> ReadAsObjAsync(Stream? stream)
    {
        if (stream == null)
        {
            return null;
        }
        var data = await JsonNode.ParseAsync(stream);
        return data?.AsObject();
    }

    public static JsonObject? ReadAsObj(string str)
    {
        var data = JsonNode.Parse(str);
        return data?.AsObject();
    }

    public static JsonObject? ReadAsObj(Stream? stream)
    {
        if (stream == null)
        {
            return null;
        }
        var data = JsonNode.Parse(stream);
        return data?.AsObject();
    }

    public static JsonObject? GetObj(this JsonObject obj, string key)
    {
        if (obj.TryGetPropertyValue(key, out var obj1) && obj1?.AsObject() is { } obj2)
        {
            return obj2;
        }

        return null;
    }

    public static JsonArray? GetArray(this JsonObject obj, string key)
    {
        if (obj.TryGetPropertyValue(key, out var obj1) && obj1?.AsArray() is { } obj2)
        {
            return obj2;
        }

        return null;
    }

    public static string? GetString(this JsonObject obj, string key)
    {
        if (obj.TryGetPropertyValue(key, out var obj1) && obj1 is { })
        {
            return obj1.GetValue<string>();
        }

        return null;
    }

    public static int? GetInt(this JsonObject obj, string key)
    {
        if (obj.TryGetPropertyValue(key, out var obj1) && obj1 is { })
        {
            return obj1.GetValue<int>();
        }

        return null;
    }
}
