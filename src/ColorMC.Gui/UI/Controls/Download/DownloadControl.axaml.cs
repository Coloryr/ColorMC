using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Core;
using ColorMC.Gui.UI.Model.Download;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using System.ComponentModel;
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
        model.PropertyChanged += Model_PropertyChanged;
        DataContext = model;

        Button_P1.PointerExited += Button_P1_PointerLeave;
        Button_P.PointerEntered += Button_P_PointerEnter;

        Button_S1.PointerExited += Button_S1_PointerLeave;
        Button_S.PointerEntered += Button_S_PointerEnter;
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "DisplayP")
        {
            if (model.DisplayP)
            {
                App.CrossFade100.Start(null, Button_P1, CancellationToken.None);
            }
            else
            {
                App.CrossFade100.Start(Button_P1, null, CancellationToken.None);
            }
        }
        else if (e.PropertyName == "DisplayS")
        {
            if (model.DisplayS)
            {
                App.CrossFade100.Start(null, Button_S1, CancellationToken.None);
            }
            else
            {
                App.CrossFade100.Start(Button_S1, null, CancellationToken.None);
            }
        }
    }

    private void Button_S1_PointerLeave(object? sender, PointerEventArgs e)
    {
        model.DisplayS = false;
    }

    private void Button_S_PointerEnter(object? sender, PointerEventArgs e)
    {
        model.DisplayS = true;
    }

    private void Button_P1_PointerLeave(object? sender, PointerEventArgs e)
    {
        model.DisplayP = false;
    }

    private void Button_P_PointerEnter(object? sender, PointerEventArgs e)
    {
        model.DisplayP = true;
    }

    public void Opened()
    {
        Window.SetTitle(App.GetLanguage("DownloadWindow.Title"));

        DataGrid1.MakeTran();
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
