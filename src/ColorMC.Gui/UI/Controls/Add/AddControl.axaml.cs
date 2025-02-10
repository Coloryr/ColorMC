using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Add;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddControl : BaseUserControl
{
    private readonly GameSettingObj _obj;

    public AddControl() : base(WindowManager.GetUseName<AddControl>())
    {
        InitializeComponent();
    }

    public AddControl(GameSettingObj obj) : base(WindowManager.GetUseName<AddControl>(obj))
    {
        InitializeComponent();

        _obj = obj;

        Title = string.Format(App.Lang("AddWindow.Title"), obj.Name);

        VersionDisplay.PointerPressed += VersionDisplay_PointerPressed;
        OptifineDisplay.PointerPressed += OptifineDisplay_PointerPressed;
        ModDownloadDisplay.PointerPressed += ModDownloadDisplay_PointerPressed;
    }

    public override TopModel GenModel(BaseModel model)
    {
        var amodel = new AddControlModel(model, _obj);
        amodel.PropertyChanged += Model_PropertyChanged;
        return amodel;
    }

    public override Task<bool> OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            (DataContext as AddControlModel)!.Reload();

            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public override Bitmap GetIcon()
    {
        var icon = ImageManager.GetGameIcon(_obj);
        return icon ?? ImageManager.GameIcon;
    }

    public override void Closed()
    {
        WindowManager.GameAddWindows.Remove(_obj.UUID);
    }

    public override void Opened()
    {
        (DataContext as AddControlModel)!.Display = true;
    }

    public async Task GoSet()
    {
        await (DataContext as AddControlModel)!.GoSet();
    }

    public void GoTo(FileType type)
    {
        (DataContext as AddControlModel)?.GoTo(type);
    }

    public void GoFile(SourceType type, string pid)
    {
        (DataContext as AddControlModel)!.GoFile(type, pid);
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var model = (DataContext as AddControlModel)!;
        if (e.PropertyName == nameof(AddControlModel.OptifineDisplay))
        {
            if (model.OptifineDisplay == true)
            {
                ThemeManager.CrossFade.Start(null, OptifineDisplay);
                ThemeManager.CrossFade.Start(ScrollViewer1, null);
            }
            else
            {
                ThemeManager.CrossFade.Start(OptifineDisplay, null);
                ThemeManager.CrossFade.Start(null, ScrollViewer1);
            }
        }
        else if (e.PropertyName == nameof(AddControlModel.ModDownloadDisplay))
        {
            if (model.ModDownloadDisplay == true)
            {
                ThemeManager.CrossFade.Start(null, ModDownloadDisplay);
                ThemeManager.CrossFade.Start(ScrollViewer1, null);
            }
            else
            {
                ThemeManager.CrossFade.Start(ModDownloadDisplay, null);
                ThemeManager.CrossFade.Start(null, ScrollViewer1);
            }
        }
        else if (e.PropertyName == nameof(AddControlModel.VersionDisplay))
        {
            if (model.VersionDisplay == true)
            {
                ThemeManager.CrossFade.Start(null, VersionDisplay);
                ThemeManager.CrossFade.Start(ScrollViewer1, null);
            }
            else
            {
                ThemeManager.CrossFade.Start(VersionDisplay, null);
                ThemeManager.CrossFade.Start(null, ScrollViewer1);
            }
        }
        else if (e.PropertyName == AddControlModel.NameScrollToHome)
        {
            ScrollViewer1.ScrollToHome();
        }
    }

    private void VersionDisplay_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsXButton1Pressed)
        {
            (DataContext as AddControlModel)!.VersionDisplay = false;
            e.Handled = true;
        }
    }
    private void OptifineDisplay_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsXButton1Pressed)
        {
            (DataContext as AddControlModel)!.OptifineDisplay = false;
            e.Handled = true;
        }
    }
    private void ModDownloadDisplay_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsXButton1Pressed)
        {
            (DataContext as AddControlModel)!.ModDownloadDisplay = false;
            e.Handled = true;
        }
    }

    public void GoUpgrade(ICollection<ModUpgradeModel> list)
    {
        (DataContext as AddControlModel)!.Upgrade(list);
    }

    public void ReloadTitle()
    {
        Title = string.Format(App.Lang("AddWindow.Title"), _obj.Name);
        Window.SetTitle(Title);
    }
}
