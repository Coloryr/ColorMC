using Avalonia.Controls;
using ColorMC.Gui.UI.Model.Count;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.Count;

public partial class CountControl : UserControl, IUserControl
{
    private readonly CountModel model;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public CountControl()
    {
        InitializeComponent();

        model = new(this);
        DataContext = model;
    }

    public void Closed()
    {
        App.CountWindow = null;
    }

    public void Opened()
    {
        Window.SetTitle(App.GetLanguage("CountWindow.Title"));

        Expander1.MakeTran();
        Expander2.MakeTran();
        Expander3.MakeTran();
    }
}
