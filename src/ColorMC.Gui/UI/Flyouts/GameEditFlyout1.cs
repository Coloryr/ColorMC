using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Controls.GameEdit;
using ColorMC.Gui.UIBinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout1
{
    private readonly IEnumerable<ModDisplayObj> List;
    private readonly Tab4Control Con;
    private ModDisplayObj Obj;
    private bool Single;

    public GameEditFlyout1(Tab4Control con, IList obj)
    {
        Con = con;
        List = obj.Cast<ModDisplayObj>();
        if (List.Count() == 1)
        {
            Single = true;
            Obj = List.First();
        }

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("GameEditWindow.Flyouts1.Text1"), true, Button1_Click),
            (App.GetLanguage("GameEditWindow.Flyouts1.Text2"), true, Button2_Click),
            (App.GetLanguage("Button.OpFile"), Single, Button3_Click),
            (App.GetLanguage("GameEditWindow.Flyouts1.Text6"), true, Button7_Click),
            (App.GetLanguage("GameEditWindow.Flyouts1.Text3"), Single, Button4_Click),
            (App.GetLanguage("GameEditWindow.Flyouts1.Text4"), Single
                && !string.IsNullOrWhiteSpace(Obj?.Url), Button5_Click),
            (App.GetLanguage("GameEditWindow.Flyouts1.Text5"), Single
                && !string.IsNullOrWhiteSpace(Obj?.PID) && !string.IsNullOrWhiteSpace(Obj?.FID), Button6_Click),
        }, con);
    }

    private void Button1_Click()
    {
        if (Single)
        {
            Con.DisE(Obj);
        }
        else
        {
            foreach (var item in List)
            {
                Con.DisE(item);
            }
        }
    }

    private void Button2_Click()
    {
        if (Single)
        {
            Con.Delete(Obj);
        }
        else
        {
            Con.Delete(List);
        }
    }

    private void Button3_Click()
    {
        BaseBinding.OpFile(Obj.Local);
    }

    private async void Button7_Click()
    {
        var list = new List<IStorageFile>();
        var window = App.FindRoot(Con.GetVisualRoot());
        foreach (var item in List)
        {
            list.Add(await (window as Window).StorageProvider.TryGetFileFromPathAsync(item.Local));
        }
        await BaseBinding.CopyFileClipboard(list);
    }

    private void Button4_Click()
    {
        WebBinding.OpenMcmod(Obj);
    }

    private void Button5_Click()
    {
        BaseBinding.OpUrl(Obj.Url);
    }

    private void Button6_Click()
    {
        App.ShowAdd(Obj.Obj.Game, Obj);
    }
}
