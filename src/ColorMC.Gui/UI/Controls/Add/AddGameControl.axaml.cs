using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Core;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.FTB;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using DynamicData;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddGameControl : UserControl, IUserControl
{
    private readonly ObservableCollection<string> List = new();
    private readonly ObservableCollection<string> List1 = new();
    private bool add;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public AddGameControl()
    {
        InitializeComponent();

        ComboBox_GameVersion.ItemsSource = List;
        ComboBox_GameVersion.SelectionChanged += GameVersion_SelectionChanged;

        ComboBox_Group.ItemsSource = List1;

        ComboBox4.ItemsSource = GameBinding.GetPackType();

        Button_Add.Click += Button_Add_Click;

        Button4.Click += Button4_Click;

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

        ColorMCCore.PackState = PackState;
        ColorMCCore.PackUpdate = PackUpdate;
        ColorMCCore.GameOverwirte = GameOverwirte;

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);
    }

    private void DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data.Contains(DataFormats.Files))
        {
            Grid2.IsVisible = true;
        }
    }

    private void DragLeave(object? sender, DragEventArgs e)
    {
        Grid2.IsVisible = false;
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        Grid2.IsVisible = false;
        if (e.Data.Contains(DataFormats.Files))
        {
            var files = e.Data.GetFiles();
            if (files == null || files.Count() > 1)
                return;

            var item = BaseBinding.GetPath(files.First());
            if (item?.EndsWith(".zip") == true || item?.EndsWith(".mrpack") == true)
            {
                AddFile(item);
            }
        }
    }

    private void Button4_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        if (BaseBinding.IsDownload)
        {
            window.Info.Show(App.GetLanguage("AddGameWindow.Info19"));
            return;
        }

        switch (ComboBox4.SelectedIndex)
        {
            case 0:
                AddPack(PackType.ColorMC);
                break;
            case 1:
                AddPack(PackType.CurseForge);
                break;
            case 2:
                AddPack(PackType.Modrinth);
                break;
            case 3:
                AddPack(PackType.MMC);
                break;
            case 4:
                AddPack(PackType.HMCL);
                break;
        }
    }

    private async void Button1_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        window.Info1.Show(App.GetLanguage("GameEditWindow.Info1"));
        var res = await GameBinding.ReloadVersion();
        window.Info1.Close();
        if (!res)
        {
            window.Info.Show(App.GetLanguage("GameEditWindow.Error1"));
            return;
        }

        Load();
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        VersionSelect();
    }

    private async void VersionSelect()
    {
        CheckBox_Forge.IsEnabled = false;
        CheckBox_Fabric.IsEnabled = false;
        CheckBox_Quilt.IsEnabled = false;

        CheckBox_Forge.IsChecked = false;
        CheckBox_Fabric.IsChecked = false;
        CheckBox_Quilt.IsChecked = false;

        ComboBox_LoaderVersion.ItemsSource = null;
        ComboBox_LoaderVersion.SelectedItem = null;

        var item = ComboBox_GameVersion.SelectedItem as string;
        if (string.IsNullOrWhiteSpace(item))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(TextBox_Input1.Text))
        {
            TextBox_Input1.Text = item;
        }

        var window = App.FindRoot(VisualRoot);
        window.Info1.Show(App.GetLanguage("AddGameWindow.Info3"));
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
        window.Info1.Close();
    }

    private async void Button3_Click(object? sender, RoutedEventArgs e)
    {
        ComboBox_LoaderVersion.IsEnabled = false;

        var item = ComboBox_GameVersion.SelectedItem as string;
        if (string.IsNullOrWhiteSpace(item))
        {
            return;
        }

        var window = App.FindRoot(VisualRoot);
        if (CheckBox_Forge.IsChecked == true)
        {
            window.Info1.Show(App.GetLanguage("AddGameWindow.Info6"));
            CheckBox_Fabric.IsEnabled = false;
            CheckBox_Quilt.IsEnabled = false;

            var list = await GameBinding.GetForgeVersion(item);
            window.Info1.Close();
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
            window.Info1.Show(App.GetLanguage("AddGameWindow.Info5"));
            CheckBox_Forge.IsEnabled = false;
            CheckBox_Quilt.IsEnabled = false;

            var list = await GameBinding.GetFabricVersion(item);
            window.Info1.Close();
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
            window.Info1.Show(App.GetLanguage("AddGameWindow.Info4"));
            CheckBox_Forge.IsEnabled = false;
            CheckBox_Fabric.IsEnabled = false;

            var list = await GameBinding.GetQuiltVersion(item);
            window.Info1.Close();
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
        var window = App.FindRoot(VisualRoot);
        await window.Info3.ShowOne(App.GetLanguage("AddGameWindow.Info1"), false);
        if (window.Info3.Cancel)
        {
            return;
        }

        var res = window.Info3.Read().Item1;
        if (string.IsNullOrWhiteSpace(res))
        {
            window.Info1.Show(App.GetLanguage("AddGameWindow.Error6"));
            return;
        }

        if (!GameBinding.AddGameGroup(res))
        {
            window.Info1.Show(App.GetLanguage("AddGameWindow.Error7"));
            return;
        }

        window.Info2.Show(App.GetLanguage("AddGameWindow.Info2"));

        List1.Clear();
        List1.AddRange(GameBinding.GetGameGroups().Keys);
    }

    public void Closed()
    {
        ColorMCCore.PackState = null;
        ColorMCCore.PackUpdate = null;
        ColorMCCore.GameOverwirte = null;

        App.AddGameWindow = null;
    }

    private void Button_Add5_Click(object? sender, RoutedEventArgs e)
    {
        App.ShowCurseForge();
    }

    private async void Button_Add_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        if (BaseBinding.IsDownload)
        {
            window.Info.Show(App.GetLanguage("AddGameWindow.Error8"));
            return;
        }

        var name = TextBox_Input1.Text;
        if (string.IsNullOrWhiteSpace(name))
        {
            window.Info.Show(App.GetLanguage("AddGameWindow.Error1"));
            return;
        }

        string? version = ComboBox_GameVersion.SelectedItem as string;
        if (string.IsNullOrWhiteSpace(version))
        {
            window.Info.Show(App.GetLanguage("AddGameWindow.Error2"));
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
            window.Info.Show(App.GetLanguage("AddGameWindow.Info5"));
        }
        else
        {
            App.MainWindow?.Window.Info2.Show(App.GetLanguage("AddGameWindow.Info2"));
            App.MainWindow?.Load();

            window.Close();
        }
    }

    private void GameVersion_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        VersionSelect();
    }

    private async void Quilt_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        if (CheckBox_Quilt.IsChecked == true)
        {
            var item = ComboBox_GameVersion.SelectedItem as string;
            if (item == null)
                return;

            window.Info1.Show(App.GetLanguage("AddGameWindow.Info4"));
            CheckBox_Forge.IsEnabled = false;
            CheckBox_Fabric.IsEnabled = false;

            var list = await GameBinding.GetQuiltVersion(item);
            window.Info1.Close();
            if (list == null)
            {
                return;
            }

            ComboBox_LoaderVersion.IsEnabled = true;
            ComboBox_LoaderVersion.ItemsSource = list;
        }
        else
        {
            ComboBox_LoaderVersion.IsEnabled = false;
            CheckBox_Forge.IsEnabled = true;
            CheckBox_Fabric.IsEnabled = true;
            ComboBox_LoaderVersion.ItemsSource = null;
        }
    }

    private async void Fabric_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        if (CheckBox_Fabric.IsChecked == true)
        {
            if (ComboBox_GameVersion.SelectedItem is not string item)
                return;

            window.Info1.Show(App.GetLanguage("AddGameWindow.Info5"));
            CheckBox_Forge.IsEnabled = false;
            CheckBox_Quilt.IsEnabled = false;

            var list = await GameBinding.GetFabricVersion(item);
            window.Info1.Close();
            if (list == null)
            {
                return;
            }

            ComboBox_LoaderVersion.IsEnabled = true;
            ComboBox_LoaderVersion.ItemsSource = list;
        }
        else
        {
            ComboBox_LoaderVersion.IsEnabled = false;
            CheckBox_Forge.IsEnabled = true;
            CheckBox_Quilt.IsEnabled = true;
            ComboBox_LoaderVersion.ItemsSource = null;
        }
    }

    private async void Forge_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        if (CheckBox_Forge.IsChecked == true)
        {
            string? item = ComboBox_GameVersion.SelectedItem as string;
            if (item == null)
                return;

            window.Info1.Show(App.GetLanguage("AddGameWindow.Info6"));
            CheckBox_Fabric.IsEnabled = false;
            CheckBox_Quilt.IsEnabled = false;

            var list = await GameBinding.GetForgeVersion(item);
            window.Info1.Close();
            if (list == null)
            {
                return;
            }

            ComboBox_LoaderVersion.IsEnabled = true;
            ComboBox_LoaderVersion.ItemsSource = list;
        }
        else
        {
            ComboBox_LoaderVersion.IsEnabled = false;
            CheckBox_Fabric.IsEnabled = true;
            CheckBox_Quilt.IsEnabled = true;
            ComboBox_LoaderVersion.ItemsSource = null;
        }
    }

    private async Task<bool> GameOverwirte(GameSettingObj obj)
    {
        var window = App.FindRoot(VisualRoot);
        window.Info1.Close();
        var test = await window.Info.ShowWait(
            string.Format(App.GetLanguage("AddGameWindow.Info7"), obj.Name));
        if (!add)
        {
            window.Info1.Show();
        }
        return test;
    }

    private void PackUpdate(int size, int now)
    {
        var window = App.FindRoot(VisualRoot);
        window.Info1.Progress((double)now / size);
    }

    private void PackState(CoreRunState state)
    {
        var window = App.FindRoot(VisualRoot);
        if (state == CoreRunState.Read)
        {
            window.Info1.Show(App.GetLanguage("AddGameWindow.Info8"));
        }
        else if (state == CoreRunState.Init)
        {
            window.Info1.NextText(App.GetLanguage("AddGameWindow.Info9"));
        }
        else if (state == CoreRunState.GetInfo)
        {
            window.Info1.NextText(App.GetLanguage("AddGameWindow.Info10"));
        }
        else if (state == CoreRunState.Download)
        {
            window.Info1.NextText(App.GetLanguage("AddGameWindow.Info11"));
            window.Info1.Progress(-1);
        }
        else if (state == CoreRunState.End)
        {
            TextBox_Input1.Text = "";
            ComboBox_GameVersion.SelectedItem = null;
        }
    }

    private async void AddPack(PackType type)
    {
        add = false;
        var file = await SelectPack();
        if (file == null)
            return;

        var window = App.FindRoot(VisualRoot);
        window.Info1.Show(App.GetLanguage("AddGameWindow.Info17"));
        var res = await GameBinding.AddPack(file, type, TextBox_Input1.Text, ComboBox_Group.SelectedItem as string);
        window.Info1.Close();
        if (res.Item1)
        {
            App.MainWindow?.Window.Info2.Show(App.GetLanguage("AddGameWindow.Info12"));
            App.MainWindow?.Load();
            window.Close();
        }
        else
        {
            window.Info.Show(App.GetLanguage("AddGameWindow.Error3"));
        }
    }

    private async Task<string?> SelectPack()
    {
        var window = App.FindRoot(VisualRoot);
        return await BaseBinding.OpFile(window, FileType.ModPack);
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

    public async void Install(CurseForgeObjList.Data.LatestFiles data, CurseForgeObjList.Data data1)
    {
        var window = App.FindRoot(VisualRoot);
        if (BaseBinding.IsDownload)
        {
            window.Info.Show(App.GetLanguage("AddGameWindow.Info19"));
            return;
        }

        window.Info1.Show(App.GetLanguage("AddGameWindow.Info14"));
        var res = await GameBinding.InstallCurseForge(data, data1, TextBox_Input1.Text, ComboBox_Group.SelectedItem as string);
        window.Info1.Close();
        if (!res)
        {
            window.Info.Show(App.GetLanguage("AddGameWindow.Error4"));
        }
        else
        {
            App.MainWindow?.Window.Info2.Show(App.GetLanguage("AddGameWindow.Info2"));
            App.MainWindow?.Load();
            window.Close();
        }
    }

    public async void Install(ModrinthVersionObj data, ModrinthSearchObj.Hit data1)
    {
        var window = App.FindRoot(VisualRoot);
        if (BaseBinding.IsDownload)
        {
            window.Info.Show(App.GetLanguage("AddGameWindow.Info19"));
            return;
        }

        window.Info1.Show(App.GetLanguage("AddGameWindow.Info14"));
        var res = await GameBinding.InstallModrinth(data, data1, TextBox_Input1.Text, ComboBox_Group.SelectedItem as string);
        window.Info1.Close();
        if (!res)
        {
            window.Info.Show(App.GetLanguage("AddGameWindow.Error4"));
        }
        else
        {
            App.MainWindow?.Window.Info2.Show(App.GetLanguage("AddGameWindow.Info2"));
            App.MainWindow?.Load();
            window.Close();
        }
    }

    public async void Install(FTBModpackObj.Versions data, FTBModpackObj data1)
    {
        var window = App.FindRoot(VisualRoot);
        if (BaseBinding.IsDownload)
        {
            window.Info.Show(App.GetLanguage("AddGameWindow.Info19"));
            return;
        }

        window.Info1.Show(App.GetLanguage("AddGameWindow.Info14"));
        var res = await GameBinding.InstallFTB(data, data1, TextBox_Input1.Text, ComboBox_Group.SelectedItem as string);
        window.Info1.Close();
        if (!res)
        {
            window.Info.Show(App.GetLanguage("AddGameWindow.Error4"));
        }
        else
        {
            FTBAPI.PostIntall(data1.id, data.id);
            App.MainWindow?.Window.Info2.Show(App.GetLanguage("AddGameWindow.Info2"));
            App.MainWindow?.Load();
            window.Close();
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

    public async void AddFile(string file)
    {
        add = false;
        var list = GameBinding.GetPackType();
        var window = App.FindRoot(VisualRoot);
        await window.Info5.Show(App.GetLanguage("AddGameWindow.Info18"), list);
        if (window.Info5.Cancel)
        {
            window.Close();
            return;
        }
        var type = window.Info5.Read().Item1 switch
        {
            1 => PackType.CurseForge,
            2 => PackType.Modrinth,
            3 => PackType.MMC,
            4 => PackType.HMCL,
            _ => PackType.ColorMC,
        };
        window.Info1.Show(App.GetLanguage("AddGameWindow.Info17"));
        var res = await GameBinding.AddPack(file, type, TextBox_Input1.Text, ComboBox_Group.SelectedItem as string);
        window.Info1.Close();
        if (res.Item1)
        {
            App.MainWindow?.Window.Info2.Show(App.GetLanguage("AddGameWindow.Info12"));
            App.MainWindow?.Load();
            window.Close();
        }
        else
        {
            window.Info.Show(App.GetLanguage("AddGameWindow.Error3"));
        }
    }

    public void Opened()
    {
        Window.SetTitle(App.GetLanguage("AddGameWindow.Title"));

        Load();
    }
}
