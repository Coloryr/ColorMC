using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Main;

public partial class GameGroupControl : UserControl
{
    private readonly Semaphore semaphore = new(0, 2);
    private ObservableCollection<string> List = new();
    public bool Cancel { get; private set; }
    public GameGroupControl()
    {
        InitializeComponent();

        Button_Cancel.Click += Button_Cancel_Click;
        Button_Confirm.Click += Button_Confirm_Click;
        Button_Add.Click += Button_Add_Click;

        ComboBox1.Items = List;
    }

    private async void Button_Add_Click(object? sender, RoutedEventArgs e)
    {
        await App.MainWindow!.AddGroup();
        List.Clear();
        List.AddRange(GameBinding.GetGameGroups().Keys);
    }

    private void Button_Confirm_Click(object? sender, RoutedEventArgs e)
    {
        Cancel = false;
        semaphore.Release();
    }

    private void Button_Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Cancel = true;
        semaphore.Release();
    }

    public Task Set(GameSettingObj obj)
    {
        List.Clear();
        List.AddRange(GameBinding.GetGameGroups().Keys);

        App.CrossFade300.Start(null, this, CancellationToken.None);

        ComboBox1.SelectedItem = obj.GroupName;

        return Task.Run(() =>
        {
            semaphore.WaitOne();
        });
    }

    public void Close()
    {
        App.CrossFade300.Start(this, null, CancellationToken.None);
    }
    public string? Read()
    {
        return ComboBox1.SelectedItem as string;
    }
}
