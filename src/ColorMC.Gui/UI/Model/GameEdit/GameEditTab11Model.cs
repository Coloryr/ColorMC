using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaEdit.Utils;
using ColorMC.Core.Game;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditTab11Model : GameEditModel
{
    public ObservableCollection<ShaderpackDisplayObj> ShaderpackList { get; init; } = new();

    [ObservableProperty]
    private ShaderpackDisplayObj? _item;

    public GameEditTab11Model(IUserControl con, GameSettingObj obj) : base(con, obj)
    {

    }

    [RelayCommand]
    public void Open()
    {
        BaseBinding.OpPath(Obj, PathType.ShaderpacksPath);
    }
    [RelayCommand]
    public async Task Load()
    {
        Progress(App.GetLanguage("GameEditWindow.Tab10.Info4"));
        ShaderpackList.Clear();
        ShaderpackList.AddRange(await GameBinding.GetShaderpacks(Obj));
        ProgressClose();
    }
    [RelayCommand]
    public async Task Import()
    {
        var res = await GameBinding.AddFile(Window, Obj, FileType.Shaderpack);
        if (res == null)
            return;

        if (res == false)
        {
            Notify(App.GetLanguage("Gui.Error12"));
            return;
        }

        Notify(App.GetLanguage("GameEditWindow.Tab11.Info3"));
        await Load();
    }

    [RelayCommand]
    public void Add()
    {
        App.ShowAdd(Obj, FileType.Shaderpack);
    }

    public async void Drop(IDataObject data)
    {
        var res = await GameBinding.AddFile(Obj, data, FileType.Shaderpack);
        if (res)
        {
            await Load();
        }
    }

    public async void Delete(ShaderpackDisplayObj obj)
    {
        obj.Shaderpack.Delete();
        Notify(App.GetLanguage("GameEditWindow.Tab10.Info5"));
        await Load();
    }
}
