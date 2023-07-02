using Avalonia;
using Avalonia.Platform;
using System;
using System.IO;

namespace ColorMC.Gui.Player.Decoder.Mp3;

public static class JavaLayerUtils
{
    public static float[] DeserializeArrayResource(string name)
    {
        string local = $"resm:ColorMC.Launcher.Mp3.{name}?assembly=ColorMC.Launcher";

        using var asset = AssetLoader.Open(new Uri(local));
        var stream = new MemoryStream();
        asset.CopyTo(stream);
        var values = stream.ToArray();

        var result = new float[values.Length / 4];
        Buffer.BlockCopy(values, 0, result, 0, values.Length);
        return result;
    }
}

