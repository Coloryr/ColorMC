using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;

namespace ColorMC.Gui.UI.Windows;

/// <summary>
/// 带有边框的窗口
/// 用于某些系统
/// </summary>
public partial class BorderControl : UserControl
{
    public BorderControl()
    {
        InitializeComponent();

        if (SystemInfo.Os == OsType.Linux)
        {
            NorthWest.PointerPressed += SizeC;
            North.PointerPressed += SizeC;
            NorthEast.PointerPressed += SizeC;
            West.PointerPressed += SizeC;
            East.PointerPressed += SizeC;
            SouthWest.PointerPressed += SizeC;
            South.PointerPressed += SizeC;
            SouthEast.PointerPressed += SizeC;

            NorthWest.Cursor = new Cursor(StandardCursorType.TopLeftCorner);
            North.Cursor = new Cursor(StandardCursorType.TopSide);
            NorthEast.Cursor = new Cursor(StandardCursorType.TopRightCorner);
            West.Cursor = new Cursor(StandardCursorType.LeftSide);
            East.Cursor = new Cursor(StandardCursorType.RightSide);
            SouthWest.Cursor = new Cursor(StandardCursorType.BottomLeftCorner);
            South.Cursor = new Cursor(StandardCursorType.BottomSide);
            SouthEast.Cursor = new Cursor(StandardCursorType.BottomRightCorner);
        }
        else
        {
            //Win32Properties.SetNonClientHitTestResult(NorthWest, Win32Properties.Win32HitTestValue.TopLeft);
            //Win32Properties.SetNonClientHitTestResult(North, Win32Properties.Win32HitTestValue.Top);
            //Win32Properties.SetNonClientHitTestResult(NorthEast, Win32Properties.Win32HitTestValue.TopRight);
            //Win32Properties.SetNonClientHitTestResult(West, Win32Properties.Win32HitTestValue.Left);
            //Win32Properties.SetNonClientHitTestResult(East, Win32Properties.Win32HitTestValue.Right);
            //Win32Properties.SetNonClientHitTestResult(SouthWest, Win32Properties.Win32HitTestValue.BottomLeft);
            //Win32Properties.SetNonClientHitTestResult(South, Win32Properties.Win32HitTestValue.Bottom);
            //Win32Properties.SetNonClientHitTestResult(SouthEast, Win32Properties.Win32HitTestValue.BottomRight);
        }
    }

    /// <summary>
    /// 选中边框后可以拉伸窗口大小
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
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
