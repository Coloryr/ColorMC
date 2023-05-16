using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UIBinding;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ColorMC.Gui.UI.Controls.Server;

public partial class Tab3Control : UserControl
{
    private ServerPackObj Obj1;
    private ObservableCollection<ServerPackModDisplayModel> List = new();

    public Tab3Control()
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
        var item = Obj1.Resourcepack?.FirstOrDefault(a => a.Sha1 == obj.Sha1
                        && a.File == obj.FileName);
        if (obj.Check)
        {
            if (item != null)
            {
                item.Sha1 = obj.Sha1;
                item.File = obj.FileName;
            }
            else
            {
                Obj1.Resourcepack ??= new();
                item = new()
                {
                    Sha1 = obj.Sha1,
                    File = obj.FileName
                };
                Obj1.Resourcepack.Add(item);
            }

            obj.Url = item.Url = BaseBinding.MakeUrl(item, FileType.Resourcepack, Obj1.Url);
        }
        else
        {
            if (item != null)
            {
                obj.Url = "";
                Obj1.Resourcepack?.Remove(item);
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
        var mods = await GameBinding.GetResourcepacks(Obj1.Game);

        Obj1.Resourcepack?.RemoveAll(a => mods.Find(b => a.Sha1 == b.Pack.Sha1) == null);

        mods.ForEach(item =>
        {
            if (item.Pack.Broken)
                return;

            string file = Path.GetFileName(item.Pack.Local);

            var item1 = Obj1.Resourcepack?.FirstOrDefault(a => a.Sha1 == item.Pack.Sha1
                        && a.File == file);

            var item2 = new ServerPackModDisplayModel()
            {
                FileName = file,
                Check = item1 != null,
                Sha1 = item.Pack.Sha1,
                Obj1 = item
            };
            if (item1 != null)
            {
                if (!string.IsNullOrWhiteSpace(item1.Url))
                {
                    item2.Url = item1.Url;
                }
                else
                {
                    item2.Url = BaseBinding.MakeUrl(item1, FileType.Resourcepack, Obj1.Url);
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
