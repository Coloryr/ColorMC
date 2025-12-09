using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Input;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Add;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.Add;

/// <summary>
/// 下载整合包窗口
/// </summary>
public partial class AddModPackControl : BaseUserControl
{
    public AddModPackControl() : base(WindowManager.GetUseName<AddModPackControl>())
    {
        InitializeComponent();

        ItemInfo.PointerPressed += ItemInfo_PointerPressed;

        Title = LangUtils.Get("AddModPackWindow.Title");
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
        (DataContext as AddModPackControlModel)?.Opened();
    }

    protected override ControlModel GenModel(WindowModel model)
    {
        var amodel = new AddModPackControlModel(model);
        amodel.PropertyChanged += Model_PropertyChanged;
        return amodel;
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AddBaseModel.DisplayList))
        {
            ScrollViewer1.ScrollToHome();
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

    public void GoFile(SourceType type, string pid)
    {
        (DataContext as AddModPackControlModel)?.GoFile(type, pid);
    }

    public void SetGroup(string? group)
    {
        (DataContext as AddModPackControlModel)?.SetGroup(group);
    }
}
