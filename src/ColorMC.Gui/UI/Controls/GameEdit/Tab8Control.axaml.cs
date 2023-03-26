using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.GameEdit.Items;
using ColorMC.Gui.UIBinding;
using System.Collections.Generic;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab8Control : UserControl
{
    private readonly List<ResourcePackControl> List = new();
    private GameSettingObj Obj;
    private ResourcePackControl? Last;

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
        Button_R.Click += Button_R1_Click;
        Button_A.Click += Button_A1_Click;
        Button_I.Click += Button_I1_Click;

        Button1.Click += Button1_Click;
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        BaseBinding.OpPath(Obj, PathType.ResourcepackPath);
    }

    private void Button_R1_Click(object? sender, RoutedEventArgs e)
    {
        Load();
    }

    private void Button_A1_Click(object? sender, RoutedEventArgs e)
    {
        App.ShowAdd(Obj, FileType.Resourcepack);
    }

    private async void Button_I1_Click(object? sender, RoutedEventArgs e)
    {
        var window = App.FindRoot(VisualRoot);
        var file = await BaseBinding.AddFile(window as Window, Obj, FileType.Resourcepack);
        if (file == null)
            return;

        if (file == false)
        {
            window.Info2.Show(App.GetLanguage("GameEditWindow.Tab8.Error1"));
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

    public async void Delete(ResourcepackDisplayObj obj)
    {
        var window = App.FindRoot(VisualRoot);
        var res = await window.Info.ShowWait(
            string.Format(App.GetLanguage("GameEditWindow.Tab8.Info1"), obj.Local));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteResourcepack(obj.Pack);
        window.Info2.Show(App.GetLanguage("GameEditWindow.Tab4.Info3"));
        Load();
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
        var window = App.FindRoot(VisualRoot);
        window.Info1.Show(App.GetLanguage("GameEditWindow.Tab8.Info5"));
        List.Clear();
        ListBox_Items.Children.Clear();

        var res = await GameBinding.GetResourcepacks(Obj);
        window.Info1.Close();
        foreach (var item in res)
        {
            var con = new ResourcePackControl();
            con.SetWindow(this);
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
}
