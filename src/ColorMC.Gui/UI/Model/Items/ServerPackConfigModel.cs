using ColorMC.Core.Objs.ServerPack;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 服务器包配置显示
/// </summary>
public record ServerPackConfigModel
{
    /// <summary>
    /// 组名
    /// </summary>
    public string Group => Obj.Group;
    /// <summary>
    /// 类型
    /// </summary>
    public string Type => GetType(Obj);
    /// <summary>
    /// 下载配置
    /// </summary>
    public readonly ConfigPackObj Obj;

    public ServerPackConfigModel(ConfigPackObj obj)
    {
        Obj = obj;
    }

    /// <summary>
    /// 获取类型名字
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private static string GetType(ConfigPackObj obj)
    {
        return LanguageUtils.Get(obj.IsZip ? "ServerPackWindow.Tab4.Text5"
            : "ServerPackWindow.Tab4.Text6");
    }
}
