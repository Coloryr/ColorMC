using Avalonia.Controls;
using ColorMC.Gui.UI.Model.Error;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System;

namespace ColorMC.Gui.UI.Controls.Error;

public partial class ErrorControl : UserControl, IUserControl
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public UserControl Con => this;

    public string Title => App.GetLanguage("ErrorWindow.Title");

    private readonly ErrorModel model;

    public ErrorControl()
    {
        InitializeComponent();
    }

    public ErrorControl(string? data, Exception? e, bool close) : this()
    {
        model = new(this, data, e, close);
        DataContext = model;
    }

    public ErrorControl(string data, string e, bool close) : this()
    {
        model = new(this, data, e, close);
        DataContext = model;
    }

    public void Opened()
    {
        Window.SetTitle(Title);
    }

    public void Closed()
    {
        if (model.Close || (App.IsHide && !BaseBinding.IsGameRuning()))
        {
            App.Close();
        }
    }
}
