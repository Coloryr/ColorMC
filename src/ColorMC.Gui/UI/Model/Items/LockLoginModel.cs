using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs.Config;
using ColorMC.Gui.UI.Model.Setting;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 锁定登录
/// </summary>
/// <param name="Top"></param>
/// <param name="Login"></param>
public record LockLoginModel(SettingModel Top, LockLoginSetting Login)
{
    /// <summary>
    /// 名字
    /// </summary>
    public string Name => Login.Name;
    /// <summary>
    /// 附加数据
    /// </summary>
    public string Data => Login.Data;
    /// <summary>
    /// 类型
    /// </summary>
    public string Type => Login.Type.GetName();
    /// <summary>
    /// 类型
    /// </summary>
    public AuthType AuthType => Login.Type;

    /// <summary>
    /// 删除
    /// </summary>
    public void Delete()
    {
        Top.Delete(this);
    }
}
