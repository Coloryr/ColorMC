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
using System.Threading;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddGameControl : UserControl, IUserControl
{
    private bool _switch1 = false;

    private readonly Tab1Control _tab1 = new();
    private readonly Tab2Control _tab2 = new();
    private readonly Tab3Control _tab3 = new();

    private CancellationTokenSource _cancel = new();

    private int _now;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public UserControl Con => this;

    public string Title => App.GetLanguage("AddGameWindow.Title");

    public AddGameControl()
    {
        InitializeComponent();

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);

        StackPanel1.PointerPressed += StackPanel1_PointerPressed;
        StackPanel2.PointerPressed += StackPanel2_PointerPressed;
        Content1.Content = _tab1;
    }

    private void StackPanel2_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as AddGameModel)!.OpenSide();
    }

    private void StackPanel1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as AddGameModel)!.CloseSide();
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
                model.NowView = 1;
                model.AddFile(item);
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
        else if (e.PropertyName == "NowView")
        {
            var model = (DataContext as AddGameModel)!;
            switch (model.NowView)
            {
                case 0:
                    Go(_tab1, model.NowView);
                    break;
                case 1:
                    Go(_tab2, model.NowView);
                    break;
                case 2:
                    Go(_tab3, model.NowView);
                    break;
            }

            _now = model.NowView;
        }
        else if (e.PropertyName == "SideOpen")
        {
            App.CrossFade100.Start(null, StackPanel1);
        }
        else if (e.PropertyName == "SideClose")
        {
            StackPanel1.IsVisible = false;
        }
    }

    public void Closed()
    {
        ColorMCCore.PackState = null;
        ColorMCCore.PackUpdate = null;
        ColorMCCore.GameOverwirte = null;

        App.AddGameWindow = null;
    }

    private void Go(UserControl to, int now)
    {
        _cancel.Cancel();
        _cancel.Dispose();

        _cancel = new();

        if (!_switch1)
        {
            Content2.Content = to;
            _ = App.PageSlide500.Start(Content1, Content2, _now < now, _cancel.Token);
        }
        else
        {
            Content1.Content = to;
            _ = App.PageSlide500.Start(Content2, Content1, _now < now, _cancel.Token);
        }

        _switch1 = !_switch1;
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
            model.NowView = 1;
            model.AddFile(file);
        }
    }
}
