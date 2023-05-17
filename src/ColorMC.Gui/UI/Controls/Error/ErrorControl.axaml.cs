using Avalonia.Controls;
using ColorMC.Gui.UI.Model.Error;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System;

namespace ColorMC.Gui.UI.Controls.Error;

public partial class ErrorControl : UserControl, IUserControl
{
    private readonly ErrorModel model;
    public ErrorControl()
    {
        InitializeComponent();
    }

    public ErrorControl(string? data, Exception? e, bool close)
    {
        model = new(this, data, e, close);
        DataContext = model;
    }

    public ErrorControl(string data, string e, bool close)
    {
        model = new(this, data, e, close);
        DataContext = model;
    }

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public void Opened()
    {
        Window.SetTitle(App.GetLanguage("ErrorWindow.Title"));
    }

    public void Closed()
    {
        if (model.Close || (App.IsHide && !BaseBinding.IsGameRuning()))
        {
            App.Close();
        }
    }
}
