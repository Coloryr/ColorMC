using ColorMC.Core.GuiHandle;
using ColorMC.Core.Objs.Login;
using ColorMC.Core.Objs.Minecraft;

namespace ColorMC.Core.Objs;

/// <summary>
/// 游戏启动所使用的参数
/// </summary>
public record GameLaunchArg
{
    /// <summary>
    /// 登录账户
    /// </summary>
    public required LoginObj Auth;
    /// <summary>
    /// 自动进入的世界
    /// </summary>
    public SaveObj? World;
    /// <summary>
    /// 自动加入的服务器
    /// </summary>
    public ServerObj? Server;
    /// <summary>
    /// 是否以管理员方式启动
    /// </summary>
    public bool Admin;
    /// <summary>
    /// 解密对接
    /// </summary>
    public ILaunchGui? Gui;
    /// <summary>
    /// ColorMC ASM端口
    /// </summary>
    public int? Mixinport;
}

/// <summary>
/// 游戏实例导出
/// </summary>
public record GameExportArg
{
    /// <summary>
    /// 压缩包位置
    /// </summary>
    public required string File;
    /// <summary>
    /// 整合包类型
    /// </summary>
    public required PackType Type;
    /// <summary>
    /// 游戏实例
    /// </summary>
    public required GameSettingObj Obj;
    /// <summary>
    /// 选择的Mod
    /// </summary>
    public IEnumerable<ModExportObj> Mods;
    /// <summary>
    /// 选择的其他文件
    /// </summary>
    public IEnumerable<ModExportBaseObj> OtherFiles;
    /// <summary>
    /// 不选择的文件
    /// </summary>
    public List<string> UnSelectItems;
    /// <summary>
    /// 选中的文件
    /// </summary>
    public List<string> SelectItems;
    /// <summary>
    /// 名字
    /// </summary>
    public string Name;
    /// <summary>
    /// 作者
    /// </summary>
    public string Author;
    /// <summary>
    /// 版本
    /// </summary>
    public string Version;
    /// <summary>
    /// 说明
    /// </summary>
    public string Summary;
}