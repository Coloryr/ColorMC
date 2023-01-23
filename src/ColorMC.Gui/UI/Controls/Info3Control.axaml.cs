using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls;

public partial class Info3Control : UserControl
{
    private readonly Semaphore semaphore = new(0, 2);

    public bool Cancel { get; private set; }

    public Info3Control()
    {
        InitializeComponent();

        Button_Confirm.Click += Button_Add_Click;
        Button_Cancel.Click += Button_Cancel_Click;
    }

    private void Button_Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Cancel = true;
        semaphore.Release();
    }

    private void Button_Add_Click(object? sender, RoutedEventArgs e)
    {
        Cancel = false;
        semaphore.Release();
    }

    public void Close()
    {
        App.CrossFade300.Start(this, null, CancellationToken.None);
    }

    public Task Show(string title, string title1, bool lock1 = true)
    {
        TextBox_Text1.IsReadOnly = TextBox_Text.IsReadOnly = lock1;
        if (lock1)
        {
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
        }
        else
        {
            ProgressBar_Value.IsVisible = false;

            TextBox_Text.Watermark = title;
            TextBox_Text1.Watermark = title1;

            Button_Confirm.IsEnabled = true;
            Button_Confirm.IsVisible = true;

            Button_Cancel.IsEnabled = true;
            Button_Cancel.IsVisible = true;

            TextBox_Text1.PasswordChar = '*';
        }
        App.CrossFade300.Start(null, this, cancellationToken: CancellationToken.None);

        if (!lock1)
        {
            return Task.Run(() =>
            {
                semaphore.WaitOne();
            });
        }

        return Task.CompletedTask;
    }

    public Task ShowOne(string title, bool lock1 = true)
    {
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
        App.CrossFade300.Start(null, this, cancellationToken: CancellationToken.None);

        if (!lock1)
        {
            return Task.Run(() =>
            {
                semaphore.WaitOne();
            });
        }

        return Task.CompletedTask;
    }

    public (string?, string?) Read()
    {
        return (TextBox_Text.Text, TextBox_Text1.Text);
    }
}
