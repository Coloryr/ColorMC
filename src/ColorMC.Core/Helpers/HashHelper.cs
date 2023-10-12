using System.Security.Cryptography;
using System.Text;

namespace ColorMC.Core.Helpers;

/// <summary>
/// 文件哈希值计算
/// </summary>
public static class HashHelper
{
    /// <summary>
    /// 获取MD5值
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string GenMd5(byte[] data)
    {
        var text = new StringBuilder();
        foreach (byte iByte in MD5.HashData(data))
        {
            text.AppendFormat("{0:x2}", iByte);
        }
        return text.ToString().ToLower();
    }
    /// <summary>
    /// 获取SHA1值
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string GenSha1(byte[] data)
    {
        var text = new StringBuilder();
        foreach (byte item in SHA1.HashData(data))
        {
            text.AppendFormat("{0:x2}", item);
        }
        return text.ToString().ToLower();
    }
    /// <summary>
    /// 获取SHA1值
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string GenSha1(string input)
    {
        return GenSha1(Encoding.UTF8.GetBytes(input));
    }
    /// <summary>
    /// 获取SHA256值
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string GenSha256(string input)
    {
        return GenSha256(Encoding.UTF8.GetBytes(input));
    }
    /// <summary>
    /// 获取SHA1值
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static string GenSha1(Stream stream)
    {
        var text = new StringBuilder();
        foreach (byte item in SHA1.HashData(stream))
        {
            text.AppendFormat("{0:x2}", item);
        }
        return text.ToString().ToLower();
    }
    /// <summary>
    /// 获取SHA1值
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static async Task<string> GenSha1Async(Stream stream)
    {
        var text = new StringBuilder();
        foreach (byte item in await SHA1.HashDataAsync(stream))
        {
            text.AppendFormat("{0:x2}", item);
        }
        return text.ToString().ToLower();
    }
    /// <summary>
    /// 获取SHA256值
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static string GenSha256(byte[] data)
    {
        var text = new StringBuilder();
        foreach (byte item in SHA256.HashData(data))
        {
            text.AppendFormat("{0:x2}", item);
        }
        return text.ToString().ToLower();
    }
    /// <summary>
    /// 获取SHA256值
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static string GenSha256(Stream stream)
    {
        var text = new StringBuilder();
        foreach (byte item in SHA256.HashData(stream))
        {
            text.AppendFormat("{0:x2}", item);
        }
        return text.ToString().ToLower();
    }

    /// <summary>
    /// 获取SHA256值
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static async Task<string> GenSha256Async(Stream stream)
    {
        var text = new StringBuilder();
        foreach (byte item in await SHA256.HashDataAsync(stream))
        {
            text.AppendFormat("{0:x2}", item);
        }
        return text.ToString().ToLower();
    }

    /// <summary>
    /// 获取SHA512值
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static async Task<string> GenSha512Async(Stream stream)
    {
        var text = new StringBuilder();
        foreach (byte item in await SHA512.HashDataAsync(stream))
        {
            text.AppendFormat("{0:x2}", item);
        }
        return text.ToString().ToLower();
    }

    /// <summary>
    /// 生成Base64
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string GenBase64(string input)
    {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
    }
    /// <summary>
    /// 反解Base64
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string DeBase64(string input)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(input));
    }
}
