using Avalonia.Controls;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using Avalonia.Interactivity;
using System.Collections.Generic;
using System;
using ColorMC.Core.Utils;
using ColorMC.Core.LaunchPath;
using System.IO;

namespace ColorMC.Gui.UI.Controls.Server;

public partial class Tab4Control : UserControl
{
    private bool load;

    private ServerPackObj Obj1;
    private ObservableCollection<ServerPackConfigDisplayObj> List = new();
    private ObservableCollection<string> List1 = new();

    public Tab4Control()
    {
        InitializeComponent();

        DataGrid1.Items = List;
        DataGrid1.CellPointerPressed += DataGrid1_CellPointerPressed;

        Button1.Click += Button1_Click;

        ComboBox1.Items = List1;

        ComboBox2.Items = new List<string>() { "压缩包同步", "完整同步" };
        ComboBox2.SelectedIndex = 0;
    }

    private string GetUrl(ConfigPackObj item)
    {
        if (!string.IsNullOrWhiteSpace(Obj1.Url))
        {
            return Obj1.Url + "config/" + item.Group;
        }
        else
        {
            return "";
        }
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        if (ComboBox1.SelectedItem is not string group)
            return;
        string local = Obj1.Game.GetGamePath() + "/" + group;
        var type = ComboBox2.SelectedIndex;
        Obj1.Config ??= new();
        if (local.EndsWith("/"))
        {
            if (type == 0)
            {
                var item = new ConfigPackObj()
                {
                    Group = group ,
                    Zip = true,
                };

                item.Url = GetUrl(item)[..^1] + ".zip";
                Obj1.Config.Add(item);
            }
            else
            {
                var item = new ConfigPackObj()
                {
                    Group = group,
                    Zip = false,
                };

                item.Url = GetUrl(item);
                Obj1.Config.Add(item);
            }
        }
        else
        {
            var item = new ConfigPackObj()
            {
                Group = group,
                Zip = type == 0,
            };

            item.Url = GetUrl(item);
            Obj1.Config.Add(item);
        }
        Load();
    }

    private void DataGrid1_CellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        if (e.PointerPressedEventArgs.GetCurrentPoint(this)
            .Properties.IsRightButtonPressed)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (DataGrid1.SelectedItem is ServerPackConfigDisplayObj obj)
                {
                    new ServerPackFlyout1(this, obj).ShowAt(this, true);
                }
            });
        }
    }

    private string GetType(ConfigPackObj obj)
    {
        if (obj.Zip)
            return "压缩包同步";
        else
            return "完整同步";
    }

    public void Load()
    {
        if (Obj1 == null)
            return;

        load = true;
        List1.Clear();
        List.Clear();
        var mods = GameBinding.GetAllTopConfig(Obj1.Game);

        Obj1.Config?.RemoveAll(a => mods.Find(b => a.Group == b) == null);

        mods.ForEach(item =>
        {
            var item1 = Obj1.Config?.FirstOrDefault(a => a.Group == item);
            if (item1 != null)
            {
                var item2 = new ServerPackConfigDisplayObj()
                {
                    Group = item,
                    Type = GetType(item1),
                    Url = item1.Url
                };

                List.Add(item2);
            }
            else
            {
                List1.Add(item);
            }
        });

        GameBinding.SaveServerPack(Obj1);

        load = false;
    }

    public void SetObj(ServerPackObj obj1)
    {
        Obj1 = obj1;
    }

    public void Delete(ServerPackConfigDisplayObj obj)
    {
        var item1 = Obj1.Config?.FirstOrDefault(a => a.Group == obj.Group);
        if (item1 != null)
        {
            Obj1.Config?.Remove(item1);
            Load();
        }
    }
}
