using Avalonia.Input;
using Avalonia.Media.Imaging;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.User;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.Gui.UI.Controls.User;

public partial class UsersControl : BaseUserControl
{
    public UsersControl()
    {
        InitializeComponent();

        Title = App.Lang("UserWindow.Title");
        UseName = ToString() ?? "UsersControl";

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);
    }

    public override void Opened()
    {
        Window.SetTitle(Title);
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

    public void AddUrl(string url)
    {
        (DataContext as UsersControlModel)!.AddUrl(url);
    }

    public void Add()
    {
        (DataContext as UsersControlModel)!.SetAdd();
    }

    public override void SetModel(BaseModel model)
    {
        var amodel = new UsersControlModel(model);
        DataContext = amodel;
    }

    public override Bitmap GetIcon()
    {
        return ImageManager.GameIcon;
    }
}
