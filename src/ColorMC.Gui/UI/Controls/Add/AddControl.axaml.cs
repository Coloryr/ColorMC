using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Add;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.Utils;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddControl : UserControl, IUserControl, IAddWindow
{
    public GameSettingObj Obj { get; private set; }

    private readonly AddControlModel model;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public AddControl() : this(new() { Empty = true })
    {

    }

    public AddControl(GameSettingObj obj)
    {
        Obj = obj;

        InitializeComponent();

        model = new AddControlModel(this, obj);
        model.PropertyChanged += Model_PropertyChanged;

        DataContext = model;

        DataGridFiles.DoubleTapped += DataGridFiles_DoubleTapped;

        DataGrid1.DoubleTapped += DataGrid1_DoubleTapped;
        DataGrid2.DoubleTapped += DataGrid2_DoubleTapped;

        Grid1.PointerPressed += Grid1_PointerPressed;
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "OptifineDisplay")
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (model.OptifineDisplay == true)
                {
                    App.CrossFade300.Start(null, Grid2, CancellationToken.None);
                }
                else
                {
                    App.CrossFade300.Start(Grid2, null, CancellationToken.None);
                }
            });
        }
        else if (e.PropertyName == "ModDownloadDisplay")
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (model.ModDownloadDisplay == true)
                {
                    App.CrossFade300.Start(null, Grid4, CancellationToken.None);
                }
                else
                {
                    App.CrossFade300.Start(Grid4, null, CancellationToken.None);
                }
            });
        }
        else if (e.PropertyName == "VersionDisplay")
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (model.VersionDisplay == true)
                {
                    App.CrossFade300.Start(null, Grid1, CancellationToken.None);
                }
                else
                {
                    App.CrossFade300.Start(Grid1, null, CancellationToken.None);
                }
            });
        }
        else if (e.PropertyName == "DisplayList")
        {
            ScrollViewer1.ScrollToHome();
        }
    }

    private void Grid1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsXButton1Pressed)
        {
            model.VersionClose();
            e.Handled = true;
        }
    }
    private void DataGrid2_DoubleTapped(object? sender, TappedEventArgs e)
    {
        var item = model.Mod;
        if (item != null)
        {
            item.Download = !item.Download;
        }
    }

    private async void DataGrid1_DoubleTapped(object? sender, TappedEventArgs e)
    {
        await model.DownloadOptifine();
    }

    private async void DataGridFiles_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        await model.GoFile();
    }

    public void Closed()
    {
        model.DisplayList.Clear();

        App.AddWindows.Remove(Obj.UUID);

        if (model.Set)
            model.Set = false;
    }

    public void SetSelect(FileItemModel last)
    {
        model.SetSelect(last);
    }

    public void Install(FileItemModel item)
    {
        model.Install();
    }

    public void GoFile(SourceType type, string pid)
    {
        model.GoFile(type, pid);
    }

    public void Opened()
    {
        Window.SetTitle(string.Format(App.GetLanguage("AddWindow.Title"), Obj.Name));

        DataGridFiles.SetFontColor();

        model.display = true;
    }

    public async Task GoSet()
    {
        model.Set = true;

        model.Type = (int)FileType.Mod - 1;
        model.DownloadSource = 0;
        await Task.Run(() =>
        {
            while (model.Set)
                Thread.Sleep(1000);
        });
    }

    public void Back()
    {
        model.Back();
    }

    public void Next()
    {
        model.Next();
    }

    public void GoTo(FileType type)
    {
        model.GoTo(type);
    }
}
