using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls;

public interface IAddFileControl
{
    /// <summary>
    /// 选中
    /// </summary>
    /// <param name="item"></param>
    public void SetSelect(FileVersionItemModel item);
    /// <summary>
    /// 上一页
    /// </summary>
    void LastVersionPage();
    /// <summary>
    /// 下一页
    /// </summary>
    void NextVersionPage();
}

/// <summary>
/// 下载数据接口
/// </summary>
public interface IAddControl
{
    /// <summary>
    /// 选中项目
    /// </summary>
    /// <param name="item"></param>
    public void SetSelect(FileItemModel item);
    /// <summary>
    /// 安装最新版本
    /// </summary>
    /// <param name="item"></param>
    public void Install(FileItemModel item);
    /// <summary>
    /// 显示详情页面
    /// </summary>
    public void ShowInfo(FileItemModel item);
    /// <summary>
    /// 上一页
    /// </summary>
    void LastListPage();
    /// <summary>
    /// 下一页
    /// </summary>
    void NextListPage();
    /// <summary>
    /// 安装选中的版本
    /// </summary>
    /// <param name="item"></param>
    public void Install(FileVersionItemModel item);
    /// <summary>
    /// 返回界面
    /// </summary>
    public void Back();
}

/// <summary>
/// 下载数据接口
/// </summary>
public interface IAddOptifineControl
{
    public void SetSelect(OptifineVersionItemModel item);
    public void Install(OptifineVersionItemModel item);
}
