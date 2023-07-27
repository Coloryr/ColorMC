using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.GameExport;

public partial class GameExportControl : UserControl, IUserControl
{
    private bool _switch1 = false;

    private readonly Tab1Control _tab1 = new();

    private CancellationTokenSource _cancel = new();

    private int _now;

    public IBaseWindow Window => App.FindRoot(VisualRoot);
    public UserControl Con => this;
    public string Title =>
        string.Format(App.GetLanguage("GameEditWindow.Title"), "");

    public GameExportControl() : this(new() { Empty = true })
    {

    }

    public GameExportControl(GameSettingObj obj)
    {
        InitializeComponent();

        if (!obj.Empty)
        {
        }

        //Tabs.SelectionChanged += Tabs_SelectionChanged;

        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;

        Content1.Content = _tab1;
    }

    public async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            switch (Tabs.SelectedIndex)
            {

            }
        }
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (e.Delta.Y > 0)
        {
            ScrollViewer1.LineLeft();
            ScrollViewer1.LineLeft();
            ScrollViewer1.LineLeft();
            ScrollViewer1.LineLeft();
            ScrollViewer1.LineLeft();
        }
        else if (e.Delta.Y < 0)
        {
            ScrollViewer1.LineRight();
            ScrollViewer1.LineRight();
            ScrollViewer1.LineRight();
            ScrollViewer1.LineRight();
            ScrollViewer1.LineRight();
        }
    }

    public void Opened()
    {
        Window.SetTitle(Title);
    }
}
