using Avalonia;
using Avalonia.Controls;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.Gui.UI.Controls.Server;

public partial class Tab1Control : UserControl
{
    private bool load = false;
    private ServerPackObj Obj1;

    public Tab1Control()
    {
        InitializeComponent();

        TextBox1.PropertyChanged += TextBox1_PropertyChanged;
    }

    private void TextBox1_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (load)
            return;

        if (e.Property.Name == "Text")
        {

        }
    }

    public void Load()
    {
        load = true;
        TextBox1.Text = Obj1.Url;
        TextBox2.Text = Obj1.Version;
        TextBox3.Text = Obj1.Text;
        TextBox4.Text = Obj1.UI;
        load = false;
    }

    public void SetObj(ServerPackObj obj1)
    {
        Obj1 = obj1;
    }

    private void Save()
    {
        Obj1.Url = TextBox1.Text;
        Obj1.Version = TextBox2.Text;
        Obj1.Text = TextBox3.Text;
        Obj1.UI = TextBox4.Text;

        var window = (VisualRoot as IBaseWindow)!;

        (window.Con as ServerPackControl)?.Save();
    }
}
