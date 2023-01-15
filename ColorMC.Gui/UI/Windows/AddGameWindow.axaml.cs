using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Net;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Utils;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;
using DynamicData;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Windows;

public partial class AddGameWindow : Window
{
    private ObservableCollection<string> List = new();
    private ObservableCollection<string> List1 = new();
    private bool add;

    public AddGameWindow()
    {
        InitializeComponent();

        this.MakeItNoChrome();
        Rectangle1.MakeResizeDrag(this);
        FontFamily = Program.FontFamily;

        ComboBox_GameVersion.Items = List;
        ComboBox_GameVersion.SelectionChanged += GameVersion_SelectionChanged;

        ComboBox_Group.Items = List1;

        Button_Add.Click += Button_Add_Click;
        Button_Add1.Click += Button_Add1_Click;
        Button_Add2.Click += Button_Add2_Click;
        Button_Add3.Click += Button_Add3_Click;
        Button_Add4.Click += Button_Add4_Click;
        Button_Add5.Click += Button_Add5_Click;
        Button_AddGroup.Click += Button_AddGroup_Click;

        CheckBox_Forge.Click += Forge_Click;
        CheckBox_Fabric.Click += Fabric_Click;
        CheckBox_Quilt.Click += Quilt_Click;
        CheckBox_Release.Click += Release_Click;
        CheckBox_Snapshot.Click += Snapshot_Click;
        CheckBox_Other.Click += Other_Click;

        CoreMain.PackState = PackState;
        CoreMain.PackUpdate = PackUpdate;
        CoreMain.GameOverwirte = GameOverwirte;

        Closed += AddGameWindow_Closed;

        Load();
    }

    private async void Button_AddGroup_Click(object? sender, RoutedEventArgs e)
    {
        await Info3.ShowOne(Localizer.Instance["AddGameWindow.Info15"], false);
        Info3.Close();
        if (Info3.Cancel)
        {
            return;
        }

        var res = Info3.Read().Item1;
        if (string.IsNullOrWhiteSpace(res))
        {
            Info1.Show(Localizer.Instance["AddGameWindow.Error6"]);
            return;
        }

        if (!GameBinding.AddGameGroup(res))
        {
            Info1.Show(Localizer.Instance["AddGameWindow.Error7"]);
            return;
        }

        Info2.Show(Localizer.Instance["AddGameWindow.Info2"]);

        List1.Clear();
        List1.AddRange(GameBinding.GetGameGroups().Keys);
    }

    private void AddGameWindow_Closed(object? sender, EventArgs e)
    {
        CoreMain.PackState = null;
        CoreMain.PackUpdate = null;
        CoreMain.GameOverwirte = null;
        App.AddGameWindow = null;
    }

    private void Button_Add5_Click(object? sender, RoutedEventArgs e)
    {
        App.ShowCurseForge();
    }

