using Avalonia.Controls;
using Avalonia.Input;
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

    private readonly ContentControl content1 = new();
    private readonly ContentControl content2 = new();
    private CancellationTokenSource cancel = new();

    private int now;

    private AddGameTab1Model model1;
    private AddGameTab2Model model2;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public AddGameControl()
    {
        InitializeComponent();

        model1 = new(this);
        model2 = new(this);

        tab1.DataContext = model1;
        tab2.DataContext = model2;

        Tabs.SelectionChanged += Tabs_SelectionChanged;

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);

        Tab1.Children.Add(content1);
        Tab1.Children.Add(content2);

        content1.Content = tab1;
    }

    private void DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files))
        {
            Grid2.IsVisible = true;

            var files = e.Data.GetFiles();
            if (files == null)
                return;

            if (files.Count() == 1)
            {
                Label1.Content = App.GetLanguage("AddGameWindow.Text1");
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
            content2.Content = to;
            App.PageSlide500.Start(content1, content2, now < Tabs.SelectedIndex, cancel.Token);
        }
        else
        {
            content1.Content = to;
            App.PageSlide500.Start(content2, content1, now < Tabs.SelectedIndex, cancel.Token);
        }

        switch1 = !switch1;
        Tabs.IsEnabled = true;
    }

    public void Opened()
    {
        Window.SetTitle(App.GetLanguage("AddGameWindow.Title"));

    }

    public void Install(CurseForgeObjList.Data.LatestFiles data, CurseForgeObjList.Data data1)
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
