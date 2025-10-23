using System.Collections.Generic;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.Objs;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 模组升级项目
/// </summary>
/// <param name="obj"></param>
/// <param name="name"></param>
/// <param name="version"></param>
/// <param name="items"></param>
public partial class ModUpgradeModel(ModObj obj, string name, List<string> version, List<DownloadModArg> items)
    : FileModVersionModel(name, version, items, false)
{
    /// <summary>
    /// 模组信息
    /// </summary>
    public ModObj Obj => obj;
}
