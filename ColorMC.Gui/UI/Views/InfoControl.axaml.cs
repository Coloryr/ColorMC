using Avalonia.Animation;
using Avalonia.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Interactivity;

namespace ColorMC.Gui.UI.Views;

public partial class InfoControl : UserControl
{
    private Action<bool>? call;

    private CrossFade transition = new(TimeSpan.FromMilliseconds(300));

    public InfoControl()
    {
        InitializeComponent();

        Confirm.Click += Confirm_Click;
        Cancel.Click += Cancel_Click;
    }

    private async void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Cancel.IsEnabled = false;
        await transition.Start(this, null, CancellationToken.None);

        call?.Invoke(false);
    }

    private async void Confirm_Click(object? sender, RoutedEventArgs e)
    {
        Confirm.IsEnabled = false;
        Cancel.IsEnabled = false;
        await transition.Start(this, null, CancellationToken.None);

        call?.Invoke(true);
    }

    public async void Show(string title, Action<bool> res)
    {
        Confirm.IsEnabled = true;
        Cancel.IsEnabled = true;
        Confirm.IsVisible = true;
        Cancel.IsVisible = true;
        Text.Text = title;
        call = res;

        await transition.Start(null, this, cancellationToken: CancellationToken.None);
    }

    public async Task<bool> ShowWait(string title)
    {
        bool reut = false;
        Semaphore semaphore = new(0, 2);
        Confirm.IsEnabled = true;
        Cancel.IsEnabled = true;
        Confirm.IsVisible = true;
        Cancel.IsVisible = true;
        Text.Text = title;

        call = (res) =>
        {
            reut = res;
            semaphore.Release();
        };

        await transition.Start(null, this, cancellationToken: CancellationToken.None);

        await Task.Run(() => 
        {
            semaphore.WaitOne();
        });

        return reut;
    }

    public async void Show(string title)
    {
        Confirm.IsEnabled = true;
        Cancel.IsVisible = false;
        Text.Text = title;

        await transition.Start(null, this, cancellationToken: CancellationToken.None);
    }
}
