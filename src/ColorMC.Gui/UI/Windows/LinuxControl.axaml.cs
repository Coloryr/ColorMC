using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;

namespace ColorMC.Gui.UI.Windows;

public partial class LinuxControl : UserControl
{
    public LinuxControl()
    {
        InitializeComponent();

        NorthWest.PointerPressed += SizeC;
        North.PointerPressed += SizeC;
        NorthEast.PointerPressed += SizeC;
        West.PointerPressed += SizeC;
        East.PointerPressed += SizeC;
        SouthWest.PointerPressed += SizeC;
        South.PointerPressed += SizeC;
        SouthEast.PointerPressed += SizeC;
    }

    private void SizeC(object? sender, PointerPressedEventArgs e)
    {
        if (TopLevel.GetTopLevel(this) is Window window
        && sender is Rectangle rectangle)
        {
            if (rectangle.Name == "NorthWest")
            { 
                window.BeginResizeDrag(WindowEdge.NorthWest, e); 
            }
            else if (rectangle.Name == "North")
            { 
                window.BeginResizeDrag(WindowEdge.North, e); 
            }
            else if (rectangle.Name == "NorthEast")
            { 
                window.BeginResizeDrag(WindowEdge.NorthEast, e); 
            }
            else if (rectangle.Name == "West")
            { 
                window.BeginResizeDrag(WindowEdge.West, e); 
            }
            else if (rectangle.Name == "East")
            { 
                window.BeginResizeDrag(WindowEdge.East, e); 
            }
            else if (rectangle.Name == "SouthWest")
            { 
                window.BeginResizeDrag(WindowEdge.SouthWest, e); 
            }
            else if (rectangle.Name == "South")
            { 
                window.BeginResizeDrag(WindowEdge.South, e); 
            }
            else if (rectangle.Name == "SouthEast")
            { 
                window.BeginResizeDrag(WindowEdge.SouthEast, e); 
            }
        }
    }
}
