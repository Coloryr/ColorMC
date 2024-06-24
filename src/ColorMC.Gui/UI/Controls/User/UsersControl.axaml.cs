using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Flyouts;
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

        DataGrid_User.DoubleTapped += DataGrid_User_DoubleTapped;
        DataGrid_User.CellPointerPressed += DataGrid_User_PointerPressed;

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
        Grid2.IsVisible = false;
        (DataContext as UsersControlModel)!.Drop(e.Data);
    }

    private void DataGrid_User_PointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var model = (DataContext as UsersControlModel)!;
            if (model.Item == null)
            {
                return;
            }

            var pro = e.PointerPressedEventArgs.GetCurrentPoint(this);

            if (pro.Properties.IsRightButtonPressed)
            {
                Flyout((sender as Control)!);
            }
            else if (e.Column.DisplayIndex == 0 && pro.Properties.IsLeftButtonPressed)
            {
                model.Select(model.Item);
            }
            else
            {
                LongPressed.Pressed(() => Flyout((sender as Control)!));
            }
        });
    }

    private void Flyout(Control control)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var model = (DataContext as UsersControlModel)!;
            _ = new UserFlyout(control, model);
        });
    }

    private void DataGrid_User_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        (DataContext as UsersControlModel)!.Select();
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

    private void TextBox_KeyDown(object? sender, KeyEventArgs e)
    {

    }
}
