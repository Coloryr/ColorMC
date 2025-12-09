using System;
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
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.Add;

/// <summary>
/// 添加资源窗口
/// </summary>
public partial class AddResourceControl : BaseUserControl
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    private readonly GameSettingObj _obj;

    public AddResourceControl() : base(WindowManager.GetUseName<AddResourceControl>())
    {
        InitializeComponent();
    }

    public AddResourceControl(GameSettingObj obj) : base(WindowManager.GetUseName<AddResourceControl>(obj))
    {
        InitializeComponent();

        _obj = obj;

        Title = string.Format(LangUtils.Get("AddResourceWindow.Title"), obj.Name);

        ItemInfo.PointerPressed += ItemInfo_PointerPressed;
        OptifineDisplay.PointerPressed += OptifineDisplay_PointerPressed;

        EventManager.GameIconChange += EventManager_GameIconChange;
        EventManager.GameNameChange += EventManager_GameNameChange;
        EventManager.GameDelete += EventManager_GameDelete;
    }

    private void EventManager_GameDelete(Guid uuid)
    {
        if (uuid != _obj.UUID)
        {
            return;
        }

        Window?.Close();
    }

    private void EventManager_GameNameChange(Guid uuid)
    {
        if (uuid != _obj.UUID)
        {
            return;
        }

        Title = string.Format(LangUtils.Get("AddResourceWindow.Title"), _obj.Name);
    }

    private void EventManager_GameIconChange(Guid uuid)
    {
        if (uuid != _obj.UUID)
        {
            return;
        }

        ReloadIcon();
    }

    protected override ControlModel GenModel(WindowModel model)
    {
        var amodel = new AddResourceControlModel(model, _obj);
        amodel.PropertyChanged += Model_PropertyChanged;
        return amodel;
    }

    public override Task<bool> OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.F5)
        {
            (DataContext as AddResourceControlModel)?.ReloadF5();

            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public override Bitmap GetIcon()
    {
        return ImageManager.GetGameIcon(_obj) ?? ImageManager.Icon;
    }

    public override void Closed()
    {
        EventManager.GameIconChange -= EventManager_GameIconChange;
        EventManager.GameNameChange -= EventManager_GameNameChange;
        EventManager.GameDelete -= EventManager_GameDelete;

        WindowManager.GameAddWindows.Remove(_obj.UUID);
    }

    public override void Opened()
    {
        (DataContext as AddResourceControlModel)?.Display = true;
    }

    public void GoTo(FileType type)
    {
        (DataContext as AddResourceControlModel)?.GoTo(type);
    }

    public void GoFile(SourceType type, string pid)
    {
        (DataContext as AddResourceControlModel)?.GoFile(type, pid);
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var model = (DataContext as AddResourceControlModel)!;
        if (e.PropertyName == nameof(AddResourceControlModel.OptifineDisplay))
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
        else if (e.PropertyName == nameof(AddBaseModel.DisplayItemInfo))
        {
            if ((DataContext as AddBaseModel)!.DisplayItemInfo)
            {
                ThemeManager.CrossFade.Start(null, ItemInfo);
            }
            else
            {
                ThemeManager.CrossFade.Start(ItemInfo, null);
            }
        }
        else if (e.PropertyName == nameof(AddBaseModel.DisplayList))
        {
            ScrollViewer1.ScrollToHome();
        }
    }

    private void OptifineDisplay_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsXButton1Pressed)
        {
            (DataContext as AddResourceControlModel)!.OptifineDisplay = false;
            e.Handled = true;
        }
    }

    private void ItemInfo_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsXButton1Pressed)
        {
            (DataContext as AddBaseModel)!.DisplayItemInfo = false;
            e.Handled = true;
        }
    }

    /// <summary>
    /// 转到模组升级
    /// </summary>
    /// <param name="list"></param>
    public void GoUpgrade(ICollection<ModUpgradeModel> list)
    {
        (DataContext as AddResourceControlModel)?.Upgrade(list);
    }
}
