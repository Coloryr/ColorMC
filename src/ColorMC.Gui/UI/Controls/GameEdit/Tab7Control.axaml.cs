using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using System;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab7Control : UserControl
{
    private GameEditWindow Window;
    private GameSettingObj Obj;
    private ScrollViewer? scroll;
    public Tab7Control()
    {
        InitializeComponent();
    }

    public void Clear() 
    {
        Dispatcher.UIThread.Post(() =>
        {
            TextBox1.Text = "";
        });
    }

    public void Log(string data) 
    {
        Dispatcher.UIThread.Post(() =>
        {
            TextBox1.Text += data + Environment.NewLine;
            if (scroll == null)
            {
                scroll = TextBox1.FindToEnd<ScrollViewer>();
            }
            else
            {
                scroll.ScrollToEnd();
            }
        });
    }

    public void SetWindow(GameEditWindow window)
    {
        Window = window;
    }

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;
    }

    public void Update()
    {
        if (Obj == null)
            return;
    }
}


