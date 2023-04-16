using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls;

public partial class Info4Control : UserControl
{
    private Action<bool>? call;

    public Info4Control()
    {
        InitializeComponent();

        Button_Confirm.Click += Confirm_Click;
        Button_Cancel.Click += Cancel_Click;
    }

    private async void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Button_Cancel.IsEnabled = false;
        await App.CrossFade300.Start(this, null, CancellationToken.None);

        call?.Invoke(false);
    }

    private async void Confirm_Click(object? sender, RoutedEventArgs e)
    {
        Button_Confirm.IsEnabled = false;
        Button_Cancel.IsEnabled = false;
        await App.CrossFade300.Start(this, null, CancellationToken.None);

        call?.Invoke(true);
    }

    public async void Show(string title, Action<bool> res)
    {
        Button_Confirm.IsEnabled = true;
        Button_Cancel.IsEnabled = true;
        Button_Confirm.IsVisible = true;
        Button_Cancel.IsVisible = true;
        TextBlock_Text.Text = title;
        call = res;

        await App.CrossFade300.Start(null, this, CancellationToken.None);
    }

    public async Task<bool> ShowWait(string title)
    {
        bool reut = false;
        using Semaphore semaphore = new(0, 2);
        Button_Confirm.IsEnabled = true;
        Button_Cancel.IsEnabled = true;
        Button_Confirm.IsVisible = true;
        Button_Cancel.IsVisible = true;
        TextBlock_Text.Text = title;

        call = (res) =>
        {
            reut = res;
            semaphore.Release();
        };

        await App.CrossFade300.Start(null, this, CancellationToken.None);

        await Task.Run(() =>
        {
            semaphore.WaitOne();
        });

        call = null;

        return reut;
    }

    public async void Show(string title)
    {
        Button_Confirm.IsEnabled = true;
        Button_Cancel.IsVisible = false;
        TextBlock_Text.Text = title;

        await App.CrossFade300.Start(null, this, CancellationToken.None);
    }

    public void ShowOk(string title, Action action)
    {
        using Semaphore semaphore = new(0, 2);
        Button_Confirm.IsEnabled = true;
        Button_Cancel.IsEnabled = false;
        Button_Confirm.IsVisible = true;
        Button_Cancel.IsVisible = false;
        TextBlock_Text.Text = title;

        call = (res) =>
        {
            action.Invoke();
        };

        App.CrossFade300.Start(null, this, CancellationToken.None);
    }
}
