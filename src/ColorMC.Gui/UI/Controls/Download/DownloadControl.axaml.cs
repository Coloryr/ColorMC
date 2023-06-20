using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Core;
using ColorMC.Gui.UI.Model.Download;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Download;

public partial class DownloadControl : UserControl, IUserControl
{
    private readonly DownloadModel model;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public DownloadControl()
    {
        InitializeComponent();

        model = new(this);
        DataContext = model;

        Button_P1.PointerExited += Button_P1_PointerLeave;
        Button_P.PointerEntered += Button_P_PointerEnter;

        Button_S1.PointerExited += Button_S1_PointerLeave;
        Button_S.PointerEntered += Button_S_PointerEnter;
    }

    private void Button_S1_PointerLeave(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(Button_S1, null, CancellationToken.None);
        Button_S.IsVisible = true;
    }

    private void Button_S_PointerEnter(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(null, Button_S1, CancellationToken.None);
        Button_S.IsVisible = false;
    }

    private void Button_P1_PointerLeave(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(Button_P1, null, CancellationToken.None);
        Button_P.IsVisible = true;
    }

    private void Button_P_PointerEnter(object? sender, PointerEventArgs e)
    {
        App.CrossFade100.Start(null, Button_P1, CancellationToken.None);
        Button_P.IsVisible = false;
    }

    public void Opened()
    {
        Window.SetTitle(App.GetLanguage("DownloadWindow.Title"));

        DataGrid1.SetFontColor();
    }

    public void Closed()
    {
        model.Close();

        ColorMCCore.DownloadItemStateUpdate = null;

        App.DownloadWindow = null;
    }

    public void Load()
    {
        model.Load();
    }

    public async Task<bool> Closing()
    {
        var windows = App.FindRoot(VisualRoot);
        if (BaseBinding.IsDownload)
        {
            var res = await windows.OkInfo.ShowWait(App.GetLanguage("DownloadWindow.Info4"));
            if (res)
            {
                BaseBinding.DownloadStop();
                return false;
            }
            return true;
        }

        return false;
    }
}
