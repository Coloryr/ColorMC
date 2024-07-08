using ColorMC.Core.Config;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Main;

public partial class MainModel
{
    public string[] LanguageList { get; init; } = LanguageBinding.GetLanguages();

    [ObservableProperty]
    private LanguageType _language;

    private bool _emptyLoad = true;

    [RelayCommand]
    public void AddUser()
    {
        WindowManager.ShowUser(true);
    }

    [RelayCommand]
    public void SetJava()
    {
        WindowManager.ShowSetting(SettingType.SetJava);
    }

    [RelayCommand]
    public void AddGame()
    {
        WindowManager.ShowAddGame(null);
    }

    [RelayCommand]
    public void OpenWeb1()
    {
        WebBinding.OpenWeb(WebType.Web);
    }

    [RelayCommand]
    public void OpenWeb2()
    {
        WebBinding.OpenWeb(WebType.Minecraft);
    }

    partial void OnLanguageChanged(LanguageType value)
    {
        if (_emptyLoad)
        {
            return;
        }

        Model.Progress(App.Lang("SettingWindow.Tab2.Info1"));
        ConfigBinding.SetLanguage(value);
        Model.ProgressClose();
    }

    public void LoadEmptyGame()
    {
        _emptyLoad = true;
        var config = ConfigUtils.Config;
        if (config != null)
        {
            Language = config.Language;
        }
        _emptyLoad = false;
    }
}
