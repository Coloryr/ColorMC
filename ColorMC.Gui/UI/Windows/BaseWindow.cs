using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Windows;

public interface IBaseWindow
{
    public Controls.Info4Control Info { get; }
    public Controls.Info1Control Info1 { get; }
    public Controls.Info2Control Info2 { get; }
    public Controls.Info3Control Info3 { get; }

    public Window Window { get; }

    public void Update();
    public void Next();
}
