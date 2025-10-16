using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls;

/// <summary>
/// 收藏页面接口
/// </summary>
public interface ICollectControl
{
    /// <summary>
    /// 选中项目
    /// </summary>
    /// <param name="item"></param>
    void SetSelect(CollectItemModel item);
    /// <summary>
    /// 安装项目
    /// </summary>
    /// <param name="item"></param>
    void Install(CollectItemModel item);
    /// <summary>
    /// 安装所有选中项目
    /// </summary>
    void Install();
    bool HaveSelect();
    void DeleteSelect();
    void GroupSelect();
    bool HaveGroup();
    void ChoiseChange();
}