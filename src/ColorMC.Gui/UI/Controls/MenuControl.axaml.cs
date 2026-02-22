using System;
using System.ComponentModel;
using System.Threading;
using Avalonia.Controls;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;

namespace ColorMC.Gui.UI.Controls;

/// <summary>
/// 带有左侧菜单的窗口
/// </summary>
public abstract partial class MenuControl : BaseUserControl
{
    private CancellationTokenSource _cancel = new();

    private bool _switch1 = false;

    private int _now = -1;

    public MenuControl() : base("")
    {
        InitializeComponent();
    }

    public MenuControl(string usename) : base(usename)
    {
        InitializeComponent();
    }

    protected abstract Control ViewChange(int old, int index);

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {

        if (e.PropertyName == MenuModel.NameNowView)
        {
            var model = (DataContext as MenuModel)!;
            Go(ViewChange(_now, model.NowView));
            _now = model.NowView;
        }
    }

    private void Go(Control to)
    {
        _cancel.Cancel();
        _cancel.Dispose();

        _cancel = new();

        var model = (DataContext as MenuModel)!;

        if (_now == -1)
        {
            Content1.Child = to;
            return;
        }

        SwitchTo(_switch1, to, _now < model.NowView, _cancel.Token);

        _switch1 = !_switch1;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is MenuModel model)
        {
            model.PropertyChanged += Model_PropertyChanged;
        }
    }

    public void SwitchTo(bool dir, Control control, bool dir1, CancellationToken token)
    {
        if (!dir)
        {
            Content2.Child = control;
            _ = ThemeManager.SelfPageSlideY.Start(Content1, Content2, dir1, token);
        }
        else
        {
            Content1.Child = control;
            _ = ThemeManager.SelfPageSlideY.Start(Content2, Content1, dir1, token);
        }
    }
}
