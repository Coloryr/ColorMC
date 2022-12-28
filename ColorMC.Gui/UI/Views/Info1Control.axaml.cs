using Avalonia.Animation;
using Avalonia.Controls;
using System;
using System.Threading;

namespace ColorMC.Gui.UI.Views;

public partial class Info1Control : UserControl
{
    private Action? call;

    private CrossFade transition = new(TimeSpan.FromMilliseconds(300));

    public Info1Control()
    {
        InitializeComponent();

        Cancel.Click += Cancel_Click;
    }

    private void Cancel_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Cancel.IsEnabled = false;
        transition.Start(this, null, CancellationToken.None);

        call?.Invoke();
    }

    public void Close()
    {
        Cancel.IsEnabled = false;
        transition.Start(this, null, CancellationToken.None);
    }

    public void Show(string title)
    {
        Cancel.IsEnabled = true;
        Text.Text = title;
        call = null;

        Cancel.IsVisible = false;

        transition.Start(null, this, cancellationToken: CancellationToken.None);
    }

    public void Show(string title, Action cancel)
    {
        Cancel.IsEnabled = true;
        Text.Text = title;
        call = cancel;

        transition.Start(null, this, cancellationToken: CancellationToken.None);
    }

    public void Progress(double value)
    {
        ProgressBar1.IsIndeterminate = false;
        ProgressBar1.Value = value;
        ProgressBar1.ShowProgressText = true;
    }

    public void NextText(string title)
    {
        Text.Text = title;
    }
}
