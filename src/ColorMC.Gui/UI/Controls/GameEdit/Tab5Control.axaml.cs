using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.GameEdit.Items;
using ColorMC.Gui.UIBinding;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab5Control : UserControl
{
    private readonly List<WorldControl> List = new();
    private GameSettingObj Obj;
    private WorldControl? Last;

    public Tab5Control()
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
        Button_R.Click += Button_R1_Click;
        Button_A.Click += Button_A1_Click;
        Button_I.Click += Button_I1_Click;

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Button3.Click += Button3_Click;
    }

    private async void Button3_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        var info = new DirectoryInfo(Obj.GetWorldBackupPath());
        if (!info.Exists)
        {
            info.Create();
        }

        var list = info.GetFiles();
        var names = new List<string>();
        foreach (var item in list)
        {
            names.Add(item.Name);
        }
        await window.Info5.Show(App.GetLanguage("GameEditWindow.Tab5.Info11"), names);
        if (window.Info5.Cancel)
            return;
        var item1 = list[window.Info5.Read().Item1];
        var res = await window.Info.ShowWait(App.GetLanguage("GameEditWindow.Tab5.Info12"));
        if (!res)
            return;

        window.Info1.Show(App.GetLanguage("GameEditWindow.Tab5.Info13"));
        res = await GameBinding.BackupWorld(Obj, item1);
        window.Info1.Close();
        if (!res)
        {
            window.Info.Show(App.GetLanguage("GameEditWindow.Tab5.Error4"));
        }
        else
        {
            window.Info2.Show(App.GetLanguage("GameEditWindow.Tab5.Info14"));
            Load();
        }
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        BaseBinding.OpPath(Obj.GetWorldBackupPath());
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        BaseBinding.OpPath(Obj.GetSavesPath());
    }

    private void Button_R1_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private void Button_A1_Click(object? sender, RoutedEventArgs e)
    {
        App.ShowAdd(Obj, FileType.World);
    }

    private async void Button_I1_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        var file = await BaseBinding.AddFile(window as Window, Obj, FileType.World);
        if (file == null)
            return;

        if (file == false)
        {
            window.Info2.Show(App.GetLanguage("GameEditWindow.Tab5.Error2"));
            return;
        }

        window.Info2.Show(App.GetLanguage("GameEditWindow.Tab4.Info2"));
        Load();
    }

    private void Button_I1_PointerLeave(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(Button_I1, null, CancellationToken.None);
    }

    private void Button_I_PointerEnter(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(null, Button_I1, CancellationToken.None);
    }

    private void Button_A1_PointerLeave(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(Button_A1, null, CancellationToken.None);
    }

    private void Button_A_PointerEnter(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(null, Button_A1, CancellationToken.None);
    }
    private void Button_R1_PointerLeave(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(Button_R1, null, CancellationToken.None);
    }

    private void Button_R_PointerEnter(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(null, Button_R1, CancellationToken.None);
    }

    public async void Export(WorldDisplayObj obj)
    {
        var window = App.FindRoot(VisualRoot);
        window.Info1.Show(App.GetLanguage("GameEditWindow.Tab5.Info4"));
        var file = await BaseBinding.SaveFile(window as Window, FileType.World, new object[]
            { obj });
        window.Info1.Close();
        if (file == null)
            return;

        if (file == false)
        {
            window.Info.Show(App.GetLanguage("GameEditWindow.Tab5.Error1"));
        }
        else
        {
            window.Info2.Show(App.GetLanguage("GameEditWindow.Tab5.Info3"));
        }
    }

    public async void Delete(WorldDisplayObj obj)
    {
        var window = App.FindRoot(VisualRoot);
        var res = await window!.Info.ShowWait(
            string.Format(App.GetLanguage("GameEditWindow.Tab5.Info1"), obj.Name));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteWorld(obj.World);
        window.Info2.Show(App.GetLanguage("GameEditWindow.Tab4.Info3"));
        Load();
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
        var window = App.FindRoot(VisualRoot);
        window.Info1.Show(App.GetLanguage("GameEditWindow.Tab5.Info7"));
        List.Clear();
        ListBox_Items.Children.Clear();

        var res = await GameBinding.GetWorlds(Obj!);
        window.Info1.Close();
        foreach (var item in res)
        {
            var con = new WorldControl();
            con.Load(item);
            ListBox_Items.Children.Add(con);
            List.Add(con);
        }
    }

    public void Update()
    {
        if (Obj == null)
            return;

        Load();
    }

    public async void Backup(WorldDisplayObj obj)
    {
        var Window = App.FindRoot(VisualRoot);
        Window.Info1.Show(App.GetLanguage("GameEditWindow.Tab5.Info9"));
        var res = await GameBinding.BackupWorld(obj.World);
        Window.Info1.Close();
        if (res)
        {
            Window.Info2.Show(App.GetLanguage("GameEditWindow.Tab5.Info10"));
        }
        else
        {
            Window.Info1.Show(App.GetLanguage("GameEditWindow.Tab5.Error3"));
        }
    }
}
