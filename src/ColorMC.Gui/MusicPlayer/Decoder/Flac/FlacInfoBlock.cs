namespace ColorMC.Gui.MusicPlayer.Decoder.Flac;

public abstract class FlacInfoBlock(bool last, BlockType type, int size)
{
    public bool IsLast { get; private set; } = last;
    public BlockType Type { get; private set; } = type;
    public int Size { get; private set; } = size;
}
