using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Core;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.UI.Controls.Add.AddGame;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Add.AddGame;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.Utils;
using System.ComponentModel;
using System.Linq;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddGameControl : UserControl, IUserControl
{
    private readonly Tab1Control _tab1 = new();
    private readonly Tab2Control _tab2 = new();
    private readonly Tab3Control _tab3 = new();

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public UserControl Con => this;

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
            Grid2.IsVisible = true;
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

            var item = files.ToList()[0].GetPath();
            if (item?.EndsWith(".zip") == true || item?.EndsWith(".mrpack") == true)
            {
                var model = (DataContext as AddGameModel)!;
                model.GoTab("Tab2");
                model.SetFile(item);
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
            Content1.Content = _tab1;
        }
        else if (e.PropertyName == "GoTab2")
        {
            Content1.Content = _tab2;
        }
        else if (e.PropertyName == "GoTab3")
        {
            Content1.Content = _tab3;
        }
        else if (e.PropertyName == "Back")
        {
            Content1.Content = null;
        }
    }

    public void Closed()
    {
        ColorMCCore.PackState = null;
        ColorMCCore.PackUpdate = null;
        ColorMCCore.GameOverwirte = null;

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

    public void AddFile(string file)
    {
        if (file.EndsWith(".zip") == true || file.EndsWith(".mrpack") == true)
        {
            var model = (DataContext as AddGameModel)!;
            model.GoTab("Tab2");
            model.SetFile(file);
        }
    }
}
