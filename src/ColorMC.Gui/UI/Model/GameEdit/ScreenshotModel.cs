using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class ScreenshotModel : ObservableObject
{
    public ScreenshotDisplayObj Screenshot { get; }

    private ILoadFuntion<ScreenshotModel> _top;
    private IUserControl _con;

    [ObservableProperty]
    private bool _isSelect;

    public string Name => Screenshot.Name;
    public Bitmap Pic => Screenshot.Image;

    public ScreenshotModel(IUserControl con, ILoadFuntion<ScreenshotModel> top, ScreenshotDisplayObj obj)
    {
        _con = con;
        _top = top;
        Screenshot = obj;
    }

    public void Select()
    {
        _top.SetSelect(this);
    }

    public void Flyout(Control con)
    {
        _ = new GameEditFlyout4(con, this);
    }

    public async void Delete()
    {
        var Window = _con.Window;
        var res = await Window.OkInfo.ShowWait(
            string.Format(App.GetLanguage("GameEditWindow.Tab9.Info1"), Screenshot.Local));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteScreenshot(Screenshot.Local);
        Window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab4.Info3"));
        await _top.Load();
    }
}
