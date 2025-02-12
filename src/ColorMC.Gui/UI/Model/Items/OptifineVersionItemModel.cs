using ColorMC.Core.Objs.OptiFine;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 高清修复项目
/// </summary>
/// <param name="obj">高清修复</param>
public partial class OptifineVersionItemModel(OptifineObj obj) : SelectItemModel
{
    /// <summary>
    /// 添加
    /// </summary>
    public IAddOptifineWindow? Add { get; set; }

    /// <summary>
    /// 版本
    /// </summary>
    public string Version => obj.Version;
    /// <summary>
    /// 游戏版本
    /// </summary>
    public string MCVersion => obj.MCVersion;
    /// <summary>
    /// 加载类型
    /// </summary>
    public string Forge => obj.Forge;
    /// <summary>
    /// 更新日期
    /// </summary>
    public string Date => obj.Date;

    /// <summary>
    /// 高清修复数据
    /// </summary>
    public OptifineObj Obj => obj;

    /// <summary>
    /// 选中
    /// </summary>
    public void SetSelect()
    {
        Add?.SetSelect(this);
    }

    /// <summary>
    /// 安装
    /// </summary>
    public void Install()
    {
        Add?.Install(this);
    }
}
