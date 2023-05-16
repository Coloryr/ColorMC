using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls;

public partial class Info5Control : UserControl
{
    private readonly Semaphore semaphore = new(0, 2);
    private bool Display = false;

    public bool Cancel { get; private set; }

    public Info5Control()
    {
        InitializeComponent();

        Button_Confirm.Click += Button_Add_Click;
        Button_Cancel.Click += Button_Cancel_Click;
    }
    private void Button_Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Cancel = true;
        semaphore.Release();
        Close();
    }

    private void Button_Add_Click(object? sender, RoutedEventArgs e)
    {
        Cancel = false;
        semaphore.Release();
        Close();
    }

    public void Close()
    {
        if (!Display)
            return;

        App.CrossFade300.Start(this, null, CancellationToken.None);
    }

    public Task Show(string title, List<string> list)
    {
        Display = true;

        Label1.Content = title;
        ComboBox1.ItemsSource = list;
        ComboBox1.SelectedIndex = 0;

        App.CrossFade300.Start(null, this, CancellationToken.None);

        return Task.Run(() =>
        {
            semaphore.WaitOne();
        });
    }

    public (int, string?) Read()
    {
        return (ComboBox1.SelectedIndex, ComboBox1.SelectedItem as string);
    }
}
