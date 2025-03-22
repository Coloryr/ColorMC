using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls;

/// <summary>
/// �������ݽӿ�
/// </summary>
public interface IAddControl
{
    /// <summary>
    /// ѡ��
    /// </summary>
    /// <param name="item"></param>
    public void SetSelect(FileItemModel item);
    /// <summary>
    /// ��װ
    /// </summary>
    /// <param name="item"></param>
    public void Install(FileItemModel item);
    /// <summary>
    /// ѡ��
    /// </summary>
    /// <param name="item"></param>
    public void SetSelect(FileVersionItemModel item);
    /// <summary>
    /// ��װ
    /// </summary>
    /// <param name="item"></param>
    public void Install(FileVersionItemModel item);
    void Back();
    void Next();
    void BackVersion();
    void NextVersion();
}

/// <summary>
/// �������ݽӿ�
/// </summary>
public interface IAddOptifineControl : IAddControl
{
    public void SetSelect(OptifineVersionItemModel item);
    public void Install(OptifineVersionItemModel item);
}
