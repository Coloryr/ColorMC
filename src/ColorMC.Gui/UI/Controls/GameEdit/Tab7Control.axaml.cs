using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;
using System;
using System.Timers;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab7Control : UserControl
{
    private GameSettingObj Obj;

    private string temp = "";
    private Timer timer;

    public Tab7Control()
    {
        InitializeComponent();

        timer = new(TimeSpan.FromMilliseconds(100));
        timer.BeginInit();
        timer.Elapsed += Timer_Elapsed;
        timer.EndInit();

        CheckBox1.Click += CheckBox1_Click;

        Button1.Click += Button1_Click;

        //TextEditor1.PointerWheelChanged += TextEditor1_PointerWheelChanged;
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        BaseBinding.StopGame(Obj);
        Button1.IsEnabled = false;
    }

    private void CheckBox1_Click(object? sender, RoutedEventArgs e)
    {
        //TextEditor1.WordWrap = CheckBox1.IsChecked == true;
    }

    private void TextEditor1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        CheckBox2.IsChecked = false;
    }

    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            lock (temp)
            {
                if (!string.IsNullOrWhiteSpace(temp))
                {
                    //TextEditor1.AppendText(temp);
                    temp = "";
                }
            }

            if (CheckBox2.IsChecked == true)
            {
                //TextEditor1.ScrollToLine(TextEditor1.LineCount);
            }
        });
    }

    public void Clear()
    {
        Dispatcher.UIThread.Post(() =>
        {
            //TextEditor1.Text = "";
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

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;
    }

    public void Update()
    {
        if (Obj == null)
            return;

        Button1.IsEnabled = true;
    }
}

