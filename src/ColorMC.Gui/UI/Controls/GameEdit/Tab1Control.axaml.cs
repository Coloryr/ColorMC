using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
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
    public Tab1Control()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Button3.Click += Button3_Click;
        Button4.Click += Button4_Click;
        Button5.Click += Button5_Click;

        CheckBox_Forge.Click += Forge_Click;
        CheckBox_Fabric.Click += Fabric_Click;
        CheckBox_Quilt.Click += Quilt_Click;
        CheckBox_Release.Click += Other_Click;
        CheckBox_Snapshot.Click += Other_Click;
        CheckBox_Other.Click += Other_Click;

        ComboBox1.SelectionChanged += ComboBox1_SelectionChanged;
        ComboBox2.SelectionChanged += ComboBox2_SelectionChanged;
        ComboBox3.SelectionChanged += ComboBox3_SelectionChanged;

        ComboBox1.Items = List;
        ComboBox2.Items = List1;
        ComboBox3.Items = List2;
    }

    private void ComboBox3_SelectionChanged(object? sender,
        SelectionChangedEventArgs e)
    {
        GameBinding.MoveGameGroup(Obj, ComboBox3.SelectedItem as string);
    }

    private void ComboBox2_SelectionChanged(object? sender,
        SelectionChangedEventArgs e)
    {
        Save();
    }

    private void ComboBox1_SelectionChanged(object? sender,
        SelectionChangedEventArgs e)
    {
        ComboBox2.SelectedItem = null;
        List1.Clear();
        CheckBox_Forge.IsChecked = false;
        CheckBox_Fabric.IsChecked = false;
        CheckBox_Quilt.IsChecked = false;

        Save();
    }

    private async void Button5_Click(object? sender, RoutedEventArgs e)
    {
        var Window = (VisualRoot as GameEditWindow)!;
        var res = await Window.Info.ShowWait(string.Format(
            App.GetLanguage("GameEditWindow.Tab1.Info1"), Obj.Name));
        if (!res)
            return;

        await GameBinding.DeleteGame(Obj);
    }

    private void Save()
    {
        Loaders loaders = Loaders.Normal;
        if (CheckBox_Forge.IsChecked == true)
        {
            loaders = Loaders.Forge;
        }
        else if (CheckBox_Fabric.IsChecked == true)
        {
            loaders = Loaders.Fabric;
        }
        else if (CheckBox_Quilt.IsChecked == true)
        {
            loaders = Loaders.Quilt;
        }

        GameBinding.SaveGame(Obj, ComboBox1.SelectedItem as string,
            loaders, ComboBox2.SelectedItem as string);
    }

    private async void Button4_Click(object? sender, RoutedEventArgs e)
    {
        var Window = (VisualRoot as GameEditWindow)!;
        await Window.Info3.ShowOne(App.GetLanguage("AddGameWindow.Info1"), false);
        Window.Info3.Close();
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
        var window = (VisualRoot as GameEditWindow)!;

        ComboBox2.IsEnabled = false;

        if (CheckBox_Forge.IsChecked == true)
        {
            window.Info1.Show(App.GetLanguage("AddGameWindow.Info6"));
            CheckBox_Fabric.IsEnabled = false;
            CheckBox_Quilt.IsEnabled = false;

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
        else if (CheckBox_Fabric.IsChecked == true)
        {
            window.Info1.Show(App.GetLanguage("AddGameWindow.Info5"));
            CheckBox_Forge.IsEnabled = false;
            CheckBox_Quilt.IsEnabled = false;

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
        else if (CheckBox_Quilt.IsChecked == true)
        {
            window.Info1.Show(App.GetLanguage("AddGameWindow.Info4"));
            CheckBox_Forge.IsEnabled = false;
            CheckBox_Fabric.IsEnabled = false;

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
        if (CheckBox_Quilt.IsChecked == true)
        {
            CheckBox_Forge.IsEnabled = false;
            CheckBox_Forge.IsChecked = false;
            CheckBox_Fabric.IsEnabled = false;
            CheckBox_Fabric.IsChecked = false;
            Button3_Click(sender, e);
        }
        else
        {
            ComboBox2.IsEnabled = false;
            CheckBox_Forge.IsEnabled = true;
            CheckBox_Fabric.IsEnabled = true;
            List1.Clear();
        }
    }
    private void Fabric_Click(object? sender, RoutedEventArgs e)
    {
        if (CheckBox_Fabric.IsChecked == true)
        {
            CheckBox_Forge.IsEnabled = false;
            CheckBox_Forge.IsChecked = false;
            CheckBox_Quilt.IsEnabled = false;
            CheckBox_Quilt.IsChecked = false;
            Button3_Click(sender, e);
        }
        else
        {
            ComboBox2.IsEnabled = false;
            CheckBox_Forge.IsEnabled = true;
            CheckBox_Quilt.IsEnabled = true;
            List1.Clear();
        }
    }

    private void Forge_Click(object? sender, RoutedEventArgs e)
    {
        if (CheckBox_Forge.IsChecked == true)
        {
            CheckBox_Fabric.IsEnabled = false;
            CheckBox_Fabric.IsChecked = false;
            CheckBox_Quilt.IsEnabled = false;
            CheckBox_Quilt.IsChecked = false;
            Button3_Click(sender, e);
        }
        else
        {
            ComboBox2.IsEnabled = false;
            CheckBox_Fabric.IsEnabled = true;
            CheckBox_Quilt.IsEnabled = true;
            List1.Clear();
        }
    }

    private async void Button2_Click(object? sender, RoutedEventArgs e)
    {
        var window = (VisualRoot as GameEditWindow)!;
        window.Info1.Show(App.GetLanguage("AddGameWindow.Info3"));
        var list = await GameBinding.GetForgeSupportVersion();
        if (list != null && list.Contains(Obj.Version))
        {
            CheckBox_Forge.IsEnabled = true;
        }

        list = await GameBinding.GetFabricSupportVersion();
        if (list != null && list.Contains(Obj.Version))
        {
            CheckBox_Fabric.IsEnabled = true;
        }

        list = await GameBinding.GetQuiltSupportVersion();
        if (list != null && list.Contains(Obj.Version))
        {
            CheckBox_Quilt.IsEnabled = true;
        }
        window.Info1.Close();
    }

    private async void Button1_Click(object? sender, RoutedEventArgs e)
    {
        var window = (VisualRoot as GameEditWindow)!;
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
        List.AddRange(GameBinding.GetGameVersion(CheckBox_Release.IsChecked,
            CheckBox_Snapshot.IsChecked, CheckBox_Other.IsChecked));

        ComboBox1.SelectedItem = Obj.Version;
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

        if (obj.GameType == GameType.Snapshot)
        {
            CheckBox_Snapshot.IsChecked = true;
        }
        else if (obj.GameType == GameType.Other)
        {
            CheckBox_Other.IsChecked = true;
        }
    }

    public void Update()
    {
        if (Obj == null)
            return;

        Load1();
        Load2();
        if (Obj.Loader != Loaders.Normal)
        {
            switch (Obj.Loader)
            {
                case Loaders.Forge:
                    CheckBox_Forge.IsEnabled = true;
                    CheckBox_Forge.IsChecked = true;
                    break;
                case Loaders.Fabric:
                    CheckBox_Fabric.IsEnabled = true;
                    CheckBox_Fabric.IsChecked = true;
                    break;
                case Loaders.Quilt:
                    CheckBox_Quilt.IsEnabled = true;
                    CheckBox_Quilt.IsChecked = true;
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
    }
}
