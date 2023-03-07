using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Windows;

public interface IUserControl
{
    public UserControl Con { get; }
    public void Opened();
    public void Closed();
    public void Update();
    public void Closing();
}
