using System;
using System.IO;
using System.Runtime.Loader;
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

        using var stream = File.OpenRead(local1);
        if (File.Exists(local2))
        {
            using var stream1 = File.OpenRead(local2);

            LoadFromStream(stream, stream1);
        }
        else
        {
            LoadFromStream(stream);
        }

        foreach (var item in Assemblies)
        {
            if (Plugin != null)
            {
                break;
            }
            foreach (var item1 in item.GetTypes())
            {
                if (Plugin != null)
                {
                    break;
                }
                foreach (var item2 in item1.GetInterfaces())
                {
                    if (item2 == typeof(ICustomControl))
                    {
                        Plugin = (Activator.CreateInstance(item1) as ICustomControl)!;
                        Window = Plugin.GetControl();
                        IsLoad = true;
                        break;
                    }
                }
            }
        }
    }
}