using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class ScreenshotModel : ObservableObject
{
    public ScreenshotDisplayObj Screenshot { get; }

    private IScreenshotFuntion Top;
    private IUserControl Con;

    [ObservableProperty]
    private bool isSelect;

    public string Name => Screenshot.Name;
    public Bitmap Pic => Screenshot.Image;

    public ScreenshotModel(IUserControl con, IScreenshotFuntion top, ScreenshotDisplayObj obj)
    {
        Con = con;
        Top = top;
        Screenshot = obj;
    }

    public void Select()
    {
        Top.SetSelect(this);
    }

    public void Flyout(Control con)
    {
        _ = new GameEditFlyout4(con, this);
    }

    public async void Delete()
    {
        var Window = Con.Window;
        var res = await Window.OkInfo.ShowWait(
            string.Format(App.GetLanguage("GameEditWindow.Tab9.Info1"), Screenshot.Local));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteScreenshot(Screenshot.Local);
        Window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab4.Info3"));
        Top.Load();
    }
}
