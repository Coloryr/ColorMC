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

public partial class GameEditModel : GameModel
{
    public ObservableCollection<SchematicObj> SchematicList { get; set; } = new();

    [ObservableProperty]
    private SchematicObj? _schematicItem;

    [RelayCommand]
    public void OpenSchematic()
    {
        PathBinding.OpPath(Obj, PathType.SchematicsPath);
    }

    [RelayCommand]
    public async Task LoadSchematic()
    {
        Model.Progress(App.GetLanguage("GameEditWindow.Tab10.Info4"));
        SchematicList.Clear();
        SchematicList.AddRange(await GameBinding.GetSchematics(Obj));
        Model.ProgressClose();
    }

    [RelayCommand]
    public async Task AddSchematic()
    {
        var res = await PathBinding.AddFile(Obj, FileType.Schematic);

        if (res == null)
            return;

        if (res == false)
        {
            Model.Show(App.GetLanguage("Gui.Error12"));
            return;
        }

        Model.Show(App.GetLanguage("GameEditWindow.Tab12.Info3"));
        await LoadSchematic();
    }

    public async void DropSchematic(IDataObject data)
    {
        var res = await GameBinding.AddFile(Obj, data, FileType.Schematic);
        if (res)
        {
            await LoadSchematic();
        }
    }

    public async void DeleteSchematic(SchematicObj obj)
    {
        GameBinding.DeleteSchematic(obj);
        Model.Show(App.GetLanguage("GameEditWindow.Tab10.Info5"));
        await LoadSchematic();
    }
}
