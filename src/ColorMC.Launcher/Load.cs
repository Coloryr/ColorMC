using ColorMC.Gui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Launcher;

internal static class TestLoad
{
    public static void Load1()
    {
        Program.MainCall = ColorMCGui.Main;
        Program.BuildApp = ColorMCGui.BuildAvaloniaApp;
        Program.SetBaseSha1 = ColorMCGui.SetBaseSha1;
    }
}
