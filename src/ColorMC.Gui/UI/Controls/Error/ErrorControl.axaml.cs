using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Error;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System;

namespace ColorMC.Gui.UI.Controls.Error;

public partial class ErrorControl : UserControl, IUserControl
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public string Title => App.GetLanguage("ErrorWindow.Title");

    public ErrorControl()
    {
        InitializeComponent();
    }

    public ErrorControl(string? data, Exception? e, bool close) : this()
    {
        DataContext = new ErrorModel(data, e, close);
    }

    public ErrorControl(string data, string e, bool close) : this()
    {
        DataContext = new ErrorModel(data, e, close);
    }

    public void Opened()
    {
        Window.SetTitle(Title);
    }

    public void Closed()
    {
        if ((DataContext as ErrorModel)!.NeedClose 
            || (App.IsHide && !BaseBinding.IsGameRuning()))
        {
            App.Close();
        }
    }

    public void SetBaseModel(BaseModel model)
    {
        
    }
}
