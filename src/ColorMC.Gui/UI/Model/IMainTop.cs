using System.Collections.Generic;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Model;

/// <summary>
/// 主界面回调
/// </summary>
public interface IMainTop
{
    /// <summary>
    /// 启动游戏实例
    /// </summary>
    /// <param name="obj">游戏实例</param>
    void Launch(GameItemModel obj);
    /// <summary>
    /// 启动游戏实例
    /// </summary>
    /// <param name="list">游戏实例列表</param>
    void Launch(ICollection<GameItemModel> list);
    /// <summary>
    /// 启动游戏实例
    /// </summary>
    /// <param name="list">游戏实例UUID列表</param>
    void Launch(ICollection<string> list);
    /// <summary>
    /// 选中一个游戏实例
    /// </summary>
    /// <param name="model">游戏实例</param>
    void Select(GameItemModel? model);
    /// <summary>
    /// 编辑一个游戏实例分组
    /// </summary>
    /// <param name="model">游戏实例</param>
    void EditGroup(GameItemModel model);
    /// <summary>
    /// 编辑一个游戏实例星标
    /// </summary>
    /// <param name="model">游戏实例</param>
    void DoStar(GameItemModel model);
    /// <summary>
    /// 关闭筛选
    /// </summary>
    void SearchClose();
    /// <summary>
    /// 获取游戏实例
    /// </summary>
    /// <param name="uuid">游戏实例UUID</param>
    /// <returns>游戏实例</returns>
    GameItemModel? GetGame(string uuid);
    /// <summary>
    /// 导出游戏实例启动参数
    /// </summary>
    /// <param name="obj">游戏实例</param>
    void ExportCmd(GameSettingObj obj);
}
