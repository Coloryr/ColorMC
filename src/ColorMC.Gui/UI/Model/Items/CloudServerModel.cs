using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Items;

public partial class CloudServerModel : ObservableObject
{
    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 地址
    /// </summary>
    public string IP { get; set; }
    /// <summary>
    /// 当前在线人数
    /// </summary>
    public string Now { get; set; }
    /// <summary>
    /// 最大在线人数
    /// </summary>
    public string Max { get; set; }

    [RelayCommand]
    public void Join()
    {

    }

    [RelayCommand]
    public async Task Copy()
    {
        await BaseBinding.CopyTextClipboard(IP);
    }
}
