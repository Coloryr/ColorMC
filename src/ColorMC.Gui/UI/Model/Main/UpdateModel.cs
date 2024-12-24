using System.Threading.Tasks;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

#if !DEBUG
using ColorMC.Gui.Objs;
using ColorMC.Gui.Utils;
#endif

namespace ColorMC.Gui.UI.Model.Main;

public partial class MainModel
{
    [ObservableProperty]
    private bool _haveUpdate;
#if !DEBUG
    private bool _isNewUpdate;
    private string _updateStr;
#endif

    [RelayCommand]
    public async Task NewInfo()
    {
        if (_isGetNewInfo)
        {
            return;
        }
        _isGetNewInfo = true;

        var data = await WebBinding.GetNewLog();
        if (data == null)
        {
            Model.Show(App.Lang("MainWindow.Error1"));
        }
        else
        {
            Model.ShowText(App.Lang("MainWindow.Info40"), data);
        }

        _isGetNewInfo = false;
    }

    [RelayCommand]
    public async Task Upgrade()
    {
#if !DEBUG
        var res = await Model.ShowTextWait(App.Lang("BaseBinding.Info2"), _updateStr);
        if (res)
        {
            if (_isNewUpdate)
            {
                WebBinding.OpenWeb(WebType.ColorMCDownload);
            }
            else
            {
                UpdateUtils.StartUpdate();
            }
        }
#endif
    }

    private async void CheckUpdate()
    {
#if DEBUG
        HaveUpdate = true;
#else
        var data = await UpdateUtils.Check();
        if (!data.Item1)
        {
            return;
        }
        HaveUpdate = true;
        _isNewUpdate = data.Item2 || ColorMCGui.IsAot || ColorMCGui.IsMin;
        _updateStr = data.Item3!;
#endif
    }
}
