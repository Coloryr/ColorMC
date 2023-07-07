using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaEdit.Utils;
using ColorMC.Core.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditTab12Model : GameEditTabModel
{
    public ObservableCollection<SchematicDisplayObj> SchematicList { get; set; } = new();

    [ObservableProperty]
    private SchematicDisplayObj? item;

    public GameEditTab12Model(IUserControl con, GameSettingObj obj) : base(con, obj)
    {

    }

    [RelayCommand]
    public void Open()
    {
        BaseBinding.OpPath(Obj.GetSchematicsPath());
    }

    [RelayCommand]
    public void Load()
    {
        var window = Con.Window;
        window.ProgressInfo.Show(App.GetLanguage("GameEditWindow.Tab10.Info4"));
        SchematicList.Clear();
        SchematicList.AddRange(GameBinding.GetSchematics(Obj));
        window.ProgressInfo.Close();
    }

    [RelayCommand]
    public async Task Add()
    {
        var window = Con.Window;
        var res = await GameBinding.AddFile(window as Window, Obj, FileType.Schematic);

        if (res == null)
            return;

        if (res == false)
        {
            window.NotifyInfo.Show(App.GetLanguage("Gui.Error12"));
            return;
        }

        window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab12.Info3"));
        Load();
    }

    public async void Drop(IDataObject data)
    {
        var res = await GameBinding.AddFile(Obj, data, FileType.Schematic);
        if (res)
        {
            Load();
        }
    }

    public void Delete(SchematicDisplayObj obj)
    {
        var window = Con.Window;
        obj.Schematic.Delete();
        window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab10.Info5"));
        Load();
    }
}
