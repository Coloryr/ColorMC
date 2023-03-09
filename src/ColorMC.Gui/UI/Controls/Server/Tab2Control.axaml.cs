using Avalonia.Controls;
using ColorMC.Core.Game;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ColorMC.Gui.UI.Controls.Server;

public partial class Tab2Control : UserControl
{
    private bool load = false;
    private ServerPackObj Obj1;
    private ObservableCollection<ServerPackModDisplayObj> List = new();

    public Tab2Control()
    {
        InitializeComponent();

        DataGrid1.Items = List;
        DataGrid1.CellEditEnded += DataGrid1_CellEditEnded;
    }

    private void DataGrid1_CellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (e.Row.DataContext is ServerPackModDisplayObj obj)
        {
            var item = Obj1.Mod?.FirstOrDefault(a => a.Sha1 == obj.Sha1
                        && a.File == obj.FileName);
            if (item != null)
            {
                item.Projcet = obj.PID;
                item.FileId = obj.FID;
                item.Source = obj.Source;
                item.Sha1 = obj.Sha1;
                item.Url = obj.Url;
                item.IsNet = !(string.IsNullOrWhiteSpace(obj.PID)
                    || string.IsNullOrWhiteSpace(obj.FID));
                item.File = obj.FileName;
            }
            else
            {
                Obj1.Mod ??= new();
                Obj1.Mod.Add(new()
                {
                    Projcet = obj.PID,
                    FileId = obj.FID,
                    Source = obj.Source,
                    Sha1 = obj.Sha1,
                    Url = obj.Url,
                    IsNet = !(string.IsNullOrWhiteSpace(obj.PID)
                    || string.IsNullOrWhiteSpace(obj.FID)),
                    File = obj.FileName
                });
            }

            GameBinding.SaveServerPack(Obj1);
        }
    }

    public async void Load()
    {
        if (Obj1 == null)
            return;

        load = true;
        List.Clear();
        var mods = await GameBinding.GetGameMods(Obj1.Game);

        Obj1.Mod?.RemoveAll(a => mods.Find(b => a.Sha1 == b.Obj.Sha1) == null);

        mods.ForEach(item =>
        {
            if (item.Obj.Broken)
                return;

            string file = Path.GetFileName(item.Obj.Local);

            var item1 = Obj1.Mod?.FirstOrDefault(a => a.Sha1 == item.Obj.Sha1
                        && a.File == file);

            List.Add(new()
            {
                FileName = file,
                Check = item1 != null,
                PID = item.PID,
                FID = item.FID,
                Source = item.Source1(),
                Sha1 = item.Obj.Sha1,
                Url = item1  == null ? item.Obj1.Url : item1.Url,
                Obj = item
            });
        });

        GameBinding.SaveServerPack(Obj1);

        load = false;
    }

    public void SetObj(ServerPackObj obj1)
    {
        Obj1 = obj1;
    }
}
