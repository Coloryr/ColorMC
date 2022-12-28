using Avalonia.Animation;
using Avalonia.Controls;
using System;
using System.Threading;
using Avalonia.Interactivity;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Views;

public partial class Info3Control : UserControl
{
    private CrossFade transition = new(TimeSpan.FromMilliseconds(300));
    private Semaphore semaphore = new(0,2);

    public Info3Control()
    {
        InitializeComponent();

        Button_Add.Click += Button_Add_Click;
    }

    private void Button_Add_Click(object? sender, RoutedEventArgs e)
    {
        semaphore.Release();
    }

    public void Close()
    {
        transition.Start(this, null, CancellationToken.None);
    }

    public Task Show(string title, string title1, bool lock1 = true)
    {
        Text1.IsReadOnly = Text.IsReadOnly = lock1;
        if (lock1)
        {
            Text.Text = title;
            Text1.Text = title1;

            Text.Watermark = "";
            Text1.Watermark = "";

            ProgressBar1.IsVisible = true;

            Button_Add.IsEnabled = false;
            Button_Add.IsVisible = false;

            Text1.PasswordChar = (char)0;
        }
        else
        {
            ProgressBar1.IsVisible = false;

            Text.Watermark = title;
            Text1.Watermark = title1;

            Button_Add.IsEnabled = true;
            Button_Add.IsVisible = true;

            Text1.PasswordChar = '*';
        }
        transition.Start(null, this, cancellationToken: CancellationToken.None);

        if (!lock1)
        {
            return Task.Run(() =>
            {
                semaphore.WaitOne();
            });
        }

        return Task.CompletedTask;
    }

    public (string, string) Read()
    {
        return (Text.Text, Text1.Text);
    }
}
