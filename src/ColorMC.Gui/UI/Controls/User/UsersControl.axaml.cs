using Avalonia.Input;
using Avalonia.Media.Imaging;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.User;

namespace ColorMC.Gui.UI.Controls.User;

/// <summary>
/// 用户列表窗口
/// </summary>
public partial class UsersControl : BaseUserControl
{
    public const string DialogName = "UsersControl";

    public UsersControl() : base(WindowManager.GetUseName<UsersControl>())
    {
        InitializeComponent();

        Title = App.Lang("UserWindow.Title");

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);
    }

    public override void Closed()
    {
        WindowManager.UserWindow = null;
    }

    private void DragEnter(object? sender, DragEventArgs e)
    {
        var model = (DataContext as UsersControlModel)!;
        if (model.LockLogin)
        {
            return;
        }
        if (e.Data.Contains(DataFormats.Text))
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
        var model = (DataContext as UsersControlModel)!;
        if (model.LockLogin)
        {
            return;
        }
        Grid2.IsVisible = false;
        (DataContext as UsersControlModel)!.Drop(e.Data);
    }

    /// <summary>
    /// 添加一个自定义验证服务器
    /// </summary>
    /// <param name="url">服务器地址</param>
    public void AddUrl(string url)
    {
        (DataContext as UsersControlModel)?.AddUrl(url);
    }

    /// <summary>
    /// 添加一个账户
    /// </summary>
    public void Add()
    {
        (DataContext as UsersControlModel)?.SetAdd();
    }

    protected override UsersControlModel GenModel(BaseModel model)
    {
        return new UsersControlModel(model);
    }

    public override Bitmap GetIcon()
    {
        return ImageManager.GameIcon;
    }

    /// <summary>
    /// 重新登陆
    /// </summary>
    public void Relogin()
    {
        (DataContext as UsersControlModel)?.ReLogin();
    }

    /// <summary>
    /// 刷新账户列表
    /// </summary>
    public void LoadUsers()
    {
        (DataContext as UsersControlModel)?.LoadUsers();
    }
}
