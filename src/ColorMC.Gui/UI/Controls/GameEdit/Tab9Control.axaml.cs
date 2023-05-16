using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.GameEdit.Items;
using ColorMC.Gui.UIBinding;
using System.Collections.Generic;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab9Control : UserControl
{
    private readonly List<ScreenshotControl> List = new();
    private GameSettingObj Obj;
    private ScreenshotControl? Last;

    public Tab9Control()
    {
        InitializeComponent();

        Button_C1.PointerExited += Button_C1_PointerLeave;
        Button_C.PointerEntered += Button_C_PointerEnter;

        Button_R1.PointerExited += Button_R1_PointerLeave;
        Button_R.PointerEntered += Button_R_PointerEnter;

        Button_R1.Click += Button_R1_Click;
        Button_C1.Click += Button_C1_Click;
        Button_R.Click += Button_R1_Click;
        Button_C.Click += Button_C1_Click;

        Button1.Click += Button1_Click;
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        BaseBinding.OpPath(Obj.GetScreenshotsPath());
    }

    private async void Button_C1_Click(object? sender, RoutedEventArgs e)
    {
        var Window = App.FindRoot(VisualRoot);
        var res = await Window.OkInfo.ShowWait(
            string.Format(App.GetLanguage("GameEditWindow.Tab9.Info2"), Obj.Name));
        if (!res)
        {
            return;
        }

        GameBinding.ClearScreenshots(Obj);
        Window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab4.Info3"));
        Load();
    }

    private void Button_R1_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private void Button_C1_PointerLeave(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(Button_C1, null, CancellationToken.None);
    }

    private void Button_C_PointerEnter(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(null, Button_C1, CancellationToken.None);
    }

    private void Button_R1_PointerLeave(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(Button_R1, null, CancellationToken.None);
    }

    private void Button_R_PointerEnter(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(null, Button_R1, CancellationToken.None);
    }
    public async void Delete(ScreenshotDisplayObj obj)
    {
        var Window = App.FindRoot(VisualRoot);
        var res = await Window.OkInfo.ShowWait(
            string.Format(App.GetLanguage("GameEditWindow.Tab9.Info1"), obj.Local));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteScreenshot(obj.Local);
        Window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab4.Info3"));
        Load();
    }

    private async void Load()
    {
        var window = App.FindRoot(VisualRoot);
        window.ProgressInfo.Show(App.GetLanguage("GameEditWindow.Tab9.Info3"));
        List.Clear();
        WrapPanel1.Children.Clear();

        var res = await GameBinding.GetScreenshots(Obj);
        window.ProgressInfo.Close();
        foreach (var item in res)
        {
            var con = new ScreenshotControl();
            con.SetWindow(this);
            con.Load(item);
            WrapPanel1.Children.Add(con);
            List.Add(con);
        }
    }

    public void SetSelect(ScreenshotControl item)
    {
        Last?.SetSelect(false);
        Last = item;
        Last.SetSelect(true);
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
