using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Add;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddGameControl : BaseUserControl
{
    private AddGameTab1Control _tab1;
    private AddGameTab2Control _tab2;
    private AddGameTab3Control _tab3;
    private AddGameTab4Control _tab4;

    public AddGameControl()
    {
        InitializeComponent();

        Title = App.Lang("AddGameWindow.Title");
        UseName = ToString() ?? "AddGameControl";

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);
    }

    public override TopModel GenModel(BaseModel model)
    {
        var amodel = new AddGameModel(model);
        amodel.PropertyChanged += Model_PropertyChanged;
        return amodel;
    }

    public override void Closed()
    {
        WindowManager.AddGameWindow = null;
    }

    public override void Opened()
    {
        Window.SetTitle(Title);
    }
    public override Bitmap GetIcon()
    {
        return ImageManager.GameIcon;
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
            model.DefaultGroup ??= group;
            model.Group ??= group;
        }
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "GoTab1")
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
        else if (e.PropertyName == "GoDownload")
        {
            Content1.Child = _tab4 ??= new();
        }
        else if (e.PropertyName == "Back")
        {
            Content1.Child = null;
        }
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
                Label1.Text = App.Lang("AddGameWindow.Text2");
            }
            else if (item.Name.EndsWith(".zip") || item.Name.EndsWith(".mrpack"))
            {
                Grid2.IsVisible = true;
                Label1.Text = App.Lang("MainWindow.Text25");
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
}
