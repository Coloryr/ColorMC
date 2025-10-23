using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 自定义游戏启动配置
/// </summary>
/// <param name="Obj"></param>
public record CustomJsonModel(CustomGameArgObj Obj)
{
    /// <summary>
    /// 文件路径
    /// </summary>
    public string File => Obj.File;

    /// <summary>
    /// 名字
    /// </summary>
    public string Name => Obj.Name;
    /// <summary>
    /// 版本号
    /// </summary>
    public string Version => Obj.Version;
    /// <summary>
    /// Id
    /// </summary>
    public string Uid => Obj.Uid;
    /// <summary>
    /// 加载顺序
    /// </summary>
    public int Order => Obj.Order;
}
