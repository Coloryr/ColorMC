using ColorMC.Core.Objs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditModel : GameModel
{
    [ObservableProperty]
    private bool _displayFilter = true;

    [RelayCommand]
    public void ShowFilter()
    {
        DisplayFilter = !DisplayFilter;
    }

    public GameEditModel(BaseModel model, GameSettingObj obj) : base(model, obj)
    {
        _title = string.Format(App.GetLanguage("GameEditWindow.Tab2.Text13"), Obj.Name);
        GameLoad();
        ConfigLoad();
    }

    protected override void Close()
    {
        _configLoad = true;
        _gameLoad = true;
        GameVersionList.Clear();
        LoaderVersionList.Clear();
        GroupList.Clear();
        JvmList.Clear();
        ModList.Clear();
        _modItems.Clear();
        foreach (var item in WorldList)
        {
            item.Close();
        }
        WorldList.Clear();
        _selectWorld = null;
        foreach (var item in ResourcePackList)
        {
            item.Close();
        }
        ResourcePackList.Clear();
        _lastResource = null;
        foreach (var item in ScreenshotList)
        {
            item.Close();
        }
        ScreenshotList.Clear();
        _lastScreenshot = null;
        ServerList.Clear();
        ShaderpackList.Clear();
        SchematicList.Clear();
    }
}
