using System;
using System.IO;
using System.Reflection;

namespace ColorMC.Gui.Player.Decoder.Mp3;

public static class JavaLayerUtils
{
    public static float[] DeserializeArrayResource()
    {
        string local = $"ColorMC.Gui.MusicPlayer.Decoder.Mp3.sfd.temp";

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

