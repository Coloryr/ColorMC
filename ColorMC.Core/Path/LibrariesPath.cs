using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Path;

public static class LibrariesPath
{
    private const string Name = "libraries";

    public static string BaseDir { get; set; }

    public static void InitPath(string dir)
    {
        BaseDir = dir + "/" + Name;

        Directory.CreateDirectory(BaseDir);
    }
}
