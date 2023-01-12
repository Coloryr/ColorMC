using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using System.Collections.ObjectModel;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab4Control : UserControl
{
    private readonly ObservableCollection<string> List = new();
    private GameEditWindow Window;
    private GameSettingObj Obj;
    public Tab4Control()
    {
        InitializeComponent();
    }
    public void SetWindow(GameEditWindow window)
    {
        Window = window;
    }

    public void SetGame(GameSettingObj obj)
    {
        Obj = obj;
    }

    public void Update()
    {
        if (Obj == null)
            return;
    }
}
