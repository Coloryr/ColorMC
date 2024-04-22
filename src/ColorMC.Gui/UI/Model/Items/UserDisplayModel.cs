using ColorMC.Core.Objs;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// �˻���ʾ
/// </summary>
public record UserDisplayModel
{
    /// <summary>
    /// �Ƿ�����
    /// </summary>
    public bool Use { get; set; }
    /// <summary>
    /// �û���
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// UUID
    /// </summary>
    public string UUID { get; set; }
    /// <summary>
    /// ����
    /// </summary>
    public string Type { get; set; }
    /// <summary>
    /// ������Ϣ
    /// </summary>
    public string Text1 { get; set; }
    /// <summary>
    /// ������Ϣ
    /// </summary>
    public string Text2 { get; set; }

    public AuthType AuthType;
}