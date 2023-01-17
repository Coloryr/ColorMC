using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using System;
using System.Timers;
using Avalonia.Controls.Primitives;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab7Control : UserControl
{
    private GameEditWindow Window;
    private GameSettingObj Obj;
    private ScrollViewer? scroll;

    private string temp = "";
    private Timer timer;

    public Tab7Control()
    {
        InitializeComponent();

        timer = new(TimeSpan.FromMilliseconds(1000));
        timer.BeginInit();
        timer.Elapsed += Timer_Elapsed;
        timer.EndInit();
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            lock (temp)
            {
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    TextBox1.Text += temp;
                    temp = "";
                }
            }
            if (scroll == null)
            {
                scroll = TextBox1.FindToEnd<ScrollViewer>();
                if (scroll != null)
                {
                    scroll.ScrollChanged += Scroll_ScrollChanged;
                }
            }
            else
            {
                if (CheckBox1.IsChecked == true)
                {
                    scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                }
                else
                {
                    scroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
                }
                if (CheckBox2.IsChecked == true)
                {
                    scroll.ScrollToEnd();
                    Dispatcher.UIThread.Post(() =>
                    {
                        CheckBox2.IsChecked = true;
                    });
                }
            }
        });
    }

    private void Scroll_ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        CheckBox2.IsChecked = false;
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
        if (!timer.Enabled)
        {
            timer.Start();
        }
        lock (temp)
        {
            temp += data + Environment.NewLine;
        }
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


