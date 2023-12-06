using Avalonia.Controls;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Setting;
using System.ComponentModel;

namespace ColorMC.Gui.UI.Controls.Setting;

public partial class SettingControl : MenuControl
{
    private readonly Tab1Control _tab1 = new();
    private readonly Tab2Control _tab2 = new();
    private readonly Tab3Control _tab3 = new();
    private readonly Tab4Control _tab4 = new();
    private readonly Tab5Control _tab5 = new();
    private readonly Tab6Control _tab6 = new();
    private readonly Tab7Control _tab7 = new();

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

    public async void GoTo(SettingType type)
    {
        var model = (DataContext as SettingModel)!;
        switch (type)
        {
            case SettingType.SetJava:
                model.NowView = 3;
                break;
            case SettingType.Net:
                model.NowView = 1;
                await model.GameCloudConnect();
                break;
        }
    }

    public override void Opened()
    {
        Window.SetTitle(Title);

        Content1.Child = _tab2;
        (DataContext as SettingModel)!.LoadUISetting();
    }

    private void Amodel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Switch")
        {
            var model = (DataContext as SettingModel)!;
            switch (model.NowView)
            {
                case 0:
                    _tab2.Reset();
                    break;
                case 1:
                    _tab3.Reset();
                    break;
                case 2:
                    _tab4.Reset();
                    break;
                case 4:
                    _tab6.Reset();
                    break;
            }
        }
    }

    protected override MenuModel SetModel(BaseModel model)
    {
        var amodel = new SettingModel(model);
        amodel.PropertyChanged += Amodel_PropertyChanged;
        return amodel;
    }

    protected override Control ViewChange(int old, int index)
    {
        var model = (DataContext as SettingModel)!;
        switch (index)
        {
            case 0:
                model.LoadUISetting();
                return _tab2;
            case 1:
                model.LoadHttpSetting();
                return _tab3;
            case 2:
                model.LoadArg();
                return _tab4;
            case 3:
                model.LoadJava();
                return _tab5;
            case 4:
                model.LoadServer();
                return _tab6;
            case 5:
                return _tab1;
            case 6:
                _tab7.Start();
                return _tab7;
            default:
                throw new InvalidEnumArgumentException();
        }
    }
}
