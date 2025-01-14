using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Setting;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class SettingControl : MenuControl
{
    private Tab1Control _tab1;
    private Tab2Control _tab2;
    private Tab3Control _tab3;
    private Tab4Control _tab4;
    private Tab5Control _tab5;
    private Tab6Control _tab6;
    private Tab7Control _tab7;
    private Tab8Control _tab8;

    private readonly int _needJava;

    public SettingControl()
    {
        Title = App.Lang("SettingWindow.Title");
        UseName = ToString() ?? "SettingControl";
    }

    public SettingControl(int mainversion) : this()
    {
        _needJava = mainversion;
    }

    public override Task<bool> OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (DataContext is SettingModel model)
        {
            return Task.FromResult(model.InputKey(e.KeyModifiers, e.Key));
        }

        return Task.FromResult(false);
    }

    public override void IPointerPressed(PointerPressedEventArgs e)
    {
        if (DataContext is SettingModel model)
        {
            model.InputMouse(e.KeyModifiers, e.GetCurrentPoint(this).Properties);
        }
    }

    public override void Closed()
    {
        WindowManager.SettingWindow = null;
    }

    public void GoTo(SettingType type)
    {
        var model = (DataContext as SettingModel)!;
        switch (type)
        {
            case SettingType.Normal:
                model.NowView = 0;
                break;
            case SettingType.SetJava:
                model.NowView = 3;
                break;
            case SettingType.Net:
                model.NowView = 1;
                break;
            case SettingType.Arg:
                model.NowView = 2;
                break;
        }
    }

    public override void Opened()
    {
        (DataContext as SettingModel)!.LoadUISetting();
    }

    public override TopModel GenModel(BaseModel model)
    {
        return new SettingModel(model);
    }

    protected override Control ViewChange(int old, int index)
    {
        var model = (DataContext as SettingModel)!;
        switch (index)
        {
            case 0:
                model.LoadUISetting();
                _tab2 ??= new();
                return _tab2;
            case 1:
                model.LoadHttpSetting();
                model.TestGameCloudConnect();
                _tab3 ??= new();
                return _tab3;
            case 2:
                model.LoadArg();
                _tab4 ??= new();
                return _tab4;
            case 3:
                model.Load(_needJava);
                return _tab5 ??= new();
            case 4:
                model.LoadServer();
                _tab6 ??= new();
                return _tab6;
            case 5:
                return _tab1 ??= new();
            case 6:
                model.LoadInput();
                return _tab8 ??= new();
            case 7:
                _tab7 ??= new();
                _tab7.Start();
                return _tab7;
            default:
                throw new InvalidEnumArgumentException();
        }
    }

    public override Bitmap GetIcon()
    {
        return ImageManager.GameIcon;
    }
}
