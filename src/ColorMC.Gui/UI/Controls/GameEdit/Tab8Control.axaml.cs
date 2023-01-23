using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;
using System;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab8Control : UserControl
{
    private readonly List<ResourcePackControl> List = new();
    private GameEditWindow Window;
    private GameSettingObj Obj;
    private ResourcePackControl? Last;
    private AddResourcePackWindow? AddResourcePackWindow;

    public Tab8Control()
    {
        InitializeComponent();

        Button_A1.PointerExited += Button_A1_PointerLeave;
        Button_A.PointerEntered += Button_A_PointerEnter;

        Button_I1.PointerExited += Button_I1_PointerLeave;
        Button_I.PointerEntered += Button_I_PointerEnter;

        Button_R1.PointerExited += Button_R1_PointerLeave;
        Button_R.PointerEntered += Button_R_PointerEnter;

        Button_R1.Click += Button_R1_Click;
        Button_A1.Click += Button_A1_Click;
        Button_I1.Click += Button_I1_Click;

        LayoutUpdated += Tab8Control_LayoutUpdated;
    }

    public void CloseAddResourcepack()
    {
        AddResourcePackWindow = null;
    }

    private void Button_R1_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private void Button_A1_Click(object? sender, RoutedEventArgs e)
    {
        if (AddResourcePackWindow == null)
        {
            AddResourcePackWindow = new();
            AddResourcePackWindow.SetTab8Control(Obj, this);
            AddResourcePackWindow.Show();
        }
        else
        {
            AddResourcePackWindow.Activate();
        }
    }

    private async void Button_I1_Click(object? sender, RoutedEventArgs e)
    {
        OpenFileDialog openFile = new()
        {
            Title = Localizer.Instance["GameEditWindow.Tab8.Info2"],
            AllowMultiple = false,
            Filters = new()
            {
                new FileDialogFilter()
                {
                    Extensions = new()
                    {
                        "zip"
                    }
                }
            }
        };

        var file = await openFile.ShowAsync(Window);
        if (file?.Length > 0)
        {
            var res = await GameBinding.AddResourcepack(Obj, file[0]);
            if (!res)
            {
                Window.Info2.Show(Localizer.Instance["GameEditWindow.Tab4.Info2"]);
                return;
            }
            Window.Info2.Show(Localizer.Instance["GameEditWindow.Tab4.Info2"]);
            Load();
        }
    }

    private void Button_I1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_I.IsExpanded = false;
    }

    private void Button_I_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_I.IsExpanded = true;
    }

    private void Button_A1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_A.IsExpanded = false;
    }

    private void Button_A_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_A.IsExpanded = true;
    }
    private void Button_R1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_R.IsExpanded = false;
    }

    private void Button_R_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_R.IsExpanded = true;
    }

    public async void AddResourcepack(CurseForgeObj.Data.LatestFiles data)
    {
        Window.Info1.Show(Localizer.Instance["GameEditWindow.Tab8.Info6"]);
        var res = await GameBinding.DownloadResourcepack(Obj, data);
        Window.Info1.Close();
        if (res)
        {
            Window.Info2.Show(Localizer.Instance["GameEditWindow.Tab8.Info7"]);
            Load();
        }
        else
        {
            Window.Info.Show(Localizer.Instance["GameEditWindow.Tab8.Error2"]);
        }
    }

    public async void Delete(ResourcepackDisplayObj obj)
    {
        var res = await Window.Info.ShowWait(
            string.Format(Localizer.Instance["GameEditWindow.Tab8.Info1"], obj.Local));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteResourcepack(obj.Pack);
        Window.Info2.Show(Localizer.Instance["GameEditWindow.Tab4.Info3"]);
        Load();
    }

    public void SetWindow(GameEditWindow window)
    {
        Window = window;
    }

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;
    }

    public void SetSelect(ResourcePackControl item)
    {
        Last?.SetSelect(false);
        Last = item;
        Last.SetSelect(true);
    }

    private async void Load()
    {
        Window.Info1.Show(Localizer.Instance["GameEditWindow.Tab8.Info8"]);
        List.Clear();
        ListBox_Items.Children.Clear();

        var res = await GameBinding.GetResourcepacks(Obj);
        Window.Info1.Close();
        foreach (var item in res)
        {
            var con = new ResourcePackControl();
            con.SetWindow(this);
            con.Load(item);
            ListBox_Items.Children.Add(con);
            List.Add(con);
        }
    }

    private void Tab8Control_LayoutUpdated(object? sender, EventArgs e)
    {
        Expander_I.MakePadingNull();
        Expander_A.MakePadingNull();
        Expander_R.MakePadingNull();
    }

    public void Update()
    {
        if (Obj == null)
            return;

        Load();
    }
}
