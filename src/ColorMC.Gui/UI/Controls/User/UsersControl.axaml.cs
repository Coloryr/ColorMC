using Avalonia.Input;
using Avalonia.Media.Imaging;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.User;

namespace ColorMC.Gui.UI.Controls.User;

/// <summary>
/// �û��б���
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
    /// ���һ���Զ�����֤������
    /// </summary>
    /// <param name="url">��������ַ</param>
    public void AddUrl(string url)
    {
        (DataContext as UsersControlModel)?.AddUrl(url);
    }

    /// <summary>
    /// ���һ���˻�
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
    /// ���µ�½
    /// </summary>
    public void Relogin()
    {
        (DataContext as UsersControlModel)?.ReLogin();
    }

    /// <summary>
    /// ˢ���˻��б�
    /// </summary>
    public void LoadUsers()
    {
        (DataContext as UsersControlModel)?.LoadUsers();
    }
}
