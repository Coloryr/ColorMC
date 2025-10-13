using System.Collections.Generic;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.Main;

namespace ColorMC.Gui.UI.Model;

/// <summary>
/// 多选类型主界面回调
/// </summary>
public interface IMutTop : IMainTop
{
    /// <summary>
    /// 是否多选
    /// </summary>
    bool IsMut { get; }
    /// <summary>
    /// 开始多选
    /// </summary>
    void StartMut();
    /// <summary>
    /// 开始多选
    /// </summary>
    /// <param name="model">选中的游戏分组</param>
    void StartMut(GameGroupModel model);
    /// <summary>
    /// 结束多选
    /// </summary>
    /// <returns>选中的游戏实例</returns>
    List<GameItemModel> EndMut();
    /// <summary>
    /// 获取多选
    /// </summary>
    /// <returns>选中的游戏实例</returns>
    List<GameItemModel> GetMut();
    /// <summary>
    /// 多选启动
    /// </summary>
    void MutLaunch();
    /// <summary>
    /// 多选编辑
    /// </summary>
    void MutEdit();
    /// <summary>
    /// 多选编辑分组
    /// </summary>
    void MutEditGroup();
    /// <summary>
    /// 多选删除
    /// </summary>
    void MutDelete();
}