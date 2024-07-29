using System.Collections.ObjectModel;
using Avalonia.Input;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditModel
{
    public ObservableCollection<ShaderpackObj> ShaderpackList { get; init; } = [];

    [ObservableProperty]
    private ShaderpackObj? _shaderpackItem;

    [ObservableProperty]
    private bool _shaderpackEmptyDisplay;


    private void OpenShaderpack()
    {
        PathBinding.OpenPath(_obj, PathType.ShaderpacksPath);
    }

    public async void LoadShaderpack()
    {
        Model.Progress(App.Lang("GameEditWindow.Tab10.Info4"));
        ShaderpackList.Clear();
        ShaderpackList.AddRange(await GameBinding.GetShaderpacks(_obj));
        Model.ProgressClose();

        ShaderpackEmptyDisplay = ShaderpackList.Count == 0;
    }

    private async void ImportShaderpack()
    {
        var res = await PathBinding.AddFile(_obj, FileType.Shaderpack);
        if (res == null)
            return;

        if (res == false)
        {
            Model.Notify(App.Lang("GameEditWindow.Tab11.Error1"));
            return;
        }

        Model.Notify(App.Lang("GameEditWindow.Tab11.Info1"));
        LoadShaderpack();
    }


    private void AddShaderpack()
    {
        WindowManager.ShowAdd(_obj, FileType.Shaderpack);
    }

    public async void DropShaderpack(IDataObject data)
    {
        var res = await GameBinding.AddFile(_obj, data, FileType.Shaderpack);
        if (res)
        {
            LoadShaderpack();
        }
    }

    public void DeleteShaderpack(ShaderpackObj obj)
    {
        GameBinding.DeleteShaderpack(obj);
        Model.Notify(App.Lang("GameEditWindow.Tab10.Info5"));
        LoadShaderpack();
    }
}