    private async void Button_Add_Click(object? sender, RoutedEventArgs e)
    {
        string name = TextBox_Input1.Text;
        if (string.IsNullOrWhiteSpace(name))
        {
            Info.Show(Localizer.Instance["AddGameWindow.Error1"]);
            return;
        }

        string? version = ComboBox_GameVersion.SelectedItem as string;
        if (string.IsNullOrWhiteSpace(version))
        {
            Info.Show(Localizer.Instance["AddGameWindow.Error2"]);
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
        var res = await GameBinding.AddGame(name, version, loader, 
            loaderversion, ComboBox_Group.SelectedItem as string);
        if (!res)
        {
            Info.Show(Localizer.Instance["AddGameWindow.Info5"]);
        }
        else
        {
            TextBox_Input1.Text = "";
            ComboBox_GameVersion.SelectedItem = null;
            Info2.Show(Localizer.Instance["AddGameWindow.Info2"]);
            App.MainWindow?.Load();
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
            Info1.Show(Localizer.Instance["AddGameWindow.Info3"]);
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
            Info1.Close();
        }
    }

    private async void Quilt_Click(object? sender, RoutedEventArgs e)
    {
        if (CheckBox_Quilt.IsChecked == true)
        {
            string? item = ComboBox_GameVersion.SelectedItem as string;
            if (item == null)
                return;

            Info1.Show(Localizer.Instance["AddGameWindow.Info4"]);
            CheckBox_Forge.IsEnabled = false;
            CheckBox_Fabric.IsEnabled = false;

            var list = await QuiltHelper.GetLoaders(item, BaseClient.Source);
            Info1.Close();
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

            Info1.Show(Localizer.Instance["AddGameWindow.Info5"]);
            CheckBox_Forge.IsEnabled = false;
            CheckBox_Quilt.IsEnabled = false;

            var list = await FabricHelper.GetLoaders(item, BaseClient.Source);
            Info1.Close();
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

            Info1.Show(Localizer.Instance["AddGameWindow.Info6"]);
            CheckBox_Fabric.IsEnabled = false;
            CheckBox_Quilt.IsEnabled = false;

            var list = await ForgeHelper.GetVersionList(item, BaseClient.Source);
            Info1.Close();
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
        Info1.Close();
        var test = await Info.ShowWait(
            string.Format(Localizer.Instance["AddGameWindow.Info7"], obj.Name));
        if (!add)
        {
            Info1.Show();
        }
        return test;
    }

    private void PackUpdate(int size, int now)
    {
        Info1.Progress((double)now / size);
    }

    private void PackState(CoreRunState state)
    {
        if (state == CoreRunState.Read)
        {
            Info1.Show(Localizer.Instance["AddGameWindow.Info8"]);
        }
        else if (state == CoreRunState.Init)
        {
            Info1.NextText(Localizer.Instance["AddGameWindow.Info9"]);
        }
        else if (state == CoreRunState.GetInfo)
        {
            Info1.NextText(Localizer.Instance["AddGameWindow.Info10"]);
        }
        else if (state == CoreRunState.Download)
        {
            Info1.NextText(Localizer.Instance["AddGameWindow.Info11"]);
            Info1.Progress(-1);
        }
        else if (state == CoreRunState.End)
        {
            TextBox_Input1.Text = "";
            ComboBox_GameVersion.SelectedItem = null;
            Info2.Show(Localizer.Instance["AddGameWindow.Info2"]);
            App.MainWindow?.Load();
            Info1.Close();
        }
    }

    private async void Button_Add4_Click(object? sender, RoutedEventArgs e)
    {
        add = false;
        var name = await SelectPack();
        if (name == null)
            return;

        DisableButton();
        if (await GameBinding.AddPack(name, PackType.HMCL))
        {
            Info2.Show(Localizer.Instance["AddGameWindow.Info12"]);
        }
        else
        {
            Info.Show(Localizer.Instance["AddGameWindow.Error3"]);
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
        if (await GameBinding.AddPack(name, PackType.MMC))
        {
            Info2.Show(Localizer.Instance["AddGameWindow.Info12"]);
        }
        else
        {
            Info.Show(Localizer.Instance["AddGameWindow.Error3"]);
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
        if (await GameBinding.AddPack(name, PackType.CurseForge))
        {
            Info2.Show(Localizer.Instance["AddGameWindow.Info12"]);
        }
        else
        {
            Info.Show(Localizer.Instance["AddGameWindow.Error3"]);
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
        if (await GameBinding.AddPack(name, PackType.ColorMC))
        {
            Info2.Show(Localizer.Instance["AddGameWindow.Info12"]);
        }
        else
        {
            Info.Show(Localizer.Instance["AddGameWindow.Error3"]);
        }
        EnableButton();
    }

    private async Task<string?> SelectPack()
    {
        OpenFileDialog openFile = new()
        {
            Title = Localizer.Instance["AddGameWindow.Info13"],
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

        var file = await openFile.ShowAsync(this);
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

    public async void Install(CurseForgeObj.Data.LatestFiles data)
    {
        Info1.Show(Localizer.Instance["AddGameWindow.Info14"]);
        var res = await GameBinding.InstallCurseForge(data);
        Info1.Close();
        if (!res)
        {
            Info.Show(Localizer.Instance["AddGameWindow.Error4"]);
        }
    }

    public void Load()
    {
        List.Clear();
        List.AddRange(GameBinding.GetGameVersion(CheckBox_Release.IsChecked,
            CheckBox_Snapshot.IsChecked, CheckBox_Other.IsChecked));

        List1.Clear();
        List1.AddRange(GameBinding.GetGameGroups().Keys);
    }

    public void Update()
    {
        App.Update(this, Image_Back, Grid1);
    }
}
