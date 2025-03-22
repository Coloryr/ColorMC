using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls;

/// <summary>
/// 下载数据接口
/// </summary>
public interface IAddControl
{
    /// <summary>
    /// 选中
    /// </summary>
    /// <param name="item"></param>
    public void SetSelect(FileItemModel item);
    /// <summary>
    /// 安装
    /// </summary>
    /// <param name="item"></param>
    public void Install(FileItemModel item);
    /// <summary>
    /// 选中
    /// </summary>
    /// <param name="item"></param>
    public void SetSelect(FileVersionItemModel item);
    /// <summary>
    /// 安装
    /// </summary>
    /// <param name="item"></param>
    public void Install(FileVersionItemModel item);
    void Back();
    void Next();
    void BackVersion();
    void NextVersion();
}

/// <summary>
/// 下载数据接口
/// </summary>
public interface IAddOptifineControl : IAddControl
{
    public void SetSelect(OptifineVersionItemModel item);
    public void Install(OptifineVersionItemModel item);
}
