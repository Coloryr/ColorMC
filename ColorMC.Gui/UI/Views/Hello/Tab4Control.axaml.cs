using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core;
using ColorMC.Core.Http;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UIBinding;
using DynamicData;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Input;

namespace ColorMC.Gui.UI.Views.Hello;

public partial class Tab4Control : UserControl
{
    private HelloWindow Window;

    private ObservableCollection<string> List = new();

    private bool add;
    public Tab4Control()
    {
        InitializeComponent();

        ComboBox_GameVersion.Items = List;
        ComboBox_GameVersion.SelectionChanged += GameVersion_SelectionChanged;

        Button_Next.Click += Button_Next_Click;
        Button_Add.Click += Button_Add_Click;
        Button_Add1.Click += Button_Add1_Click;
        Button_Add2.Click += Button_Add2_Click;
        Button_Add3.Click += Button_Add3_Click;
        Button_Add4.Click += Button_Add4_Click;

        CheckBox_Forge.Click += Forge_Click;
        CheckBox_Fabric.Click += Fabric_Click;
        CheckBox_Quilt.Click += Quilt_Click;
        CheckBox_Release.Click += Release_Click;
        CheckBox_Snapshot.Click += Snapshot_Click;
        CheckBox_Other.Click += Other_Click;

        CoreMain.PackState = PackState;
        CoreMain.PackUpdate = PackUpdate;

        CoreMain.GameOverwirte = GameOverwirte;

        Load();
    }

    private async void Button_Add_Click(object? sender, RoutedEventArgs e)
    {
        string name = TextBox_Input1.Text;
        if (string.IsNullOrWhiteSpace(name))
        {
            Window.Info.Show("没有实例名字");
            return;
        }

        string? version = ComboBox_GameVersion.SelectedItem as string;
        if (string.IsNullOrWhiteSpace(version))
        {
            Window.Info.Show("没有选择版本");
            return;
        }

        var loaderversion = ComboBox_LoaderVersion.SelectedItem as string;
        Loaders loader = Loaders.Normal;
        if (!string.IsNullOrWhiteSpace(loaderversion))
        {
            if (CheckBox_Forge.IsChecked == true)
            {
                loader = Loaders.Forge;
            }
            else if (CheckBox_Fabric.IsChecked == true)
            {
                loader = Loaders.Fabric;
            }
            else if (CheckBox_Quilt.IsChecked == true)
            {
                loader = Loaders.Quilt;
            }
        }

        add = true;
        var res = await OtherBinding.AddGame(name, version, loader, loaderversion);
        if (!res)
        {
            Window.Info.Show("添加实例失败");
        }
        else
        {
            Window.Info2.Show("添加成功");
        }
    }

    private async void GameVersion_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        CheckBox_Forge.IsEnabled = false;
        CheckBox_Fabric.IsEnabled = false;
        CheckBox_Quilt.IsEnabled = false;

        string? item = ComboBox_GameVersion.SelectedItem as string;
        if (!string.IsNullOrWhiteSpace(item))
        {
            Window.Info1.Show("正在获取Mod加载器信息");
            var list = await ForgeHelper.GetSupportVersion();
            if (list != null && list.Contains(item))
            {
                CheckBox_Forge.IsEnabled = true;
            }

            list = await FabricHelper.GetSupportVersion();
            if (list != null && list.Contains(item))
            {
                CheckBox_Fabric.IsEnabled = true;
            }

            list = await QuiltHelper.GetSupportVersion();
            if (list != null && list.Contains(item))
            {
                CheckBox_Quilt.IsEnabled = true;
            }
            Window.Info1.Close();
        }
    }

    private async void Quilt_Click(object? sender, RoutedEventArgs e)
    {
        if (CheckBox_Quilt.IsChecked == true)
        {
            string? item = ComboBox_GameVersion.SelectedItem as string;
            if (item == null)
                return;

            Window.Info1.Show("正在获取Quilt版本信息");
            CheckBox_Forge.IsEnabled = false;
            CheckBox_Fabric.IsEnabled = false;

            var list = await QuiltHelper.GetLoaders(item, BaseClient.Source);
            Window.Info1.Close();
            if (list == null)
            {
                return;
            }

            ComboBox_LoaderVersion.IsEnabled = true;
            ComboBox_LoaderVersion.Items = list;
        }
        else
        {
            ComboBox_LoaderVersion.IsEnabled = false;
            CheckBox_Forge.IsEnabled = true;
            CheckBox_Fabric.IsEnabled = true;
            ComboBox_LoaderVersion.Items = null;
        }
    }

    private async void Fabric_Click(object? sender, RoutedEventArgs e)
    {
        if (CheckBox_Fabric.IsChecked == true)
        {
            string? item = ComboBox_GameVersion.SelectedItem as string;
            if (item == null)
                return;

            Window.Info1.Show("正在获取Fabric版本信息");
            CheckBox_Forge.IsEnabled = false;
            CheckBox_Quilt.IsEnabled = false;
            
            var list = await FabricHelper.GetLoaders(item, BaseClient.Source);
            Window.Info1.Close();
            if (list == null)
            {
                return;
            }

            ComboBox_LoaderVersion.IsEnabled = true;
            ComboBox_LoaderVersion.Items = list;
        }
        else
        {
            ComboBox_LoaderVersion.IsEnabled = false;
            CheckBox_Forge.IsEnabled = true;
            CheckBox_Quilt.IsEnabled = true;
            ComboBox_LoaderVersion.Items = null;
        }
    }

    private async void Forge_Click(object? sender, RoutedEventArgs e)
    {
        if (CheckBox_Forge.IsChecked == true)
        {
            string? item = ComboBox_GameVersion.SelectedItem as string;
            if (item == null)
                return;

            Window.Info1.Show("正在获取Forge版本信息");
            CheckBox_Fabric.IsEnabled = false;
            CheckBox_Quilt.IsEnabled = false;

            var list = await ForgeHelper.GetVersionList(item, BaseClient.Source);
            Window.Info1.Close();
            if (list == null)
            {
                return;
            }

            ComboBox_LoaderVersion.IsEnabled = true;
            ComboBox_LoaderVersion.Items = list;
        }
        else
        {
            ComboBox_LoaderVersion.IsEnabled = false;
            CheckBox_Fabric.IsEnabled = true;
            CheckBox_Quilt.IsEnabled = true;
            ComboBox_LoaderVersion.Items = null;
        }
    }

    private async Task<bool> GameOverwirte(GameSettingObj obj)
    {
        Window.Info1.Close();
        var test = await Window.Info.ShowWait($"游戏实例:{obj.Name}冲突，是否覆盖");
        if (!add)
        {
            Window.Info1.Show();
        }
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
        add = false;
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
        add = false;
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
        add = false;
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
        add = false;
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

    public void Load()
    {
        List.Clear();
        List.AddRange(OtherBinding.GetGameVersion(CheckBox_Release.IsChecked, 
            CheckBox_Snapshot.IsChecked, CheckBox_Other.IsChecked));
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
