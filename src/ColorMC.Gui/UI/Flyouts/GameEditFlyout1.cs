using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameEdit;
using ColorMC.Gui.UIBinding;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ColorMC.Gui.UI.Flyouts;

public class GameEditFlyout1
{
    private readonly IEnumerable<ModDisplayModel> _list;
    private readonly ModDisplayModel _obj;
    private readonly Control _con;
    private readonly GameEditTab4Model _model;
    private readonly bool _single;

    public GameEditFlyout1(Control con, IList obj, GameEditTab4Model model)
    {
        _con = con;
        _model = model;
        _list = obj.Cast<ModDisplayModel>();
        if (_list.Count() == 1)
        {
            _single = true;
            _obj = _list.First();
        }

        var fy = new FlyoutsControl(new()
        {
            (App.GetLanguage("GameEditWindow.Flyouts1.Text1"), true, Button1_Click),
            (App.GetLanguage("GameEditWindow.Flyouts1.Text2"), true, Button2_Click),
            (App.GetLanguage("Button.OpFile"), _single, Button3_Click),
            (App.GetLanguage("GameEditWindow.Flyouts1.Text6"), true, Button7_Click),
            (App.GetLanguage("GameEditWindow.Flyouts1.Text3"), _single, Button4_Click),
            (App.GetLanguage("GameEditWindow.Flyouts1.Text4"), _single
                && !string.IsNullOrWhiteSpace(_obj?.Url), Button5_Click),
            (App.GetLanguage("GameEditWindow.Flyouts1.Text5"), _single
                && !string.IsNullOrWhiteSpace(_obj?.PID) && !string.IsNullOrWhiteSpace(_obj?.FID), Button6_Click),
        }, con);
    }

    private void Button1_Click()
    {
        if (_single)
        {
            _model.DisE(_obj);
        }
        else
        {
            foreach (var item in _list)
            {
                _model.DisE(item);
            }
        }
    }

    private void Button2_Click()
    {
        if (_single)
        {
            _model.Delete(_obj);
        }
        else
        {
            _model.Delete(_list);
        }
    }

    private void Button3_Click()
    {
        BaseBinding.OpFile(_obj.Local);
    }

    private async void Button7_Click()
    {
        var list = new List<IStorageFile>();
        var window = _con.GetVisualRoot();
        if (window is TopLevel top)
        {
            foreach (var item in _list)
            {
                var data = await top.StorageProvider.TryGetFileFromPathAsync(item.Local);
                if (data == null)
                    continue;

                list.Add(data);
            }
            await BaseBinding.CopyFileClipboard(TopLevel.GetTopLevel(_con), list);
        }
    }

    private void Button4_Click()
    {
        WebBinding.OpenMcmod(_obj);
    }

    private void Button5_Click()
    {
        BaseBinding.OpUrl(_obj.Url);
    }

    private void Button6_Click()
    {
        App.ShowAdd(_obj.Obj.Game, _obj);
    }
}
