using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Add;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.Utils;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddGameControl : UserControl, IUserControl
{
    private AddGameTab1Control _tab1;
    private AddGameTab2Control _tab2;
    private AddGameTab3Control _tab3;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public string Title => App.Lang("AddGameWindow.Title");

    public string UseName { get; }

    public AddGameControl()
    {
        InitializeComponent();

        UseName = ToString() ?? "AddGameControl";

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);
    }

    private void DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files))
        {
            var files = e.Data.GetFiles();
            if (files == null || files.Count() > 1)
                return;

            var item = files.ToList()[0];
            if (item == null)
                return;
            if (item is IStorageFolder forder && Directory.Exists(forder.GetPath()))
            {
                Grid2.IsVisible = true;
                Label1.Text = App.Lang("Gui.Info42");
            }
            else if (item.Name.EndsWith(".zip") || item.Name.EndsWith(".mrpack"))
            {
                Grid2.IsVisible = true;
                Label1.Text = App.Lang("Gui.Info7");
            }
        }
    }

    private void DragLeave(object? sender, DragEventArgs e)
    {
        Grid2.IsVisible = false;
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        Grid2.IsVisible = false;
        if (e.Data.Contains(DataFormats.Files))
        {
            var files = e.Data.GetFiles();
            if (files == null || files.Count() > 1)
                return;

            var item = files.ToList()[0];
            if (item == null)
                return;
            if (item is IStorageFolder forder && Directory.Exists(forder.GetPath()))
            {
                var model = (DataContext as AddGameModel)!;
                model.GoTab("Tab3");
                model.SetPath(item.GetPath()!);
            }
            else if (item.Name.EndsWith(".zip") || item.Name.EndsWith(".mrpack"))
            {
                var model = (DataContext as AddGameModel)!;
                model.GoTab("Tab2");
                model.SetFile(item.GetPath()!);
            }
        }
    }

    public void SetBaseModel(BaseModel model)
    {
        var amodel = new AddGameModel(model);
        amodel.PropertyChanged += Model_PropertyChanged;
        DataContext = amodel;
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "WindowClose")
        {
            Window.Close();
        }
        else if (e.PropertyName == "GoTab1")
        {
            Content1.Child = _tab1 ??= new();
        }
        else if (e.PropertyName == "GoTab2")
        {
            Content1.Child = _tab2 ??= new();
        }
        else if (e.PropertyName == "GoTab3")
        {
            Content1.Child = _tab3 ??= new();
        }
        else if (e.PropertyName == "Back")
        {
            Content1.Child = null;
        }
    }

    public void Closed()
    {
        App.AddGameWindow = null;
    }

    public void Opened()
    {
        Window.SetTitle(Title);
    }

    public void Install(CurseForgeModObj.Data data, CurseForgeObjList.Data data1)
    {
        (DataContext as AddGameModel)?.Install(data, data1);
    }

    public void Install(ModrinthVersionObj data, ModrinthSearchObj.Hit data1)
    {
        (DataContext as AddGameModel)?.Install(data, data1);
    }

    public void AddFile(string file, bool isDir)
    {
        if (isDir)
        {
            var model = (DataContext as AddGameModel)!;
            model.GoTab("Tab3");
            model.SetPath(file);
        }
        else
        {
            var model = (DataContext as AddGameModel)!;
            model.GoTab("Tab2");
            model.SetFile(file);
        }
    }

    public void SetGroup(string? group)
    {
        if (DataContext is AddGameModel model)
        {
            model.Group = group;
        }
    }
}
