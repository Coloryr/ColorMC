using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.UIBinding;
using DynamicData;
using System.Collections.ObjectModel;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab1Control : UserControl
{
    private readonly ObservableCollection<string> List = new();
    private readonly ObservableCollection<string> List1 = new();
    private readonly ObservableCollection<string> List2 = new();
    private GameSettingObj Obj;
    private bool load = false;
    public Tab1Control()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Button3.Click += Button3_Click;
        Button4.Click += Button4_Click;
        Button5.Click += Button5_Click;
        Button6.Click += Button6_Click;
        Button7.Click += Button7_Click;
        Button8.Click += Button8_Click;
        Button9.Click += Button9_Click;

        CheckBox5.Click += Forge_Click;
        CheckBox6.Click += Fabric_Click;
        CheckBox7.Click += Quilt_Click;
        CheckBox2.Click += Other_Click;
        CheckBox3.Click += Other_Click;
        CheckBox4.Click += Other_Click;

        CheckBox1.Click += CheckBox1_Click;

        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;
        ComboBox2.SelectionChanged += ComboBox2_SelectionChanged;
        ComboBox3.SelectionChanged += ComboBox3_SelectionChanged;

        ComboBox1.ItemsSource = List;
        ComboBox2.ItemsSource = List1;
        ComboBox3.ItemsSource = List2;
    }

    private void Button9_Click(object? sender, RoutedEventArgs e)
    {
        App.ShowConfigEdit(Obj);
    }

    private void Button8_Click(object? sender, RoutedEventArgs e)
    {
        App.ShowServerPack(Obj);
    }

    private void Button7_Click(object? sender, RoutedEventArgs e)
    {
        App.ShowAdd(Obj, FileType.Optifne);
    }

    private void Button6_Click(object? sender, RoutedEventArgs e)
    {
        BaseBinding.OpPath(Obj.GetBasePath());
    }

    private void CheckBox1_Click(object? sender, RoutedEventArgs e)
    {
        Obj.ModPack = CheckBox1.IsChecked == true;

        Save();
    }

    private void ComboBox3_SelectionChanged(object? sender,
        SelectionChangedEventArgs e)
    {
        if (load)
            return;

        GameBinding.MoveGameGroup(Obj, ComboBox3.SelectedItem as string);
    }

    private void ComboBox2_SelectionChanged(object? sender,
        SelectionChangedEventArgs e)
    {
        if (load)
            return;

        Save();
    }

    private void ComboBox1_SelectionChanged(object? sender,
        SelectionChangedEventArgs e)
    {
        if (load)
            return;

        ComboBox2.SelectedItem = null;
        List1.Clear();
        CheckBox5.IsChecked = false;
        CheckBox6.IsChecked = false;
        CheckBox7.IsChecked = false;

        Save();
    }

    private async void Button5_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        if (BaseBinding.IsGameRun(Obj))
        {
            window.Info1.Show(App.GetLanguage("GameEditWindow.Tab1.Error1"));
            return;
        }

        var res = await window.Info.ShowWait(string.Format(
            App.GetLanguage("GameEditWindow.Tab1.Info1"), Obj.Name));
        if (!res)
            return;

        App.MainWindow?.DeleteGame(Obj, true);
    }

    private void Save()
    {
        Loaders loaders = Loaders.Normal;
        if (CheckBox5.IsChecked == true)
        {
            loaders = Loaders.Forge;
        }
        else if (CheckBox6.IsChecked == true)
        {
            loaders = Loaders.Fabric;
        }
        else if (CheckBox7.IsChecked == true)
        {
            loaders = Loaders.Quilt;
        }

        GameBinding.SaveGame(Obj, ComboBox1.SelectedItem as string,
            loaders, ComboBox2.SelectedItem as string);
    }

    private async void Button4_Click(object? sender, RoutedEventArgs e)
    {
        var Window = App.FindRoot(VisualRoot);
        await Window.Info3.ShowOne(App.GetLanguage("AddGameWindow.Info1"), false);
        if (Window.Info3.Cancel)
        {
            return;
        }

        var res = Window.Info3.Read().Item1;
        if (string.IsNullOrWhiteSpace(res))
        {
            Window.Info1.Show(App.GetLanguage("AddGameWindow.Error6"));
            return;
        }

        if (!GameBinding.AddGameGroup(res))
        {
            Window.Info1.Show(App.GetLanguage("AddGameWindow.Error7"));
            return;
        }

        Window.Info2.Show(App.GetLanguage("AddGameWindow.Info2"));

        Load2();
    }

    private void Other_Click(object? sender, RoutedEventArgs e)
    {
        Load1();
    }

    private async void Button3_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);

        ComboBox2.IsEnabled = false;

        if (CheckBox5.IsChecked == true)
        {
            window.Info1.Show(App.GetLanguage("AddGameWindow.Info6"));
            CheckBox6.IsEnabled = false;
            CheckBox7.IsEnabled = false;

            var list = await GameBinding.GetForgeVersion(Obj.Version);
            window.Info1.Close();
            if (list == null)
            {
                return;
            }

            ComboBox2.IsEnabled = true;
            List1.Clear();
            List1.AddRange(list);
        }
        else if (CheckBox6.IsChecked == true)
        {
            window.Info1.Show(App.GetLanguage("AddGameWindow.Info5"));
            CheckBox5.IsEnabled = false;
            CheckBox7.IsEnabled = false;

            var list = await GameBinding.GetFabricVersion(Obj.Version);
            window.Info1.Close();
            if (list == null)
            {
                return;
            }

            ComboBox2.IsEnabled = true;
            List1.Clear();
            List1.AddRange(list);
        }
        else if (CheckBox7.IsChecked == true)
        {
            window.Info1.Show(App.GetLanguage("AddGameWindow.Info4"));
            CheckBox5.IsEnabled = false;
            CheckBox6.IsEnabled = false;

            var list = await GameBinding.GetQuiltVersion(Obj.Version);
            window.Info1.Close();
            if (list == null)
            {
                return;
            }

            ComboBox2.IsEnabled = true;
            List1.Clear();
            List1.AddRange(list);
        }
    }

    private void Quilt_Click(object? sender, RoutedEventArgs e)
    {
        if (CheckBox7.IsChecked == true)
        {
            CheckBox5.IsEnabled = false;
            CheckBox5.IsChecked = false;
            CheckBox6.IsEnabled = false;
            CheckBox6.IsChecked = false;
            Button3_Click(sender, e);
        }
        else
        {
            ComboBox2.IsEnabled = false;
            CheckBox5.IsEnabled = true;
            CheckBox6.IsEnabled = true;
            List1.Clear();
        }
    }
    private void Fabric_Click(object? sender, RoutedEventArgs e)
    {
        if (CheckBox6.IsChecked == true)
        {
            CheckBox5.IsEnabled = false;
            CheckBox5.IsChecked = false;
            CheckBox7.IsEnabled = false;
            CheckBox7.IsChecked = false;
            Button3_Click(sender, e);
        }
        else
        {
            ComboBox2.IsEnabled = false;
            CheckBox5.IsEnabled = true;
            CheckBox7.IsEnabled = true;
            List1.Clear();
        }
    }

    private void Forge_Click(object? sender, RoutedEventArgs e)
    {
        if (CheckBox5.IsChecked == true)
        {
            CheckBox6.IsEnabled = false;
            CheckBox6.IsChecked = false;
            CheckBox7.IsEnabled = false;
            CheckBox7.IsChecked = false;
            Button3_Click(sender, e);
        }
        else
        {
            ComboBox2.IsEnabled = false;
            CheckBox6.IsEnabled = true;
            CheckBox7.IsEnabled = true;
            List1.Clear();
        }
    }

    private async void Button2_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        window.Info1.Show(App.GetLanguage("AddGameWindow.Info3"));
        var list = await GameBinding.GetForgeSupportVersion();
        if (list != null && list.Contains(Obj.Version))
        {
            CheckBox5.IsEnabled = true;
        }

        list = await GameBinding.GetFabricSupportVersion();
        if (list != null && list.Contains(Obj.Version))
        {
            CheckBox6.IsEnabled = true;
        }

        list = await GameBinding.GetQuiltSupportVersion();
        if (list != null && list.Contains(Obj.Version))
        {
            CheckBox7.IsEnabled = true;
        }
        window.Info1.Close();
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

        Update();
    }

    private void Load1()
    {
        List.Clear();
        List.AddRange(GameBinding.GetGameVersion(CheckBox2.IsChecked,
            CheckBox3.IsChecked, CheckBox4.IsChecked));

        ComboBox1.SelectedItem = Obj.Version;

        CheckBox1.IsChecked = Obj.ModPack;
    }

    private void Load2()
    {
        List2.Clear();
        List2.AddRange(GameBinding.GetGameGroups().Keys);

        if (!string.IsNullOrWhiteSpace(Obj.GroupName))
        {
            ComboBox2.SelectedItem = Obj.GroupName;
        }
    }

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;
    }

    public void Update()
    {
        if (Obj == null)
            return;

        load = true;

        if (Obj.GameType == GameType.Snapshot)
        {
            CheckBox3.IsChecked = true;
        }
        else if (Obj.GameType == GameType.Other)
        {
            CheckBox4.IsChecked = true;
        }

        Load1();
        Load2();
        if (Obj.Loader != Loaders.Normal)
        {
            switch (Obj.Loader)
            {
                case Loaders.Forge:
                    CheckBox5.IsEnabled = true;
                    CheckBox5.IsChecked = true;
                    break;
                case Loaders.Fabric:
                    CheckBox6.IsEnabled = true;
                    CheckBox6.IsChecked = true;
                    break;
                case Loaders.Quilt:
                    CheckBox7.IsEnabled = true;
                    CheckBox7.IsChecked = true;
                    break;
            }

            List1.Clear();
            List1.Add(Obj.LoaderVersion);
            ComboBox2.SelectedItem = Obj.LoaderVersion;
        }
        if (BaseBinding.IsGameRun(Obj))
        {
            Button5.IsEnabled = false;
        }
        else
        {
            Button5.IsEnabled = true;
        }

        load = false;
    }
}
