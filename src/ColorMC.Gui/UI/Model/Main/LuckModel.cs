using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Main;

/// <summary>
/// 幸运方块
/// </summary>
public partial class MainModel
{
    [ObservableProperty]
    private bool _isHaveLuck;
    [ObservableProperty]
    private BlockItemModel? _luckBlockItem;

    /// <summary>
    /// 打开幸运方块窗口
    /// </summary>
    [RelayCommand]
    public void LaunchLuck()
    {
        WindowManager.ShowLuck();
    }
    /// <summary>
    /// 打开方块背包
    /// </summary>
    [RelayCommand]
    public void LaunchBlock()
    {
        WindowManager.ShowBlockBackpack();
    }

    public async void LoadBlock()
    {
        var item = await BaseBinding.GetBlock();

        if (item == null)
        {
            IsHaveLuck = false;
            LuckBlockItem = null;
        }
        else
        {
            IsHaveLuck = true;
            LuckBlockItem = item;
        }
    }
}
