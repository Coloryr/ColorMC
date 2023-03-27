using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Flyouts1Control : UserControl
{
    private IEnumerable<ModDisplayObj> List;
    private ModDisplayObj Obj;
    private bool Single;
    private FlyoutBase FlyoutBase;
    private Tab4Control Con;
    public Flyouts1Control()
    {
        InitializeComponent();

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
        Button3.Click += Button3_Click;
        Button4.Click += Button4_Click;
        Button5.Click += Button5_Click;
        Button6.Click += Button6_Click;
        Button7.Click += Button7_Click;
    }

    private async void Button7_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        var list = new List<IStorageFile>();
        var window = App.FindRoot(Con.GetVisualRoot());
        foreach (var item in List)
        {
            list.Add(await (window as Window).StorageProvider.TryGetFileFromPathAsync(item.Local));
        }
        await BaseBinding.CopyFileClipboard(list);
    }

    private void Button6_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        App.ShowAdd(Obj.Obj.Game, Obj);
    }

    private void Button5_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        BaseBinding.OpUrl(Obj.Url);
    }

    private void Button4_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        GameBinding.OpenMcmod(Obj);
    }

    private void Button3_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        BaseBinding.OpFile(Obj.Local);
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        if (Single)
        {
            Con.Delete(Obj);
        }
        else
        {
            Con.Delete(List);
        }
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        FlyoutBase.Hide();
        Con.DisE(Obj);
    }

    public void Set(FlyoutBase fb, IEnumerable<ModDisplayObj> obj, Tab4Control con)
    {
        List = obj;
        if (List.Count() == 1)
        {
            Single = true;
            Obj = List.First();
            if (string.IsNullOrWhiteSpace(Obj.Url))
            {
                Button5.IsEnabled = false;
            }
            if (string.IsNullOrWhiteSpace(Obj.PID) || string.IsNullOrWhiteSpace(Obj.FID))
            {
                Button6.IsEnabled = false;
            }
        }
        else
        {
            Button1.IsEnabled = false;
            Button3.IsEnabled = false;
            Button4.IsEnabled = false;
            Button5.IsEnabled = false;
            Button6.IsEnabled = false;
        }
        Con = con;
        FlyoutBase = fb;
    }
}

public class GameEditFlyout1 : PopupFlyoutBase
{
    private readonly IEnumerable<ModDisplayObj> Obj;
    private readonly Tab4Control Con;
    public GameEditFlyout1(Tab4Control con, IList obj)
    {
        Con = con;
        Obj = obj.Cast<ModDisplayObj>();
    }

    protected override Control CreatePresenter()
    {
        var control = new Flyouts1Control();
        control.Set(this, Obj, Con);
        return control;
    }
}
