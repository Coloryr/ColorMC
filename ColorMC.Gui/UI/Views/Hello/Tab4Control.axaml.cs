using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
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

        CoreMain.PackState = PackState;
        CoreMain.PackUpdate = PackUpdate;

        CoreMain.GameOverwirte = GameOverwirte;

        Load();
    }

    private async Task<bool> GameOverwirte(GameSettingObj obj)
    {
        Window.Info1.Close();
        var test = await Window.Info.ShowWait($"游戏实例:{obj.Name}冲突，是否覆盖");
        Window.Info1.Show();
        return test;
    }

    private void PackUpdate(int size, int now)
    {
        Window.Info1.Progress((double)now / size);
    }

    private void PackState(CoreRunState state)
    {
        if (state == CoreRunState.Read)
        {
            Window.Info1.Show("正在导入压缩包");
        }
        else if (state == CoreRunState.Init)
        {
            Window.Info1.NextText("正在读取压缩包");
        }
        else if (state == CoreRunState.GetInfo)
        {
            Window.Info1.NextText("正在解析压缩包");
        }
        else if (state == CoreRunState.Download)
        {
            Window.Info1.NextText("正在下载文件");
            Window.Info1.Progress(-1);
        }
        else if (state == CoreRunState.End)
        {
            Window.Info1.Close();
        }
    }

    private async void Button_Add4_Click(object? sender, RoutedEventArgs e)
    {
        var name = await SelectPack();
        if (name == null)
            return;

        DisableButton();
        if (await OtherBinding.AddPack(name, PackType.HMCL))
        {
            Window.Info2.Show("导入完成");
        }
        else
        {
            Window.Info.Show("导入错误");
        }
        EnableButton();
    }

    private async void Button_Add3_Click(object? sender, RoutedEventArgs e)
    {
        var name = await SelectPack();
        if (name == null)
            return;

        DisableButton();
        if (await OtherBinding.AddPack(name, PackType.MMC))
        {
            Window.Info2.Show("导入完成");
        }
        else
        {
            Window.Info.Show("导入错误");
        }
        EnableButton();
    }

    private async void Button_Add2_Click(object? sender, RoutedEventArgs e)
    {
        var name = await SelectPack();
        if (name == null)
            return;

        DisableButton();
        if (await OtherBinding.AddPack(name, PackType.CurseForge))
        {
            Window.Info2.Show("导入完成");
        }
        else
        {
            Window.Info.Show("导入错误");
        }
        EnableButton();
    }

    private async void Button_Add1_Click(object? sender, RoutedEventArgs e)
    {
        var name = await SelectPack();
        if (name == null)
            return;

        DisableButton();
        if (await OtherBinding.AddPack(name, PackType.ColorMC))
        {
            Window.Info2.Show("导入完成");
        }
        else
        {
            Window.Info.Show("导入错误");
        }
        EnableButton();
    }

    private async Task<string?> SelectPack()
    {
        OpenFileDialog openFile = new()
        {
            Title = "选择压缩包",
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

    private void EnableButton()
    {
        Button_Add1.IsEnabled = true;
        Button_Add2.IsEnabled = true;
        Button_Add3.IsEnabled = true;
        Button_Add4.IsEnabled = true;
    }

    private void DisableButton()
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
