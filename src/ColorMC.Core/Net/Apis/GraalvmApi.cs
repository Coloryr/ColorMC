//using Jint;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ColorMC.Core.Net.Apis;

//public static class GraalvmApi
//{
//    public const string GraalvmUrl = "https://www.graalvm.org/resources/js/download.js";

//    public static async Task<Dictionary<string, string>?> GetJavaList()
//    {
//        var data = await BaseClient.DownloadClient.GetAsync(GraalvmUrl, HttpCompletionOption.ResponseHeadersRead);
//        if (data == null)
//            return null;
//        var str = await data.Content.ReadAsStringAsync();

//        var temp = str;
//        temp += Environment.NewLine;
//        temp += "downloadLinks";

//        using var engine = new Engine();
//        var obj1 = engine.Evaluate("downloadLinks", str);

//        return null;
//    }
//}
