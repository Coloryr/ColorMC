using System;

namespace ColorMC.Gui.Net.Apis;

public static class ChunkbaseApi
{
    public const string Url = "https://www.chunkbase.com/apps/seed-map#";

    /// <summary>
    /// 生成Chunkbase网址
    /// </summary>
    /// <param name="version">游戏版本</param>
    /// <param name="seed">世界种子</param>
    /// <param name="islb">是否为巨型生物群系</param>
    /// <returns></returns>
    public static string GenUrl(string version, long seed, bool islb)
    {
        string ver = "1_7";
        try
        {
            var version1 = new Version(version);
            if (version1.Minor < 19)
            {
                ver = $"{version1.Major}_{version1.Minor}";
            }
            else if (version1.Minor == 19)
            {
                if (version1.Build > 2)
                {
                    ver = "1_19_3";
                }
                else
                {
                    ver = "1_19";
                }
            }
            else if (version1.Minor == 20)
            {
                ver = "1_20";
            }
            else if (version1.Minor == 21)
            {
                if (version1.Build < 2)
                {
                    ver = "1_21";
                }
                else if (version1.Build < 4)
                {
                    ver = "1_21_2";
                }
                else if (version1.Build > 8)
                {
                    ver = "1_21_9";
                }
                else if (version1.Build >= 6)
                {
                    ver = "1_21_6";
                }
                else
                {
                    ver = $"{version1.Major}_{version1.Minor}_{version1.Build}";
                }
            }
        }
        catch
        {

        }
        return $"{Url}seed={seed}&platform=java_{ver}{(islb ? "_lb" : "")}&dimension=overworld&x=0&z=0&zoom=0.5";
    }
}
