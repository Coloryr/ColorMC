using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.Server;

public partial class ServerPackControl : UserControl, IUserControl
{
    private Tab1Control tab1 = new();

    private bool switch1 = false;

    private readonly ContentControl content1 = new();
    private readonly ContentControl content2 = new();

    private CancellationTokenSource cancel = new();

    private int now;

    public IBaseWindow Window => (VisualRoot as IBaseWindow)!;

    private GameSettingObj Obj;
    private ServerPackObj Obj1;

    public ServerPackControl()
    {
        InitializeComponent();

        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;

        Tabs.SelectionChanged += Tabs_SelectionChanged;
        Tab1.Children.Add(content1);
        Tab1.Children.Add(content2);

        content1.Content = tab1;

        tab1.Load();
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
                tab1.Load();

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
            App.PageSlide500.Start(content1, content2, now < Tabs.SelectedIndex,
                cancel.Token);
        }
        else
        {
            content1.Content = to;
            App.PageSlide500.Start(content2, content1, now < Tabs.SelectedIndex,
                cancel.Token);
        }

        switch1 = !switch1;
        Tabs.IsEnabled = true;
    }

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;

        Obj1 = GameBinding.GetServerPack(obj);
        if (Obj1 == null)
        {
            Obj1 = new();
            GameBinding.SaveServerPack(obj, Obj1);
        }

        tab1.SetObj(Obj1);
    }

    public void Closed()
    {
        content1.Content = null;
        content2.Content = null;
    }

    public void Save()
    {
        GameBinding.SaveServerPack(Obj, Obj1);
    }
}
