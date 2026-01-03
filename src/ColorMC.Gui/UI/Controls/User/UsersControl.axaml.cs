using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.User;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.User;

/// <summary>
/// 用户列表窗口
/// </summary>
public partial class UsersControl : BaseUserControl
{
    public UsersControl() : base(WindowManager.GetUseName<UsersControl>())
    {
        InitializeComponent();

        Title = LangUtils.Get("UserWindow.Title");

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);

        GridPanel.SizeChanged += GridPanel_SizeChanged;
    }

    private void GridPanel_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (DataContext is UsersModel model)
        {
            model.GridCount = (int)(GridPanel.Bounds.Width / 200);
        }
    }

    public override void Closed()
    {
        WindowManager.UserWindow = null;
    }

    private void DragEnter(object? sender, DragEventArgs e)
    {
        var model = (DataContext as UsersModel)!;
        if (model.LockLogin)
        {
            return;
        }
        if (e.DataTransfer.Contains(DataFormat.Text))
        {
            Grid2.IsVisible = true;
        }
    }

    private void DragLeave(object? sender, DragEventArgs e)
    {
        Grid2.IsVisible = false;
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        var model = (DataContext as UsersModel)!;
        if (model.LockLogin)
        {
            return;
        }
        Grid2.IsVisible = false;
        (DataContext as UsersModel)!.Drop(e.DataTransfer);
    }

    /// <summary>
    /// 添加一个自定义验证服务器
    /// </summary>
    /// <param name="url">服务器地址</param>
    public void AddUrl(string url)
    {
        (DataContext as UsersModel)?.AddUrl(url);
    }

    /// <summary>
    /// 添加一个账户
    /// </summary>
    public void Add()
    {
        (DataContext as UsersModel)?.SetAdd();
    }

    protected override UsersModel GenModel(WindowModel model)
    {
        return new UsersModel(model);
    }

    /// <summary>
    /// 重新登陆
    /// </summary>
    public void Relogin()
    {
        (DataContext as UsersModel)?.ReLogin();
    }
}
