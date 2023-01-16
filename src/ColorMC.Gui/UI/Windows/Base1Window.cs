using Avalonia.Controls;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Gui.UI.Controls.CurseForge;

namespace ColorMC.Gui.UI.Windows;

public interface IBase1Window
{
    public void SetSelect(CurseForgeControl control);
    public void Install();
}
