using ColorMC.Core.Config;
using ColorMC.Core.Http;

namespace ColorMC.Core;

public static class CoreMain
{
    public const string Version = "1.0.0";

    public static BaseClient? HttpClient { get; private set; }

    public static Action<string, Exception, bool> OnError;
    public static Action NewStart;

    public static void Init(string dir) 
    {
        SystemInfo.Init();

        HttpClient = new();


        Logs.Init(dir);

        ConfigUtils.Init(dir);
    }
}