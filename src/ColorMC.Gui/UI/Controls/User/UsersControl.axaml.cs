using Avalonia.Input;
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
        var model = (DataContext as UsersModel)!;
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
        var model = (DataContext as UsersModel)!;
        if (model.LockLogin)
        {
            return;
        }
        Grid2.IsVisible = false;
        (DataContext as UsersModel)!.Drop(e.Data);
    }

    /// <summary>
    /// ���һ���Զ�����֤������
    /// </summary>
    /// <param name="url">��������ַ</param>
    public void AddUrl(string url)
    {
        (DataContext as UsersModel)?.AddUrl(url);
    }

    /// <summary>
    /// ���һ���˻�
    /// </summary>
    public void Add()
    {
        (DataContext as UsersModel)?.SetAdd();
    }

    protected override UsersModel GenModel(BaseModel model)
    {
        return new UsersModel(model);
    }

    /// <summary>
    /// ���µ�½
    /// </summary>
    public void Relogin()
    {
        (DataContext as UsersModel)?.ReLogin();
    }

    /// <summary>
    /// ˢ���˻��б�
    /// </summary>
    public void LoadUsers()
    {
        (DataContext as UsersModel)?.LoadUsers();
    }

    /// <summary>
    /// ����ͷ��
    /// </summary>
    public void ReloadHead()
    {
        (DataContext as UsersModel)?.ReloadHead();
    }
}
