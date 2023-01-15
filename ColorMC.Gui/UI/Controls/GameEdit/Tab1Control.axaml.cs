using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;
using DynamicData;
using System.Collections.ObjectModel;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab1Control : UserControl
{
    private readonly ObservableCollection<string> List = new();
    private readonly ObservableCollection<string> List1 = new();
    private readonly ObservableCollection<string> List2 = new();
    private GameEditWindow Window;
    private GameSettingObj Obj;
    public Tab1Control()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Button3.Click += Button3_Click;
        Button4.Click += Button4_Click;

        CheckBox_Forge.Click += Forge_Click;
        CheckBox_Fabric.Click += Fabric_Click;
        CheckBox_Quilt.Click += Quilt_Click;
        CheckBox_Release.Click += Other_Click;
        CheckBox_Snapshot.Click += Other_Click;
        CheckBox_Other.Click += Other_Click;

        Button_Set.Click += Button_Set_Click;

        ComboBox1.Items = List;
        ComboBox2.Items = List1;
        ComboBox3.Items = List2;
    }

    private void Button_Set_Click(object? sender, RoutedEventArgs e)
    {
        GameBinding.MoveGameGroup(Obj, ComboBox2.SelectedItem as string);

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
        await Window.Info3.ShowOne(Localizer.Instance["AddGameWindow.Info15"], false);
        Window.Info3.Close();
        if (Window.Info3.Cancel)
        {
            return;
        }

        var res = Window.Info3.Read().Item1;
        if (string.IsNullOrWhiteSpace(res))
        {
            Window.Info1.Show(Localizer.Instance["AddGameWindow.Error6"]);
            return;
        }

        if (!GameBinding.AddGameGroup(res))
        {
            Window.Info1.Show(Localizer.Instance["AddGameWindow.Error7"]);
            return;
        }

        Window.Info2.Show(Localizer.Instance["AddGameWindow.Info2"]);

        Load2();
    }

    private void Other_Click(object? sender, RoutedEventArgs e)
    {
        Load1();
    }

    private async void Button3_Click(object? sender, RoutedEventArgs e)
    {
        ComboBox2.IsEnabled = false;

        if (CheckBox_Forge.IsChecked == true)
        {
            Window.Info1.Show(Localizer.Instance["AddGameWindow.Info6"]);
            CheckBox_Fabric.IsEnabled = false;
            CheckBox_Quilt.IsEnabled = false;

            var list = await GameBinding.GetForgeVersion(Obj.Version);
            Window.Info1.Close();
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
            Window.Info1.Show(Localizer.Instance["AddGameWindow.Info5"]);
            CheckBox_Forge.IsEnabled = false;
            CheckBox_Quilt.IsEnabled = false;

            var list = await GameBinding.GetFabricVersion(Obj.Version);
            Window.Info1.Close();
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
            Window.Info1.Show(Localizer.Instance["AddGameWindow.Info4"]);
            CheckBox_Forge.IsEnabled = false;
            CheckBox_Fabric.IsEnabled = false;

            var list = await GameBinding.GetQuiltVersion(Obj.Version);
            Window.Info1.Close();
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
            CheckBox_Fabric.IsEnabled = false;
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
            CheckBox_Quilt.IsEnabled = false;
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
            CheckBox_Quilt.IsEnabled = false;
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
        Window.Info1.Show(Localizer.Instance["AddGameWindow.Info3"]);
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
        Window.Info1.Close();
    }

    private async void Button1_Click(object? sender, RoutedEventArgs e)
    {
        Window.Info1.Show(Localizer.Instance["GameEditWindow.Info1"]);
        var res = await GameBinding.ReloadVersion();
        Window.Info1.Close();
        if (!res)
        {
            Window.Info.Show(Localizer.Instance["GameEditWindow.Error1"]);
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

    public void SetWindow(GameEditWindow window)
    {
        Window = window;
    }

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;
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
    }
}
