using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Core.Objs;

/// <summary>
/// 游戏启动时的配置存储
/// </summary>
public record GameLaunchObj
{
    public List<DownloadItemObj> GameLibs = [];
    public List<DownloadItemObj> LoaderLibs = [];
    public List<string> JvmArgs = [];
    public List<string> GameArgs = [];
    public HashSet<int> JavaVersions = [];
    public string MainClass;
    public AssetsObj Assets;
}
