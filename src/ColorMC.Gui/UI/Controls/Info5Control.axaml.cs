using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls;

public partial class Info5Control : UserControl
{
    private readonly Semaphore _semaphore = new(0, 2);
    private bool _display = false;

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
        _semaphore.Release();
        Close();
    }

    private void Button_Add_Click(object? sender, RoutedEventArgs e)
    {
        Cancel = false;
        _semaphore.Release();
        Close();
    }

    public void Close()
    {
        if (!_display)
            return;

        App.CrossFade300.Start(this, null, CancellationToken.None);
    }

    public async Task<(bool Cancel, int Index, string? Item)> 
        Show(string title, List<string> list)
    {
        _display = true;

        Label1.Content = title;
        ComboBox1.ItemsSource = list;
        ComboBox1.SelectedIndex = 0;

        _ = App.CrossFade300.Start(null, this, CancellationToken.None);

        await Task.Run(() =>
        {
            _semaphore.WaitOne();
        });

        return (Cancel, ComboBox1.SelectedIndex, ComboBox1.SelectedItem as string);
    }
}
