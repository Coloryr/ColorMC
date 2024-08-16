using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using ColorMC.Core;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Controls.Custom;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.Utils;

public class DllAssembly : AssemblyLoadContext
{
    public ICustomControl Plugin { get; init; }

    public bool IsLoad { get; private set; }
    public BaseUserControl Window { get; private set; }

    public DllAssembly() : base("ColorMC.Custom", true)
    {
        var local1 = Path.GetFullPath(BaseBinding.GetRunDir() + "ColorMC.CustomGui.dll");
        var local2 = Path.GetFullPath(BaseBinding.GetRunDir() + "ColorMC.CustomGui.pdb");
        if (!File.Exists(local1))
        {
            return;
        }

        Assembly abs;

        using var stream = File.OpenRead(local1);
        if (File.Exists(local2))
        {
            using var stream1 = File.OpenRead(local2);

            abs = LoadFromStream(stream, stream1);
        }
        else
        {
            abs = LoadFromStream(stream);
        }

        foreach (var item1 in abs.GetTypes())
        {
            foreach (var item2 in item1.GetInterfaces())
            {
                if (item2 == typeof(ICustomControl))
                {
                    var plugin = (Activator.CreateInstance(item1) as ICustomControl)!;
                    if (plugin.LauncherApi != ColorMCCore.TopVersion)
                    {
                        Logs.Error("CustomControl API Version error");
                        return;
                    }
                    Plugin = plugin;
                    Window = Plugin.GetControl();
                    IsLoad = true;
                    return;
                }
            }
        }
    }
}