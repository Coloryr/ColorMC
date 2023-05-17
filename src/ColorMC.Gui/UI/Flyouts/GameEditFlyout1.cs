using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using ColorMC.Gui.UI.Controls.GameEdit;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout1
{
    private readonly IEnumerable<ModDisplayModel> List;
    private readonly ModDisplayModel Obj;
    private readonly Control Con;
    private readonly GameEditTab4Model Model;
    private bool Single;

    public GameEditFlyout1(Control con, IList obj, GameEditTab4Model model)
    {
        Con = con;
        Model = model;
        List = obj.Cast<ModDisplayModel>();
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
            Model.DisE(Obj);
        }
        else
        {
            foreach (var item in List)
            {
                Model.DisE(item);
            }
        }
    }

    private void Button2_Click()
    {
        if (Single)
        {
            Model.Delete(Obj);
        }
        else
        {
            Model.Delete(List);
        }
    }

    private void Button3_Click()
    {
        BaseBinding.OpFile(Obj.Local);
    }

    private async void Button7_Click()
    {
        var list = new List<IStorageFile>();
        var window = Con.GetVisualRoot();
        if (window is TopLevel top)
        {
            foreach (var item in List)
            {
                var data = await top.StorageProvider.TryGetFileFromPathAsync(item.Local);
                if (data == null)
                    continue;

                list.Add(data);
            }
            await BaseBinding.CopyFileClipboard(TopLevel.GetTopLevel(Con), list);
        }
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
