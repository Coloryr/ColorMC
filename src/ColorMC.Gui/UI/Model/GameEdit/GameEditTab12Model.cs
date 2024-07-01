using System.Collections.ObjectModel;
using Avalonia.Input;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditModel
{
    public ObservableCollection<SchematicObj> SchematicList { get; set; } = [];

    [ObservableProperty]
    private SchematicObj? _schematicItem;

    [ObservableProperty]
    private bool _schematicEmptyDisplay;

    [RelayCommand]
    public void OpenSchematic()
    {
        PathBinding.OpPath(_obj, PathType.SchematicsPath);
    }

    public async void LoadSchematic()
    {
        Model.Progress(App.Lang("GameEditWindow.Tab10.Info4"));
        SchematicList.Clear();
        SchematicList.AddRange(await GameBinding.GetSchematics(_obj));
        Model.ProgressClose();
        SchematicEmptyDisplay = SchematicList.Count == 0;
    }

    private async void AddSchematic()
    {
        var res = await PathBinding.AddFile(_obj, FileType.Schematic);

        if (res == null)
            return;

        if (res == false)
        {
            Model.Show(App.Lang("GameEditWindow.Tab11.Error1"));
            return;
        }

        Model.Show(App.Lang("GameEditWindow.Tab12.Info3"));
        LoadSchematic();
    }

    public async void DropSchematic(IDataObject data)
    {
        var res = await GameBinding.AddFile(_obj, data, FileType.Schematic);
        if (res)
        {
            LoadSchematic();
        }
    }

    public void DeleteSchematic(SchematicObj obj)
    {
        GameBinding.DeleteSchematic(obj);
        Model.Show(App.Lang("GameEditWindow.Tab10.Info5"));
        LoadSchematic();
    }
}
