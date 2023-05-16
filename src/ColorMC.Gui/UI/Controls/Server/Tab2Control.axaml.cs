using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UIBinding;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ColorMC.Gui.UI.Controls.Server;

public partial class Tab2Control : UserControl
{
    private ServerPackObj Obj1;
    private ObservableCollection<ServerPackModDisplayModel> List = new();

    public Tab2Control()
    {
        InitializeComponent();

        DataGrid1.ItemsSource = List;
        DataGrid1.CellEditEnded += DataGrid1_CellEditEnded;

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        foreach (var item in List)
        {
            item.Check = false;
            ItemEdit(item);
        }
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        foreach (var item in List)
        {
            item.Check = true;
            ItemEdit(item);
        }
    }

    private void ItemEdit(ServerPackModDisplayModel obj)
    {
        var item = Obj1.Mod?.FirstOrDefault(a => a.Sha1 == obj.Sha1
                        && a.File == obj.FileName);
        if (obj.Check)
        {
            SourceType? source = null;
            if (obj.Source == SourceType.CurseForge.GetName())
            {
                source = SourceType.CurseForge;
            }
            else if (obj.Source == SourceType.Modrinth.GetName())
            {
                source = SourceType.Modrinth;
            }

            if (item != null)
            {
                item.Projcet = obj.PID;
                item.FileId = obj.FID;
                item.Source = source;
                item.Sha1 = obj.Sha1;
                item.File = obj.FileName;
            }
            else
            {
                Obj1.Mod ??= new();
                item = new()
                {
                    Projcet = obj.PID,
                    FileId = obj.FID,
                    Source = source,
                    Sha1 = obj.Sha1,
                    File = obj.FileName
                };
                Obj1.Mod.Add(item);
            }

            obj.Url = item.Url = BaseBinding.MakeUrl(item, FileType.Mod, Obj1.Url);
        }
        else
        {
            if (item != null)
            {
                obj.Url = "";
                Obj1.Mod?.Remove(item);
            }
        }

        GameBinding.SaveServerPack(Obj1);
    }

    private void DataGrid1_CellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (e.Row.DataContext is ServerPackModDisplayModel obj)
        {
            ItemEdit(obj);
        }
    }

    public async void Load()
    {
        if (Obj1 == null)
            return;

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

            var item2 = new ServerPackModDisplayModel()
            {
                FileName = file,
                Check = item1 != null,
                PID = item.PID,
                FID = item.FID,
                Sha1 = item.Obj.Sha1,
                Obj = item
            };

            if (item2.Check)
            {
                if (item1 != null)
                {
                    item2.PID = item1.Projcet;
                    item2.FID = item1.FileId;
                    if (!string.IsNullOrWhiteSpace(item1.Url))
                    {
                        item2.Url = item1.Url;
                    }
                    else
                    {
                        item2.Url = BaseBinding.MakeUrl(item1, FileType.Mod, Obj1.Url);
                    }
                }
            }

            List.Add(item2);
        });

        GameBinding.SaveServerPack(Obj1);
    }

    public void SetObj(ServerPackObj obj1)
    {
        Obj1 = obj1;
    }
}
