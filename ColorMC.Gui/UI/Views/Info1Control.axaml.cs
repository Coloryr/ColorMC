using Avalonia.Animation;
using Avalonia.Controls;
using System;
using System.Threading;
using Avalonia.Interactivity;

namespace ColorMC.Gui.UI.Views;

public partial class Info1Control : UserControl
{
    private Action? call;

    private readonly static CrossFade transition = new(TimeSpan.FromMilliseconds(300));

    public Info1Control()
    {
        InitializeComponent();

        Button_Cancel.Click += Cancel_Click;
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Button_Cancel.IsEnabled = false;
        transition.Start(this, null, CancellationToken.None);

        call?.Invoke();
    }

    public void Close()
    {
        Button_Cancel.IsEnabled = false;
        transition.Start(this, null, CancellationToken.None);
    }

    public void Show()
    {
        transition.Start(null, this, CancellationToken.None);
    }

    public void Show(string title)
    {
        Button_Cancel.IsEnabled = true;
        TextBlock_Text.Text = title;
        call = null;

        Button_Cancel.IsVisible = false;

        transition.Start(null, this, CancellationToken.None);
    }

    public void Show(string title, Action cancel)
    {
        Button_Cancel.IsEnabled = true;
        TextBlock_Text.Text = title;
        call = cancel;

        transition.Start(null, this, CancellationToken.None);
    }

    public void Progress(double value)
    {
        if (value == -1)
        {
            ProgressBar_Value.IsIndeterminate = true;
            ProgressBar_Value.ShowProgressText = false;
        }
        else
        {
            ProgressBar_Value.IsIndeterminate = false;
            ProgressBar_Value.Value = value;
            ProgressBar_Value.ShowProgressText = true;
        }
    }

    public void NextText(string title)
    {
        TextBlock_Text.Text = title;
    }
}
