using ColorMC.Core.Net.Downloader;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using System.Threading.Tasks;

namespace ColorMC.Gui.UIBinding;

public static class OtherBinding
{
    public static bool LoadAuthDatabase(string dir)
    {
        return AuthDatabase.LoadData(dir);
    }

    public static bool LoadConfig(string dir)
    {
        return ConfigUtils.Load(dir, true);
    }

    public static (int, int) GetDownloadState()
    {
        return (DownloadManager.AllSize, DownloadManager.DoneSize);
    }

    public static async Task<(bool, string?)> Launch(GameSettingObj? obj, bool debug)
    {
        if (obj == null)
        {
            return (false, "没有选择游戏实例");
        }

        var login = UserBinding.GetLastUser();
        if (login == null)
        {
            return (false, "没有选择账户");
        }

        return (await BaseBinding.Launch(obj, login, debug), null);
    }
}
