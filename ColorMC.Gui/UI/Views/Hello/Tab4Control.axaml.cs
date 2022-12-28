using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Utils;
using ColorMC.Gui.UIBinding;
using DynamicData;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Views.Hello;

public partial class Tab4Control : UserControl
{
    private HelloWindow Window;

    private ObservableCollection<string> List = new();
    public Tab4Control()
    {
        InitializeComponent();

        GameVersion.Items = List;

        Button_Next.Click += Button_Next_Click;
        Release.Click += Release_Click;
        Snapshot.Click += Snapshot_Click;
        Other.Click += Other_Click;

        Button_Add1.Click += Button_Add1_Click;
        Button_Add2.Click += Button_Add2_Click;
        Button_Add3.Click += Button_Add3_Click;
        Button_Add4.Click += Button_Add4_Click;

        Load();
    }

    private async void Button_Add4_Click(object? sender, RoutedEventArgs e)
    {
        var name = await SelectPack();
        if (name == null)
            return;

        CloseButton();
        await OtherBinding.AddPack(name, PackType.HMCL);
    }

    private async void Button_Add3_Click(object? sender, RoutedEventArgs e)
    {
        var name = await SelectPack();
        if (name == null)
            return;

        CloseButton();
        await OtherBinding.AddPack(name, PackType.MMC);
    }

    private async void Button_Add2_Click(object? sender, RoutedEventArgs e)
    {
        var name = await SelectPack();
        if (name == null)
            return;

        CloseButton();
        await OtherBinding.AddPack(name, PackType.CurseForge);
    }

    private async void Button_Add1_Click(object? sender, RoutedEventArgs e)
    {
        var name = await SelectPack();
        if (name == null)
            return;

        CloseButton();
        await OtherBinding.AddPack(name, PackType.ColorMC);
    }

    private async Task<string?> SelectPack()
    {
        OpenFileDialog openFile = new()
        {
            Title = "Ñ¡ÔñÑ¹Ëõ°ü",
            AllowMultiple = false,
            Filters = SystemInfo.Os == OsType.Windows ? new()
            {
                new FileDialogFilter()
                {
                    Extensions =new()
                    {
                        "zip"
                    }
                }
            } : new()
        };

        var file = await openFile.ShowAsync(Window);
        if (file?.Length > 0)
        {
            var item = file[0];
            return item;
        }

        return null;
    }

    private void CloseButton()
    {
        Button_Add1.IsEnabled = false;
        Button_Add2.IsEnabled = false;
        Button_Add3.IsEnabled = false;
        Button_Add4.IsEnabled = false;
    }

    private void Other_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private void Snapshot_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private void Release_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private void Load()
    {
        List.Clear();
        List.AddRange(OtherBinding.GetGames(Release.IsChecked, Snapshot.IsChecked, Other.IsChecked));
    }

    private void Button_Next_Click(object? sender, RoutedEventArgs e)
    {
        Window.Next();
    }

    public void SetWindow(HelloWindow window)
    {
        Window = window;
    }
}
