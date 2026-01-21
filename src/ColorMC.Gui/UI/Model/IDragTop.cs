using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Model;

public interface IDragTop
{
    /// <summary>
    /// 开始拖拽
    /// </summary>
    /// <param name="item">游戏实例</param>
    void Drag(GameItemModel item);
}
