using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Core;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.UI.Controls.Add.AddGame;
using ColorMC.Gui.UI.Model.Add;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
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

    private readonly AddGameTab1Model _model1;
    private readonly AddGameTab2Model _model2;
    private readonly AddGameTab3Model _model3;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public UserControl Con => this;

    public string Title => App.GetLanguage("AddGameWindow.Title");

    public AddGameControl()
    {
        InitializeComponent();

        _model1 = new(this);
        _model2 = new(this);
        _model3 = new(this);

        _tab1.DataContext = _model1;
        _tab2.DataContext = _model2;
        _tab3.DataContext = _model3;

        Tabs.SelectionChanged += Tabs_SelectionChanged;

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);

        Content1.Content = _tab1;
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
                Tabs.SelectedIndex = 1;
                _model2.AddFile(item);
            }
        }
    }

    public void Closed()
    {
        ColorMCCore.PackState = null;
        ColorMCCore.PackUpdate = null;
        ColorMCCore.GameOverwirte = null;

        App.AddGameWindow = null;
    }

    private void Tabs_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        switch (Tabs.SelectedIndex)
        {
            case 0:
                Go(_tab1);
                break;
            case 1:
                Go(_tab2);
                break;
            case 2:
                Go(_tab3);
                break;
        }

        _now = Tabs.SelectedIndex;
    }

    private void Go(UserControl to)
    {
        _cancel.Cancel();
        _cancel.Dispose();

        _cancel = new();
        Tabs.IsEnabled = false;

        if (!_switch1)
        {
            Content2.Content = to;
            _ = App.PageSlide500.Start(Content1, Content2, _now < Tabs.SelectedIndex, _cancel.Token);
        }
        else
        {
            Content1.Content = to;
            _ = App.PageSlide500.Start(Content2, Content1, _now < Tabs.SelectedIndex, _cancel.Token);
        }

        _switch1 = !_switch1;
        Tabs.IsEnabled = true;
    }

    public void Opened()
    {
        Window.SetTitle(Title);
    }

    public void Install(CurseForgeModObj.Data data, CurseForgeObjList.Data data1)
    {
        _model1.Install(data, data1);
    }

    public void Install(ModrinthVersionObj data, ModrinthSearchObj.Hit data1)
    {
        _model1.Install(data, data1);
    }

    public void AddFile(string file)
    {
        if (file.EndsWith(".zip") == true || file.EndsWith(".mrpack") == true)
        {
            Tabs.SelectedIndex = 1;
            _model2.AddFile(file);
        }
    }
}
