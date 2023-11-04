using Avalonia.Input;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditModel : MenuModel
{
    public ObservableCollection<ShaderpackObj> ShaderpackList { get; init; } = new();

    [ObservableProperty]
    private ShaderpackObj? _shaderpackItem;

    [RelayCommand]
    public void OpenShaderpack()
    {
        PathBinding.OpPath(_obj, PathType.ShaderpacksPath);
    }
    [RelayCommand]
    public async Task LoadShaderpack()
    {
        Model.Progress(App.Lang("GameEditWindow.Tab10.Info4"));
        ShaderpackList.Clear();
        ShaderpackList.AddRange(await GameBinding.GetShaderpacks(_obj));
        Model.ProgressClose();
    }
    [RelayCommand]
    public async Task ImportShaderpack()
    {
        var res = await PathBinding.AddFile(_obj, FileType.Shaderpack);
        if (res == null)
            return;

        if (res == false)
        {
            Model.Notify(App.Lang("Gui.Error12"));
            return;
        }

        Model.Notify(App.Lang("GameEditWindow.Tab11.Info3"));
        await LoadShaderpack();
    }

    [RelayCommand]
    public void AddShaderpack()
    {
        App.ShowAdd(_obj, FileType.Shaderpack);
    }

    public async void DropShaderpack(IDataObject data)
    {
        var res = await GameBinding.AddFile(_obj, data, FileType.Shaderpack);
        if (res)
        {
            await LoadShaderpack();
        }
    }

    public async void DeleteShaderpack(ShaderpackObj obj)
    {
        GameBinding.DeleteShaderpack(obj);
        Model.Notify(App.Lang("GameEditWindow.Tab10.Info5"));
        await LoadShaderpack();
    }
}
