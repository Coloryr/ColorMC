using System.Collections.Generic;

namespace ColorMC.Gui.MusicPlayer.Decoder.Flac;

public class VorbisCommenBlock : FlacInfoBlock
{
    public string Reference { get; private set; }

    public Dictionary<string, string> Vorbis = [];

    public VorbisCommenBlock(FlacStream stream, bool last, BlockType type, int size) : base(last, type, size)
    {
        Reference = stream.ReadStringLE();

        var size1 = stream.ReadInt32LE();

        for (int a = 0; a < size1; a++)
        {
            var data = stream.ReadStringLE();
            var temp1 = data.Split("=");
            if (temp1.Length < 2)
            {
                Vorbis[temp1[0]] = "";
            }
            else
            {
                Vorbis[temp1[0]] = temp1[1];
            }
        }
    }
}
