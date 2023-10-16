using ColorMC.Core.Objs.ServerPack;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// ��������������ʾ
/// </summary>
public record ServerPackConfigModel
{
    /// <summary>
    /// ����
    /// </summary>
    public string Group => Obj.Group;
    /// <summary>
    /// ����
    /// </summary>
    public string Type => GetType(Obj);

    public readonly ConfigPackObj Obj;

    public ServerPackConfigModel(ConfigPackObj obj)
    {
        Obj = obj;
    }

    private string GetType(ConfigPackObj obj)
    {
        return App.GetLanguage(
            obj.IsZip ? "ServerPackWindow.Tab4.Item1"
            : "ServerPackWindow.Tab4.Item2");
    }
}
