using Avalonia.Controls;
using System;

namespace ColorMC.Gui.UI.Windows;

public partial class ErrorWindow : Window
{
    public ErrorWindow()
    {
        InitializeComponent();

        this.Init();
        Icon = App.Icon;
        Border1.MakeResizeDrag(this);

        Closed += HelloWindow_Closed;

        App.PicUpdate += Update;

        Update();
    }

    private void HelloWindow_Closed(object? sender, EventArgs e)
    {
        App.PicUpdate -= Update;
    }

    public void Show(string data, Exception e, bool close)
    {
        Data.Text = $"{data}{Environment.NewLine}{e}";

        if (close)
        {
            Show();
            App.Close();
        }
        else
        {
            Show();
        }
    }

    public void Show(string data, string e, bool close)
    {
        Data.Text = $"{data}{Environment.NewLine}{e}";

        if (close)
        {
            Show();
            App.Close();
        }
        else
        {
            Show();
        }
    }

    public void Update()
    {
        App.Update(this, Image_Back, Border1, Border2);
    }
}
