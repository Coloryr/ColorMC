using ColorMC.Core.Objs;
using SharpCompress.Archives;

namespace ColorMC.Core.Worker;

public interface IModPackWork : IDisposable
{
    /// <summary>
    /// 获取主信息
    /// </summary>
    /// <returns>是否获取失败</returns>
    bool ReadInfo();
    /// <summary>
    /// 获取版本数据
    /// </summary>
    /// <returns>是否获取失败</returns>
    Task<bool> ReadVersion();
    /// <summary>
    /// 创建游戏实例
    /// </summary>
    /// <param name="group"></param>
    /// <returns></returns>
    Task<GameSettingObj?> CreateGame(string? group);
    /// <summary>
    /// 解压文件
    /// </summary>
    /// <returns></returns>
    Task<bool> Unzip(List<IArchiveEntry>? unselect);
    /// <summary>
    /// 获取Mod信息
    /// </summary>
    /// <returns></returns>
    Task<bool> GetInfo();
    /// <summary>
    /// 下载所需文件
    /// </summary>
    /// <returns></returns>
    Task Download();
    /// <summary>
    /// 更新游戏实例版本信息
    /// </summary>
    /// <param name="game"></param>
    void UpdateGame(GameSettingObj game);
    /// <summary>
    /// 检查更新
    /// </summary>
    /// <returns></returns>
    Task<bool> CheckUpgrade();
}
