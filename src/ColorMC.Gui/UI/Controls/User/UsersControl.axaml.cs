using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.User;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.Utils;
using System;
using System.ComponentModel;

namespace ColorMC.Gui.UI.Controls.User;

public partial class UsersControl : UserControl, IUserControl
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public string Title => App.Lang("UserWindow.Title");

    public UsersControl()
    {
        InitializeComponent();

        DataGrid_User.DoubleTapped += DataGrid_User_DoubleTapped;
        DataGrid_User.CellPointerPressed += DataGrid_User_PointerPressed;

        TextBox_Input1.KeyDown += TextBox_Input1_KeyDown;
        TextBox_Input3.KeyDown += TextBox_Input3_KeyDown;

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "DisplayAdd")
        {
            Dispatcher.UIThread.Post(() =>
            {
                if ((DataContext as UsersControlModel)!.DisplayAdd)
                {
                    App.CrossFade300.Start(null, Grid1);
                }
                else
                {
                    App.CrossFade300.Start(Grid1, null);
                }
            });
        }
    }

    private async void TextBox_Input3_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await (DataContext as UsersControlModel)!.Add();
        }
    }

    private async void TextBox_Input1_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var model = (DataContext as UsersControlModel)!;
            if (model.Type == 0)
            {
                await model.Add();
            }
        }
    }

    public void Opened()
    {
        Window.SetTitle(Title);

        DataGrid_User.SetFontColor();
    }

    public void Closed()
    {
        ColorMCCore.LoginOAuthCode = null;

        App.UserWindow = null;
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

    public void SetBaseModel(BaseModel model)
    {
        var amodel = new UsersControlModel(model);
        amodel.PropertyChanged += Model_PropertyChanged;
        DataContext = amodel;
    }
}
