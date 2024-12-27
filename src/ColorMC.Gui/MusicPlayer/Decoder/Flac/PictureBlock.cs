namespace ColorMC.Gui.MusicPlayer.Decoder.Flac;

public class PictureBlock : FlacInfoBlock
{
    public PictureType PictureType { get; private set; }
    public string PictureMedia { get; private set; }
    public string PictureDescription { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Bits { get; private set; }
    public int Index { get; private set; }
    public byte[] Picture { get; private set; }

    public PictureBlock(FlacStream stream, bool last, BlockType type, int size) : base(last, type, size)
    {
        PictureType = (PictureType)stream.ReadInt32BE();
        PictureMedia = stream.ReadStringBE();
        PictureDescription = stream.ReadStringBE();
        Width = stream.ReadInt32BE();
        Height = stream.ReadInt32BE();
        Bits = stream.ReadInt32BE();
        Index = stream.ReadInt32BE();

        var size1 = stream.ReadInt32BE();
        Picture = new byte[size1];
        stream.ReadExactly(Picture);
    }
}
