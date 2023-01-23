using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils.LaunchSetting;
using System;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab9Control : UserControl
{
    private readonly List<ScreenshotDisplayObj> List = new();
    private GameEditWindow Window;
    private GameSettingObj Obj;
    private ScreenshotControl? Last;

    public Tab9Control()
    {
        InitializeComponent();

        Button_R1.PointerExited += Button_R1_PointerLeave;
        Button_R.PointerEntered += Button_R_PointerEnter;

        Button_R1.Click += Button_R1_Click;

        LayoutUpdated += Tab9Control_LayoutUpdated;
    }

    private void Button_R1_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private void Button_R1_PointerLeave(object? sender, PointerEventArgs e)
    {
        Expander_R.IsExpanded = false;
    }

    private void Button_R_PointerEnter(object? sender, PointerEventArgs e)
    {
        Expander_R.IsExpanded = true;
    }

    private void Tab9Control_LayoutUpdated(object? sender, EventArgs e)
    {
        Expander_R.MakePadingNull();
    }

    public async void Delete(ScreenshotDisplayObj obj)
    {
        var res = await Window.Info.ShowWait(
            string.Format(Localizer.Instance["GameEditWindow.Tab8.Info1"], obj.Local));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteResourcepack(obj.Local);
        Window.Info2.Show(Localizer.Instance["GameEditWindow.Tab4.Info3"]);
        Load();
    }

    private async void Load()
    {
        //Window.Info1.Show(Localizer.Instance["GameEditWindow.Tab8.Info8"]);
        //List.Clear();
        //ListBox_Items.Children.Clear();

        //var res = await GameBinding.GetResourcepacks(Obj);
        //Window.Info1.Close();
        //foreach (var item in res)
        //{
        //    var con = new ResourcePackControl();
        //    con.SetWindow(this);
        //    con.Load(item);
        //    ListBox_Items.Children.Add(con);
        //    List.Add(con);
        //}
    }

    public void SetSelect(ScreenshotControl item)
    {
        Last?.SetSelect(false);
        Last = item;
        Last.SetSelect(true);
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

        Load();
    }
}
