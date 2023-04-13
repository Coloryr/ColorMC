using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls;

public partial class Info3Control : UserControl
{
    private readonly Semaphore semaphore = new(0, 2);
    private Action? CancelCall;
    private bool Display = false;

    public bool Cancel { get; private set; }

    public Info3Control()
    {
        InitializeComponent();

        Button_Confirm.Click += Button_Confirm_Click;
        Button_Cancel.Click += Button_Cancel_Click;

        TextBox_Text1.KeyDown += TextBox_Text1_KeyDown;
    }

    private void TextBox_Text1_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            Button_Confirm_Click(null, null);
        }
    }

    private void Button_Cancel_Click(object? sender, RoutedEventArgs e)
    {
        if (CancelCall != null)
        {
            CancelCall();
            Button_Cancel.IsEnabled = false;
            CancelCall = null;
            return;
        }

        Cancel = true;
        semaphore.Release();

        Close();
    }

    private void Button_Confirm_Click(object? sender, RoutedEventArgs e)
    {
        Cancel = false;
        semaphore.Release();

        Close();
    }

    public void Close()
    {
        if (!Display)
            return;

        Display = false;

        App.CrossFade300.Start(this, null, CancellationToken.None);
    }

    public Task ShowInput(string title, string title1, bool password)
    {
        Display = true;

        TextBox_Text1.IsReadOnly = TextBox_Text.IsReadOnly = false;

        ProgressBar_Value.IsVisible = false;

        TextBox_Text.Text = "";
        TextBox_Text1.Text = "";

        TextBox_Text.Watermark = title;
        TextBox_Text1.Watermark = title1;

        Button_Confirm.IsEnabled = true;
        Button_Confirm.IsVisible = true;

        Button_Cancel.IsEnabled = true;
        Button_Cancel.IsVisible = true;

        TextBox_Text1.PasswordChar = password ? '*' : (char)0;
        App.CrossFade300.Start(null, this, CancellationToken.None);

        CancelCall = null;

        return Task.Run(() =>
        {
            semaphore.WaitOne();
        });
    }

    public void Show(string title, string title1)
    {
        Display = true;

        TextBox_Text1.IsReadOnly = TextBox_Text.IsReadOnly = true;
        TextBox_Text.Text = title;
        TextBox_Text1.Text = title1;

        TextBox_Text.Watermark = "";
        TextBox_Text1.Watermark = "";

        ProgressBar_Value.IsVisible = true;

        Button_Confirm.IsEnabled = false;
        Button_Confirm.IsVisible = false;

        Button_Cancel.IsEnabled = false;
        Button_Cancel.IsVisible = false;

        TextBox_Text1.PasswordChar = (char)0;

        App.CrossFade300.Start(null, this, CancellationToken.None);
    }

    public void Show(string title, string title1, Action cancel)
    {
        Display = true;

        TextBox_Text1.IsReadOnly = TextBox_Text.IsReadOnly = true;
        TextBox_Text.Text = title;
        TextBox_Text1.Text = title1;

        TextBox_Text.Watermark = "";
        TextBox_Text1.Watermark = "";

        ProgressBar_Value.IsVisible = true;

        Button_Confirm.IsEnabled = false;
        Button_Confirm.IsVisible = false;

        CancelCall = cancel;

        Button_Cancel.IsEnabled = true;
        Button_Cancel.IsVisible = true;

        TextBox_Text1.PasswordChar = (char)0;

        App.CrossFade300.Start(null, this, CancellationToken.None);
    }

    public Task ShowOne(string title, bool lock1 = true)
    {
        Display = true;

        TextBox_Text1.IsVisible = false;
        TextBox_Text1.IsReadOnly = TextBox_Text.IsReadOnly = lock1;
        if (lock1)
        {
            TextBox_Text.Text = title;
            TextBox_Text.Watermark = "";

            ProgressBar_Value.IsVisible = true;

            Button_Confirm.IsEnabled = false;
            Button_Confirm.IsVisible = false;

            Button_Cancel.IsEnabled = false;
            Button_Cancel.IsVisible = false;

            TextBox_Text1.PasswordChar = (char)0;
        }
        else
        {
            TextBox_Text.Text = "";
            ProgressBar_Value.IsVisible = false;

            TextBox_Text.Watermark = title;

            Button_Confirm.IsEnabled = true;
            Button_Confirm.IsVisible = true;

            Button_Cancel.IsEnabled = true;
            Button_Cancel.IsVisible = true;
        }
        App.CrossFade300.Start(null, this, CancellationToken.None);

        if (!lock1)
        {
            CancelCall = null;
            return Task.Run(() =>
            {
                semaphore.WaitOne();
            });
        }

        return Task.CompletedTask;
    }

    public Task ShowEdit(string title, string data)
    {
        Display = true;

        TextBox_Text1.IsVisible = false;
        TextBox_Text.IsReadOnly = false;
        ProgressBar_Value.IsVisible = false;

        TextBox_Text.Text = data;
        TextBox_Text.Watermark = title;
        App.CrossFade300.Start(null, this, CancellationToken.None);

        CancelCall = null;
        return Task.Run(() =>
        {
            semaphore.WaitOne();
        });
    }

    public (string?, string?) Read()
    {
        return (TextBox_Text.Text, TextBox_Text1.Text);
    }
}
