using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Windows;

/// <summary>
/// 下载数据接口
/// </summary>
public interface IAddWindow
{
    public void SetSelect(FileItemModel item);
    public void Install(FileItemModel item);
    public void SetSelect(FileVersionItemModel item);
    public void Install(FileVersionItemModel item);
    void Back();
    void Next();
    void BackVersion();
    void NextVersion();
}

/// <summary>
/// 下载数据接口
/// </summary>
public interface IAddOptifineWindow : IAddWindow
{
    public void SetSelect(OptifineVersionItemModel item);
    public void Install(OptifineVersionItemModel item);
}
