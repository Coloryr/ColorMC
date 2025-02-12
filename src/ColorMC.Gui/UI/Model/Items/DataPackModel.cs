using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 数据包
/// </summary>
/// <param name="obj"></param>
public partial class DataPackModel(DataPackObj obj)
{
    /// <summary>
    /// 数据包
    /// </summary>
    public DataPackObj Pack => obj;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? Enable => obj.Enable;
    /// <summary>
    /// 名字
    /// </summary>
    public string Name => obj.Name;
    /// <summary>
    /// 描述
    /// </summary>
    public string Description => obj.Description;
    /// <summary>
    /// 格式版本号
    /// </summary>
    public int PackFormat => obj.PackFormat;
}
