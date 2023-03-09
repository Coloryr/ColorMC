using Avalonia;
using Avalonia.Controls;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.UI.Windows;
using Avalonia.Interactivity;

namespace ColorMC.Gui.UI.Controls.Server;

public partial class Tab1Control : UserControl
{
    private bool load = false;
    private ServerPackObj Obj1;

    public Tab1Control()
    {
        InitializeComponent();

        TextBox1.PropertyChanged += TextBox1_PropertyChanged;
        TextBox2.PropertyChanged += TextBox1_PropertyChanged;
        TextBox3.PropertyChanged += TextBox1_PropertyChanged;
        TextBox4.PropertyChanged += TextBox1_PropertyChanged;

        Button1.Click += Button1_Click;
        Button2.Click += Button2_Click;
    }

    private void Button2_Click(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void Button1_Click(object? sender, RoutedEventArgs e)
    {
        throw new System.NotImplementedException();
    }

    private void TextBox1_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (load)
            return;

        if (e.Property.Name == "Text")
        {
            Save();
        }
    }

    public void Load()
    {
        if (Obj1 == null)
            return;

        load = true;
        TextBox1.Text = Obj1.Url;
        TextBox2.Text = Obj1.Version;
        TextBox3.Text = Obj1.Text;
        TextBox4.Text = Obj1.UI;
        CheckBox1.IsChecked = Obj1.LockMod;
        CheckBox2.IsChecked = Obj1.ForceUpdate;
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
        Obj1.LockMod = CheckBox1.IsChecked == true;
        Obj1.ForceUpdate = CheckBox2.IsChecked == true;

        var window = App.FindRoot(this);
        (window.Con as ServerPackControl)?.Save();
    }
}
