using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using ColorMC.Gui.Utils.LaunchSetting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class Tab6Control : UserControl
{
    public Tab6Control()
    {
        InitializeComponent();
        TextBox4.LostFocus += TextBox4_LostFocus;
    }

    private void TextBox4_LostFocus(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TextBox4.Text))
            return;

        if (TextBox4.Text.EndsWith("/"))
            return;

        TextBox4.Text += "/";
    }
}
