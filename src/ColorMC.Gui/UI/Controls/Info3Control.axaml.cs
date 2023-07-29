using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls;

public partial class Info3Control : UserControl
{
    private readonly Semaphore _semaphore = new(0, 2);
    private Action? _cancelCall;
    private bool _display = false;

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
            Confirm();
        }
    }

    private void Button_Cancel_Click(object? sender, RoutedEventArgs e)
    {
        if (_cancelCall != null)
        {
            _cancelCall();
            Button_Cancel.IsEnabled = false;
            _cancelCall = null;
            return;
        }

        Cancel = true;
        _semaphore.Release();

        Close();
    }

    private void Button_Confirm_Click(object? sender, RoutedEventArgs e)
    {
        Confirm();
    }

    public void Confirm()
    {
        Cancel = false;
        _semaphore.Release();

        Close();
    }

    public void Close()
    {
        if (!_display)
            return;

        _display = false;

        App.CrossFade300.Start(this, null);
    }

    public async Task<(bool Cancel, string? Text1, string? Text2)>
        ShowInput(string title, string title1, bool password)
    {
        _display = true;

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
        App.CrossFade300.Start(null, this);

        _cancelCall = null;

        await Task.Run(() =>
       {
           _semaphore.WaitOne();
       });

        return (Cancel, TextBox_Text.Text, TextBox_Text1.Text);
    }

    public void Show(string title, string title1)
    {
        _display = true;

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

        App.CrossFade300.Start(null, this);
    }

    public void Show(string title, string title1, Action cancel)
    {
        _display = true;

        TextBox_Text1.IsReadOnly = TextBox_Text.IsReadOnly = true;
        TextBox_Text.Text = title;
        TextBox_Text1.Text = title1;

        TextBox_Text.Watermark = "";
        TextBox_Text1.Watermark = "";

        ProgressBar_Value.IsVisible = true;

        Button_Confirm.IsEnabled = false;
        Button_Confirm.IsVisible = false;

        _cancelCall = cancel;

        Button_Cancel.IsEnabled = true;
        Button_Cancel.IsVisible = true;

        TextBox_Text1.PasswordChar = (char)0;

        App.CrossFade300.Start(null, this);
    }

    public async Task<(bool Cancel, string? Text)> ShowOne(string title, bool lock1 = true)
    {
        _display = true;

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

            TextBox_Text1.PasswordChar = '\0';
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
        App.CrossFade300.Start(null, this);

        if (!lock1)
        {
            _cancelCall = null;
            await Task.Run(() =>
            {
                _semaphore.WaitOne();
            });
        }

        return (Cancel, TextBox_Text.Text);
    }

    public async Task<(bool Cancel, string? Text1, string? Text2)>
        ShowEdit(string title, string data)
    {
        _display = true;

        TextBox_Text1.IsVisible = false;
        TextBox_Text.IsReadOnly = false;
        ProgressBar_Value.IsVisible = false;

        TextBox_Text.Text = data;
        TextBox_Text.Watermark = title;
        App.CrossFade300.Start(null, this);

        _cancelCall = null;
        await Task.Run(() =>
       {
           _semaphore.WaitOne();

       });

        return (Cancel, TextBox_Text.Text, TextBox_Text1.Text);
    }
}
