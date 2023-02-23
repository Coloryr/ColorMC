using ColorMC.Gui.UI.Controls.Add;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Windows;

public interface IAddWindow
{
    public void SetSelect(FileItemControl item);
    public void Install();
}
