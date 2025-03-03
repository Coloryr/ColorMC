using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameCloud;

namespace ColorMC.Gui.UI.Controls.GameCloud;

/// <summary>
/// ��Ϸ�ƴ洢
/// </summary>
public partial class GameCloudControl : MenuControl
{
    private Tab1Control _tab1;
    private Tab2Control _tab2;
    private Tab3Control _tab3;

    /// <summary>
    /// ��Ϸʵ��
    /// </summary>
    private readonly GameSettingObj _obj;

    public GameCloudControl(GameSettingObj obj) : base(WindowManager.GetUseName<GameCloudControl>(obj))
    {
        _obj = obj;

        Title = string.Format(App.Lang("GameCloudWindow.Title"), obj.Name);
    }

    protected override async void Opened()
    {
        if (DataContext is GameCloudModel model && await model.Init())
        {
            model.NowView = 0;
        }
    }

    public override void Closed()
    {
        WindowManager.GameCloudWindows.Remove((DataContext as GameCloudModel)!.Obj.UUID);
    }

    protected override TopModel GenModel(BaseModel model)
    {
        return new GameCloudModel(model, _obj);
    }

    public override Bitmap GetIcon()
    {
        return ImageManager.GetGameIcon(_obj) ?? ImageManager.GameIcon;
    }

    protected override Control ViewChange(int old, int index)
    {
        return index switch
        {
            0 => _tab1 ??= new(),
            1 => _tab2 ??= new(),
            2 => _tab3 ??= new(),
            _ => throw new InvalidEnumArgumentException(),
        };
    }

    /// <summary>
    /// ת���浵
    /// </summary>
    public void GoWorld()
    {
        if (DataContext is GameCloudModel model)
        {
            model.NowView = 2;
        }
    }

    /// <summary>
    /// ˢ�´��ڱ���
    /// </summary>
    public void ReloadTitle()
    {
        Title = string.Format(App.Lang("GameCloudWindow.Title"), _obj.Name);
        Window.SetTitle(Title);
    }

    /// <summary>
    /// ����ͼ��
    /// </summary>
    public void ReloadIcon()
    {
        if (DataContext is BaseModel model)
        {
            model.SetIcon(GetIcon());
        }
    }
}
