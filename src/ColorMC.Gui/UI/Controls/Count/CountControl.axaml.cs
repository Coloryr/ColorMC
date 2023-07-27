using Avalonia.Controls;
using ColorMC.Gui.UI.Model.Count;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.Count;

public partial class CountControl : UserControl, IUserControl
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public UserControl Con => this;

    public string Title => App.GetLanguage("CountWindow.Title");

    private readonly CountModel _model;

    public CountControl()
    {
        InitializeComponent();

        _model = new(this);
        DataContext = _model;
    }

    public void Closed()
    {
        App.CountWindow = null;
    }

    public void Opened()
    {
        Window.SetTitle(Title);

        Expander1.MakeTran();
        Expander2.MakeTran();
        Expander3.MakeTran();
    }
}
