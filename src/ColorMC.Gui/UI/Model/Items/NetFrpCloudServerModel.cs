using System.Threading.Tasks;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs.ColorMC;
using ColorMC.Gui.UI.Model.NetFrp;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 映射
/// </summary>
/// <param name="obj"></param>
public partial class NetFrpCloudServerModel(ColorMCCloudServerObj obj, NetFrpModel top) : SelectItemModel
{
    /// <summary>
    /// 名字
    /// </summary>
    public string Name => obj.Name;
    /// <summary>
    /// 地址
    /// </summary>
    public string IP => obj.IP;
    /// <summary>
    /// 当前在线人数
    /// </summary>
    public string Now => obj.Now;
    /// <summary>
    /// 最大在线人数
    /// </summary>
    public string Max => obj.Max;

    /// <summary>
    /// 提示
    /// </summary>
    public string Tips
    {
        get
        {
            if (HaveCustom)
            {
                return Obj.Custom!.Text;
            }

            return "";
        }
    }

    /// <summary>
    /// 云端数据
    /// </summary>
    public ColorMCCloudServerObj Obj => obj;

    /// <summary>
    /// 是否有自定义数据
    /// </summary>
    public bool HaveCustom => obj.Custom != null;

    /// <summary>
    /// 加载器类型
    /// </summary>
    public string LoaderName
    {
        get
        {
            if (HaveCustom)
            {
                return ((Loaders)(Obj.Custom!.Loader + 1)).GetName();
            }

            return "";
        }
    }

    /// <summary>
    /// 加入
    /// </summary>
    [RelayCommand]
    public void Join()
    {
        top.Join(this);
    }
    /// <summary>
    /// 复制地址到剪贴板
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task Copy()
    {
        var toplevel = top.Model.GetTopLevel();
        if (toplevel == null)
        {
            return;
        }
        await BaseBinding.CopyTextClipboard(toplevel, IP);
    }
    /// <summary>
    /// 获取Motd
    /// </summary>
    [RelayCommand]
    public void Test()
    {
        top.Test(this);
    }
}
