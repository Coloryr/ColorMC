using System.Threading.Tasks;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs.Frp;
using ColorMC.Gui.UI.Model.NetFrp;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;

namespace ColorMC.Gui.UI.Model.Items;

public partial class NetFrpCloudServerModel(FrpCloudObj obj) : SelectItemModel
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

    public FrpCloudObj Obj => obj;

    public bool HaveCustom => obj.Custom != null;

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

    [JsonIgnore]
    public NetFrpModel TopModel;

    [RelayCommand]
    public void Join()
    {
        TopModel.Join(this);
    }

    [RelayCommand]
    public async Task Copy()
    {
        var top = TopModel.Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        await BaseBinding.CopyTextClipboard(top, IP);
    }

    [RelayCommand]
    public void Test()
    {
        TopModel.Test(this);
    }
}
