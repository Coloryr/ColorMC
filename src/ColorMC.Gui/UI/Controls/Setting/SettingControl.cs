using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Setting;
using System.ComponentModel;

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

    public override string Title => App.Lang("SettingWindow.Title");

    public override string UseName { get; }

    public SettingControl()
    {
        UseName = ToString() ?? "SettingControl";
    }

    public override void Closed()
    {
        App.SettingWindow = null;
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
        }
    }

    public override void Opened()
    {
        Window.SetTitle(Title);

        (DataContext as SettingModel)!.LoadUISetting();
    }

    protected override MenuModel SetModel(BaseModel model)
    {
        return new SettingModel(model);
    }

    protected override Control ViewChange(bool iswhell, int old, int index)
    {
        var model = (DataContext as SettingModel)!;
        switch (index)
        {
            case 0:
                model.LoadUISetting();
                _tab2 ??= new();
                if (iswhell && old == 1)
                {
                    _tab2.End();
                }
                else
                {
                    _tab2.Reset();
                }
                return _tab2;
            case 1:
                model.LoadHttpSetting();
                _ = model.GameCloudConnect();
                _tab3 ??= new();
                if (iswhell && old == 2)
                {
                    _tab3.End();
                }
                else
                {
                    _tab3.Reset();
                }
                return _tab3;
            case 2:
                model.LoadArg();
                _tab4 ??= new();
                if (iswhell && old == 3)
                {
                    _tab4.End();
                }
                else
                {
                    _tab4.Reset();
                }
                return _tab4;
            case 3:
                model.LoadJava();
                return _tab5 ??= new();
            case 4:
                model.LoadServer();
                _tab6 ??= new();
                if (iswhell && old == 5)
                {
                    _tab6.End();
                }
                else
                {
                    _tab6.Reset();
                }
                return _tab6;
            case 5:
                return _tab1 ??= new();
            case 6:
                _tab7.Start();
                return _tab7 ??= new();
            default:
                throw new InvalidEnumArgumentException();
        }
    }
}
