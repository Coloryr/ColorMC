using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls;

public partial class Info1Control : UserControl
{
    private Action? _call;
    private bool _display = false;

    public Info1Control()
    {
        InitializeComponent();

        Button_Cancel.Click += Cancel_Click;
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        _display = false;

        Button_Cancel.IsEnabled = false;
        App.CrossFade300.Start(this, null, CancellationToken.None);

        _call?.Invoke();
    }

    public void Close()
    {
        _display = false;

        Button_Cancel.IsEnabled = false;
        App.CrossFade300.Start(this, null, CancellationToken.None);
    }

    public Task CloseAsync()
    {
        _display = false;

        Button_Cancel.IsEnabled = false;
        return App.CrossFade300.Start(this, null, CancellationToken.None);
    }

    public void Show()
    {
        _display = true;

        App.CrossFade300.Start(null, this, CancellationToken.None);
    }

    public void Show(string title)
    {
        if (_display)
        {
            NextText(title);
        }
        _display = true;

        Button_Cancel.IsEnabled = true;
        TextBlock_Text.Text = title;
        _call = null;

        Button_Cancel.IsVisible = false;

        App.CrossFade300.Start(null, this, CancellationToken.None);
    }

    public Task ShowAsync(string title)
    {
        if (_display)
        {
            NextText(title);
            return Task.CompletedTask;
        }
        _display = true;

        Button_Cancel.IsEnabled = true;
        TextBlock_Text.Text = title;
        _call = null;

        Button_Cancel.IsVisible = false;

        return App.CrossFade300.Start(null, this, CancellationToken.None);
    }

    public void Show(string title, Action cancel)
    {
        Button_Cancel.IsEnabled = true;
        TextBlock_Text.Text = title;
        _call = cancel;

        App.CrossFade300.Start(null, this, CancellationToken.None);
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
