using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.ServerPack;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.ServerPack;

public partial class ServerPackControl : UserControl, IUserControl
{
    private readonly Tab1Control tab1 = new();
    private readonly Tab2Control tab2 = new();
    private readonly Tab3Control tab3 = new();
    private readonly Tab4Control tab4 = new();

    private readonly ServerPackTab1Model model1;
    private readonly ServerPackTab2Model model2;
    private readonly ServerPackTab3Model model3;
    private readonly ServerPackTab4Model model4;

    private readonly ServerPackModel model;

    private bool switch1 = false;

    private CancellationTokenSource cancel = new();

    private int now;

    public string GameName => model1.Obj.Game.Name;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public UserControl Con => this;

    public string Title => string.Format(App.GetLanguage("ServerPackWindow.Title"),
            model1.Obj.Game.Name);

    public ServerPackControl() : this(new() { Empty = true })
    {

    }

    public ServerPackControl(GameSettingObj obj)
    {
        InitializeComponent();

        if (!obj.Empty)
        {
            var pack = GameBinding.GetServerPack(obj);
            if (pack == null)
            {
                pack = new()
                {
                    Game = obj,
                    Mod = new(),
                    Resourcepack = new(),
                    Config = new()
                };

                GameBinding.SaveServerPack(pack);
            }

            model = new(this, pack);
            DataContext = model;

            model1 = new(this, pack);
            tab1.DataContext = model1;

            model2 = new(this, pack);
            tab2.DataContext = model2;

            model3 = new(this, pack);
            tab3.DataContext = model3;

            model4 = new(this, pack);
            tab4.DataContext = model4;
        }

        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;

        Tabs.SelectionChanged += Tabs_SelectionChanged;

        Content1.Content = tab1;
    }

    public void Opened()
    {
        Window.SetTitle(Title);
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (e.Delta.Y > 0)
        {
            ScrollViewer1.LineLeft();
        }
        else if (e.Delta.Y < 0)
        {
            ScrollViewer1.LineRight();
        }
    }

    private void Tabs_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        switch (Tabs.SelectedIndex)
        {
            case 0:
                Go(tab1);
                model1.Load();
                break;
            case 1:
                Go(tab2);
                model2.Load();
                break;
            case 2:
                Go(tab3);
                model3.Load();
                break;
            case 3:
                Go(tab4);
                model4.Load();
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

    public void Closed()
    {
        App.ServerPackWindows.Remove(model1.Obj.Game.UUID);
    }
}
