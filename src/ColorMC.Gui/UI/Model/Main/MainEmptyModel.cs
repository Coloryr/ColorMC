using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Main;

public partial class MainEmptyModel : TopModel
{
    public string[] LanguageList { get; init; } = LanguageBinding.GetLanguages();

    [ObservableProperty]
    private LanguageType _language;

    private bool _load = true;

    public MainEmptyModel(BaseModel model) : base(model)
    {
        Load();

        _load = false;
    }

    [RelayCommand]
    public void AddUser()
    {
        App.ShowUser(true);
    }

    [RelayCommand]
    public void SetJava()
    {
        App.ShowSetting(SettingType.SetJava);
    }

    [RelayCommand]
    public void AddGame()
    {
        App.ShowAddGame(null);
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

    [RelayCommand]
    public void ShowSetting()
    {
        App.ShowSetting(SettingType.Normal);
    }

    partial void OnLanguageChanged(LanguageType value)
    {
        if (_load)
            return;

        Model.Progress(App.Lang("SettingWindow.Tab2.Info1"));
        ConfigBinding.SetLanguage(value);
        Model.ProgressClose();
    }

    public void Load()
    {
        var config = ConfigBinding.GetAllConfig();
        if (config.Item1 is { } con1)
        {
            Language = con1.Language;
        }
    }

    protected override void Close()
    {

    }
}
