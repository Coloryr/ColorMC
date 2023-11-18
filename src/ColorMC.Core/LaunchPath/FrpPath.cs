namespace ColorMC.Core.LaunchPath;

public static class FrpPath
{
    public const string Name1 = "frp";
    public static string BaseDir { get; private set; }

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="dir">运行路径</param>
    public static void Init(string dir)
    {
        BaseDir = dir;

        if (!BaseDir.EndsWith("/") && !BaseDir.EndsWith("\\"))
        {
            BaseDir += "/";
        }

        BaseDir += Name1;

        Directory.CreateDirectory(BaseDir);
    }

    /// <summary>
    /// 获取SakuraFrp文件路径
    /// </summary>
    /// <param name="ver"></param>
    /// <returns></returns>
    public static string GetSakuraFrpLocal(string ver)
    {
        return $"{BaseDir}/SakuraFrp/{ver}/frpc.exe";
    }
}
