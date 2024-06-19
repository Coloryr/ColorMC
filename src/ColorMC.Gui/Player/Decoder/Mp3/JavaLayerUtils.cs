using System;
using System.IO;
using System.Reflection;
using Avalonia.Platform;

namespace ColorMC.Gui.Player.Decoder.Mp3;

public static class JavaLayerUtils
{
    public static float[] DeserializeArrayResource(string name)
    {
        string local = $"ColorMC.Gui.Player.Decoder.Mp3.{name}";

        var assm = Assembly.GetExecutingAssembly();
        using var istr = assm.GetManifestResourceStream(local)!;
        var stream = new MemoryStream();
        istr.CopyTo(stream);
        var values = stream.ToArray();

        var result = new float[values.Length / 4];
        Buffer.BlockCopy(values, 0, result, 0, values.Length);
        return result;
    }
}

