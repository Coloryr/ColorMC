using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using ColorMC.Core;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.UI.Controls.Add.AddGame;
using ColorMC.Gui.UI.Model.Add;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System.Linq;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddGameControl : UserControl, IUserControl
{
    private bool switch1 = false;

    private readonly Tab1Control tab1 = new();
    private readonly Tab2Control tab2 = new();
    private readonly Tab3Control tab3 = new();

    private CancellationTokenSource cancel = new();

    private int now;

    private readonly AddGameTab1Model model1;
    private readonly AddGameTab2Model model2;
    private readonly AddGameTab3Model model3;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public UserControl Con => this;

    public string Title => App.GetLanguage("AddGameWindow.Title");

    public AddGameControl()
    {
        InitializeComponent();

        model1 = new(this);
        model2 = new(this);
        model3 = new(this);

        tab1.DataContext = model1;
        tab2.DataContext = model2;
        tab3.DataContext = model3;

        Tabs.SelectionChanged += Tabs_SelectionChanged;

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);

        Content1.Content = tab1;
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

            var item = BaseBinding.GetPath(files.First());
            if (item?.EndsWith(".zip") == true || item?.EndsWith(".mrpack") == true)
            {
                Tabs.SelectedIndex = 1;
                model2.AddFile(item);
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
                Go(tab1);
                break;
            case 1:
                Go(tab2);
                break;
            case 2:
                Go(tab3);
                break;
        }

        now = Tabs.SelectedIndex;
    }

    private void Go(UserControl to)
    {
        cancel.Cancel();
        cancel.Dispose();

        cancel = new();
        Tabs.IsEnabled = false;

        if (!switch1)
        {
            Content2.Content = to;
            _ = App.PageSlide500.Start(Content1, Content2, now < Tabs.SelectedIndex, cancel.Token);
        }
        else
        {
            Content1.Content = to;
            _ = App.PageSlide500.Start(Content2, Content1, now < Tabs.SelectedIndex, cancel.Token);
        }

        switch1 = !switch1;
        Tabs.IsEnabled = true;
    }

    public void Opened()
    {
        Window.SetTitle(Title);
    }

    public void Install(CurseForgeModObj.Data data, CurseForgeObjList.Data data1)
    {
        model1.Install(data, data1);
    }

    public void Install(ModrinthVersionObj data, ModrinthSearchObj.Hit data1)
    {
        model1.Install(data, data1);
    }

    public void AddFile(string file)
    {
        if (file.EndsWith(".zip") == true || file.EndsWith(".mrpack") == true)
        {
            Tabs.SelectedIndex = 1;
            model2.AddFile(file);
        }
    }
}
