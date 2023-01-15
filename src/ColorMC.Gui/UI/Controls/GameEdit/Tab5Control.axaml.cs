using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using DynamicData;
using Avalonia.Interactivity;
using System;
using System.Collections.Generic;
using ColorMC.Gui.Utils.LaunchSetting;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab5Control : UserControl
{
    private readonly List<WorldControl> List = new();
    private GameEditWindow Window;
    private GameSettingObj Obj;
    private WorldControl? Last;

    public Tab5Control()
    {
        InitializeComponent();

        Button_A1.PointerLeave += Button_A1_PointerLeave;
        Button_A.PointerEnter += Button_A_PointerEnter;

        Button_I1.PointerLeave += Button_I1_PointerLeave;
        Button_I.PointerEnter += Button_I_PointerEnter;

        Button_R1.PointerLeave += Button_R1_PointerLeave;
        Button_R.PointerEnter += Button_R_PointerEnter;

        Button_R1.Click += Button_R1_Click;
        Button_A1.Click += Button_A1_Click;
        Button_I1.Click += Button_I1_Click;

        LayoutUpdated += Tab5Control_LayoutUpdated1;
    }

    private void Button_R1_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private void Button_A1_Click(object? sender, RoutedEventArgs e)
    {
        
    }

    private async void Button_I1_Click(object? sender, RoutedEventArgs e)
    {
        OpenFileDialog openFile = new()
        {
            Title = "地图压缩包",
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
            var res = await GameBinding.AddWorld(Obj, file[0]);
            if (!res)
            {
                Window.Info2.Show(Localizer.Instance["GameEditWindow.Tab4.Info2"]);
                return;
            }
            Window.Info2.Show(Localizer.Instance["GameEditWindow.Tab4.Info2"]);
            Load();
        }
    }

    private void Tab5Control_LayoutUpdated1(object? sender, EventArgs e)
    {
        Expander_I.MakePadingNull();
        Expander_A.MakePadingNull();
        Expander_R.MakePadingNull();
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

    public void SetWindow(GameEditWindow window)
    {
        Window = window;
    }

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;
    }

    public void SetSelect(WorldControl item) 
    {
        Last?.SetSelect(false);
        Last = item;
        Last.SetSelect(true);
    }

    private async void Load() 
    {
        Window.Info1.Show("正在加载地图列表");
        List.Clear();
        ListBox_Items.Children.Clear();

        var res = await GameBinding.GetWorlds(Obj);
        Window.Info1.Close();
        foreach (var item in res)
        {
            var con = new WorldControl();
            con.SetWindow(this);
            con.Load(item);
            ListBox_Items.Children.Add(con);
            List.Add(con);
        }
    }

    private void Tab5Control_LayoutUpdated(object? sender, EventArgs e)
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
