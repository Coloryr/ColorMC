namespace ColorMC.Gui.MusicPlayer.Decoder.Flac;

public enum BlockType
{
    STREAMINFO = 0,
    PADDING,
    APPLICATION,
    SEEKTABLE,
    VORBIS_COMMEN,
    CUESHEET,
    PICTURE,
    /// <summary>
    /// 7-126
    /// </summary>
    reserved
}
