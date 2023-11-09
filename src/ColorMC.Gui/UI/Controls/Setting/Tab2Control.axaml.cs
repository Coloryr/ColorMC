using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model.Setting;
using System;
using System.ComponentModel;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class Tab2Control : UserControl
{
    public Tab2Control()
    {
        InitializeComponent();

        if (SystemInfo.Os == OsType.Android)
        {
            var con = ColorMCGui.PhoneGetSetting?.Invoke();
            if (con is Control con1)
            {
                PhoneSetting.Children.Add(con1);
            }
        }
    }

    public void Reset()
    {
        ScrollViewer1.ScrollToHome();
    }

    private void ScrollViewer1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (DataContext is SettingModel model && model.NowView == 0)
        {
            if (e.Delta.Y < 0 && ScrollViewer1.ScrollBarMaximum == ScrollViewer1.Offset)
            {
                model.NowView++;
            }
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ScrollViewer1.PointerWheelChanged += ScrollViewer1_PointerWheelChanged;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ScrollViewer1.PointerWheelChanged -= ScrollViewer1_PointerWheelChanged;
    }
}
