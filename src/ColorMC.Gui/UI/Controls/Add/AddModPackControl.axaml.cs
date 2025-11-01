using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Input;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Add;

namespace ColorMC.Gui.UI.Controls.Add;

/// <summary>
/// 下载整合包窗口
/// </summary>
public partial class AddModPackControl : BaseUserControl
{
    public AddModPackControl() : base(WindowManager.GetUseName<AddModPackControl>())
    {
        InitializeComponent();

        Title = LanguageUtils.Get("AddModPackWindow.Title");

        ModPackFiles.PointerPressed += ModPackFiles_PointerPressed;
    }

    public override Task<bool> OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            (DataContext as AddModPackControlModel)!.ReloadF5();

            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public override void Closed()
    {
        WindowManager.AddModPackWindow = null;
    }

    public override void Opened()
    {
        (DataContext as AddModPackControlModel)!.Source = 0;
        (DataContext as AddModPackControlModel)!.Display = true;
    }

    protected override TopModel GenModel(BaseModel model)
    {
        var amodel = new AddModPackControlModel(model);
        amodel.PropertyChanged += Model_PropertyChanged;
        return amodel;
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AddModPackControlModel.DisplayList))
        {
            ScrollViewer1.ScrollToHome();
        }
        else if (e.PropertyName == nameof(AddModPackControlModel.DisplayVersion))
        {
            if ((DataContext as AddModPackControlModel)!.DisplayVersion)
            {
                ThemeManager.CrossFade.Start(null, ModPackFiles);
                ThemeManager.CrossFade.Start(ScrollViewer1, null);
            }
            else
            {
                ThemeManager.CrossFade.Start(ModPackFiles, null);
                ThemeManager.CrossFade.Start(null, ScrollViewer1);
            }
        }
    }

    private async void ModPackFiles_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsXButton1Pressed)
        {
            await (DataContext as AddModPackControlModel)!.Download();
            e.Handled = true;
        }
    }

    public void GoFile(SourceType type, string pid)
    {
        (DataContext as AddModPackControlModel)!.GoFile(type, pid);
    }
}
