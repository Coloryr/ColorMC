using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Main;

/// <summary>
/// 主界面
/// </summary>
public partial class MainModel
{
    /// <summary>
    /// 语言列表
    /// </summary>
    public string[] LanguageList { get; init; } = LangUtils.GetLanguages();

    /// <summary>
    /// 选择语言
    /// </summary>
    [ObservableProperty]
    private LanguageType _language;

    /// <summary>
    /// 是否没有游戏实例
    /// </summary>
    private bool _emptyLoad = true;

    /// <summary>
    /// 添加账户
    /// </summary>
    [RelayCommand]
    public void AddUser()
    {
        WindowManager.ShowUser(true);
    }
    /// <summary>
    /// 添加Java
    /// </summary>
    [RelayCommand]
    public void SetJava()
    {
        WindowManager.ShowSetting(SettingType.SetJava);
    }
    /// <summary>
    /// 添加游戏实例
    /// </summary>
    [RelayCommand]
    public void AddGame()
    {
        WindowManager.ShowAddGame(null);
    }
    /// <summary>
    /// 打开网页
    /// </summary>
    [RelayCommand]
    public void OpenWeb1()
    {
        WebBinding.OpenWeb(WebType.Web);
    }
    /// <summary>
    /// 打开网页
    /// </summary>
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

        ConfigBinding.SetLanguage(value);
    }

    /// <summary>
    /// 加载空游戏界面
    /// </summary>
    public void LoadEmptyGame()
    {
        _emptyLoad = true;
        var config = GuiConfigUtils.Config;
        if (config != null)
        {
            Language = config.Language;
        }
        _emptyLoad = false;
    }
}
