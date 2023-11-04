using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Count;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.Gui.UI.Controls.Count;

public partial class CountControl : UserControl, IUserControl
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public string Title => App.Lang("CountWindow.Title");

    public CountControl()
    {
        InitializeComponent();
    }

    public void Closed()
    {
        App.CountWindow = null;
    }

    public void Opened()
    {
        Window.SetTitle(Title);
    }

    public void SetBaseModel(BaseModel model)
    {
        DataContext = new CountModel();
    }
}
