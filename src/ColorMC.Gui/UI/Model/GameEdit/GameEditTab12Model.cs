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

public partial class GameEditTab12Model : GameEditModel
{
    public ObservableCollection<SchematicDisplayObj> SchematicList { get; set; } = new();

    [ObservableProperty]
    private SchematicDisplayObj? _item;

    public GameEditTab12Model(IUserControl con, GameSettingObj obj) : base(con, obj)
    {

    }

    [RelayCommand]
    public void Open()
    {
        BaseBinding.OpPath(Obj, PathType.SchematicsPath);
    }

    [RelayCommand]
    public async Task Load()
    {
        Progress(App.GetLanguage("GameEditWindow.Tab10.Info4"));
        SchematicList.Clear();
        SchematicList.AddRange(await GameBinding.GetSchematics(Obj));
        ProgressClose();
    }

    [RelayCommand]
    public async Task Add()
    {
        var res = await GameBinding.AddFile(Window, Obj, FileType.Schematic);

        if (res == null)
            return;

        if (res == false)
        {
            Show(App.GetLanguage("Gui.Error12"));
            return;
        }

        Show(App.GetLanguage("GameEditWindow.Tab12.Info3"));
        await Load();
    }

    public async void Drop(IDataObject data)
    {
        var res = await GameBinding.AddFile(Obj, data, FileType.Schematic);
        if (res)
        {
            await Load();
        }
    }

    public async void Delete(SchematicDisplayObj obj)
    {
        obj.Schematic.Delete();
        Show(App.GetLanguage("GameEditWindow.Tab10.Info5"));
        await Load();
    }
}
