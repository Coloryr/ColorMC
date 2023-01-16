using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using System;

namespace ColorMC.Gui.UI.Controls.GameEdit;

public partial class Tab7Control : UserControl
{
    private GameEditWindow Window;
    private GameSettingObj Obj;
    public Tab7Control()
    {
        InitializeComponent();
    }


    public void Log(string data) 
    {
        TextBox1.Text += data + Environment.NewLine;
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


