using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Add;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.Add;

public partial class AddModPackControl : UserControl, IUserControl, IAddWindow
{
    private AddModPackModel model;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public AddModPackControl()
    {
        InitializeComponent();

        model = new(this);
        model.PropertyChanged += Model_PropertyChanged;
        DataContext = model;

        DataGridFiles.DoubleTapped += DataGridFiles_DoubleTapped;

        Grid1.PointerPressed += Grid1_PointerPressed;

        Input1.KeyDown += Input1_KeyDown;
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "DisplayList")
        {
            Dispatcher.UIThread.Post(() =>
            {
                ListBox_Items.Children.Clear();
                foreach (var item in model.DisplayList)
                {
                    ListBox_Items.Children.Add(new FileItemControl(item));
                }
                ScrollViewer1.ScrollToHome();
            });
        }
        else if (e.PropertyName == "Display")
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (model.Display)
                {
                    App.CrossFade300.Start(null, Grid1, CancellationToken.None);
                }
                else
                {
                    App.CrossFade300.Start(Grid1, null, CancellationToken.None);
                }
            });
        }
    }

    private void Grid1_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var ev = e.GetCurrentPoint(this);
        if (ev.Properties.IsXButton1Pressed)
        {
            model.Download();
            e.Handled = true;
        }
    }

    private void Input1_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            model.Reload();
        }
    }

    private void DataGridFiles_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        model.Download();
    }

    public void Closed()
    {
        App.AddModPackWindow = null;
    }

    public void SetSelect(FileItemControl last)
    {
        model.SetSelect(last);
    }

    public void Opened()
    {
        Window.SetTitle(App.GetLanguage("AddModPackWindow.Title"));

        DataGridFiles.MakeTran();

        model.Source = 0;
    }

    public void Back()
    {
        if (model.Page <= 0)
            return;

        model.Page -= 1;
    }

    public void Next()
    {
        model.Page += 1;
    }

    public void Install()
    {
        model.Install();
    }
}
