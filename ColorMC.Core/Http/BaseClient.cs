using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Http;

public enum SourceLocal
{ 
    Offical, BMCLAPI, MCBBS
}

public static class BaseClient
{
    public static SourceLocal Source { get; set; }

    public static HttpClient Client = new()
    {
        Timeout = TimeSpan.FromSeconds(5)
    };

    public static async Task<string> GetString(string url, Dictionary<string, string> arg = null)
    {
        if (arg == null)
        {
            return await Client.GetStringAsync(url);
        }
        else
        {
            string temp = url;
            foreach (var item in arg)
            {
                temp += $"{item.Key}={item.Value}&";
            }
            temp = temp[..^1];
            return await Client.GetStringAsync(temp);
        }
    }

    public static async Task<byte[]> GetBytes(string url, Dictionary<string, string> arg = null)
    {
        if (arg == null)
        {
            return await Client.GetByteArrayAsync(url);
        }
        else
        {
            string temp = url;
            foreach (var item in arg)
            {
                temp += $"{item.Key}={item.Value}&";
            }
            temp = temp[..^1];
            return await Client.GetByteArrayAsync(temp);
        }
    }
}
