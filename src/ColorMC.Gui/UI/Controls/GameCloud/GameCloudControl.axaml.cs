using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameCloud;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UI.Windows;
using System;
using System.ComponentModel;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.GameCloud;

public partial class GameCloudControl : UserControl, IUserControl
{
    private bool _switch1 = false;

    private readonly Tab1Control _tab1 = new();
    private readonly Tab2Control _tab2 = new();
    private readonly Tab3Control _tab3 = new();

    private CancellationTokenSource _cancel = new();

    private int _now;

    public GameSettingObj Obj { get; }

    public IBaseWindow Window => App.FindRoot(VisualRoot);
    public UserControl Con => this;
    public string Title =>
        string.Format(App.GetLanguage("GameCloudWindow.Title"), Obj.Name);

    public GameCloudControl()
    {
        InitializeComponent();
    }

    public GameCloudControl(GameSettingObj obj) : this()
    {
        Obj = obj;

        StackPanel1.PointerPressed += StackPanel1_PointerPressed;
        StackPanel2.PointerPressed += StackPanel2_PointerPressed;
    }

    private void StackPanel2_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as GameCloudModel)!.OpenSide();
    }

    private void StackPanel1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as GameCloudModel)!.CloseSide();
    }

    public void Opened()
    {
        Window.SetTitle(Title);

        Content1.Content = _tab1;
        (DataContext as GameCloudModel)!.Load();
    }

    private void Go(UserControl to)
    {
        _cancel.Cancel();
        _cancel.Dispose();

        _cancel = new();

        var model = (DataContext as GameCloudModel)!;

        if (!_switch1)
        {
            Content2.Content = to;
            _ = App.PageSlide500.Start(Content1, Content2, _now < model.NowView, _cancel.Token);
        }
        else
        {
            Content1.Content = to;
            _ = App.PageSlide500.Start(Content2, Content1, _now < model.NowView, _cancel.Token);
        }

        _switch1 = !_switch1;
    }

    public void Closed()
    {
        App.GameCloudWindows.Remove((DataContext as GameCloudModel)!.Obj.UUID);
    }

    public void SetBaseModel(BaseModel model)
    {
        var amodel = new GameCloudModel(model, Obj);
        amodel.PropertyChanged += Model_PropertyChanged;
        DataContext = amodel;
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "WindowClose")
        {
            Window.Close();
        }
        else if (e.PropertyName == "SideOpen")
        {
            App.CrossFade100.Start(null, StackPanel1);
        }
        else if (e.PropertyName == "SideClose")
        {
            StackPanel1.IsVisible = false;
        }
        else if (e.PropertyName == "NowView")
        {
            var model = (DataContext as GameCloudModel)!;
            switch (model.NowView)
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

            _now = model.NowView;
        }
    }

    public void GoWorld()
    {
        Dispatcher.UIThread.Post(() =>
        {
            var model = (DataContext as GameCloudModel)!;
            model.NowView = 2;
        });
    }
}
