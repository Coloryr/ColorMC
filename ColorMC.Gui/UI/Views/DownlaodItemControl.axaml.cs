using Avalonia.Controls;

namespace ColorMC.Gui.UI.Views;

public partial class DownlaodItemControl : UserControl
{
    public string PName
    {
        get { return ItemName.Text; }
        set { ItemName.Text = value; }
    }

    public long PAll
    {
        set { P1.IsIndeterminate = false; P1.Maximum = value; P3.Text = value.ToString(); }
    }

    public long PNow
    {
        set { P1.Value = value; P2.Text = value.ToString(); }
    }

    public int PError
    {
        set { P4.Text = value.ToString(); }
    }

    public string PState
    {
        set { P5.Text = value; }
    }

    public DownlaodItemControl()
    {
        InitializeComponent();
    }
}
