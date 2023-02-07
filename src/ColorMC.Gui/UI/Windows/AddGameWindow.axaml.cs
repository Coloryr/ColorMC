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
    private readonly ObservableCollection<string> List = new();
    private readonly ObservableCollection<string> List1 = new();
    private bool add;

    public AddGameWindow()
    {
        InitializeComponent();

        Head.SetWindow(this);
        this.BindFont();
        Icon = App.Icon;
        Rectangle1.MakeResizeDrag(this);

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
        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Button3.Click += Button3_Click;

        CheckBox_Forge.Click += Forge_Click;
        CheckBox_Fabric.Click += Fabric_Click;
        CheckBox_Quilt.Click += Quilt_Click;
        CheckBox_Release.Click += Release_Click;
        CheckBox_Snapshot.Click += Snapshot_Click;
        CheckBox_Other.Click += Other_Click;

        CoreMain.PackState = PackState;
        CoreMain.PackUpdate = PackUpdate;
        CoreMain.GameOverwirte = GameOverwirte;

        App.PicUpdate += Update;

        Closed += AddGameWindow_Closed;

        Load();
    }

    private async void Button1_Click(object? sender, RoutedEventArgs e)
    {
        Info1.Show(Localizer.Instance["GameEditWindow.Info1"]);
        var res = await GameBinding.ReloadVersion();
        Info1.Close();
        if (!res)
        {
            Info.Show(Localizer.Instance["GameEditWindow.Error1"]);
            return;
        }

        Update();
    }

    private async void Button2_Click(object? sender, RoutedEventArgs e)
    {
        CheckBox_Forge.IsEnabled = false;
        CheckBox_Fabric.IsEnabled = false;
        CheckBox_Quilt.IsEnabled = false;

        var item = ComboBox_GameVersion.SelectedItem as string;
        if (string.IsNullOrWhiteSpace(item))
        {
            return;
        }    

        Info1.Show(Localizer.Instance["AddGameWindow.Info3"]);
        var list = await GameBinding.GetForgeSupportVersion();
        if (list != null && list.Contains(item))
        {
            CheckBox_Forge.IsEnabled = true;
        }

        list = await GameBinding.GetFabricSupportVersion();
        if (list != null && list.Contains(item))
        {
            CheckBox_Fabric.IsEnabled = true;
        }

        list = await GameBinding.GetQuiltSupportVersion();
        if (list != null && list.Contains(item))
        {
            CheckBox_Quilt.IsEnabled = true;
        }
        Info1.Close();
    }
    private async void Button3_Click(object? sender, RoutedEventArgs e)
    {
        ComboBox_LoaderVersion.IsEnabled = false;

        var item = ComboBox_GameVersion.SelectedItem as string;
        if (string.IsNullOrWhiteSpace(item))
        {
            return;
        }

        if (CheckBox_Forge.IsChecked == true)
        {
            Info1.Show(Localizer.Instance["AddGameWindow.Info6"]);
            CheckBox_Fabric.IsEnabled = false;
            CheckBox_Quilt.IsEnabled = false;

            var list = await GameBinding.GetForgeVersion(item);
            Info1.Close();
            if (list == null)
            {
                return;
            }

            ComboBox_LoaderVersion.IsEnabled = true;
            List1.Clear();
            List1.AddRange(list);
        }
        else if (CheckBox_Fabric.IsChecked == true)
        {
            Info1.Show(Localizer.Instance["AddGameWindow.Info5"]);
            CheckBox_Forge.IsEnabled = false;
            CheckBox_Quilt.IsEnabled = false;

            var list = await GameBinding.GetFabricVersion(item);
            Info1.Close();
            if (list == null)
            {
                return;
            }

            ComboBox_LoaderVersion.IsEnabled = true;
            List1.Clear();
            List1.AddRange(list);
        }
        else if (CheckBox_Quilt.IsChecked == true)
        {
            Info1.Show(Localizer.Instance["AddGameWindow.Info4"]);
            CheckBox_Forge.IsEnabled = false;
            CheckBox_Fabric.IsEnabled = false;

            var list = await GameBinding.GetQuiltVersion(item);
            Info1.Close();
            if (list == null)
            {
                return;
            }

            ComboBox_LoaderVersion.IsEnabled = true;
            List1.Clear();
            List1.AddRange(list);
        }
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
        App.PicUpdate -= Update;

        CoreMain.PackState = null;
        CoreMain.PackUpdate = null;
        CoreMain.GameOverwirte = null;
        App.AddGameWindow = null;

        Head.SetWindow(null);
    }

    private void Button_Add5_Click(object? sender, RoutedEventArgs e)
    {
        App.ShowCurseForge();
    }

    private async void Button_Add_Click(object? sender, RoutedEventArgs e)
    {
        var name = TextBox_Input1.Text;
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
            App.MainWindow?.Info2.Show(Localizer.Instance["AddGameWindow.Info2"]);
            App.MainWindow?.Load();
            Close();
        }
    }

    private void GameVersion_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        Button2_Click(null, null);
    }

    private async void Quilt_Click(object? sender, RoutedEventArgs e)
    {
        if (CheckBox_Quilt.IsChecked == true)
        {
            var item = ComboBox_GameVersion.SelectedItem as string;
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
            App.MainWindow?.Info2.Show(Localizer.Instance["AddGameWindow.Info12"]);
            App.MainWindow?.Load();
            Close();
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
            App.MainWindow?.Info2.Show(Localizer.Instance["AddGameWindow.Info12"]);
            App.MainWindow?.Load();
            Close();
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
            App.MainWindow?.Info2.Show(Localizer.Instance["AddGameWindow.Info12"]);
            App.MainWindow?.Load();
            Close();
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
            App.MainWindow?.Info2.Show(Localizer.Instance["AddGameWindow.Info12"]);
            App.MainWindow?.Load();
            Close();
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
        else
        {
            App.MainWindow?.Info2.Show(Localizer.Instance["AddGameWindow.Info2"]);
            App.MainWindow?.Load();
            Close();
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
