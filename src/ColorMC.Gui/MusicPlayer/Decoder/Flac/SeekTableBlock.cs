namespace ColorMC.Gui.MusicPlayer.Decoder.Flac;

public class SeekTableBlock : FlacInfoBlock
{
    public readonly TableItem[] Tables;

    public record TableItem
    {
        /// <summary>
        /// 目标帧中或占位符点的第一个样本的样本编号。0xFFFFFFFFFFFFFFFF
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 从第一个帧标头的第一个字节到目标帧标头的第一个字节的偏移量（以字节为单位）。
        /// </summary>
        public long Offset { get; set; }
        /// <summary>
        /// 目标帧中的样本数。
        /// </summary>
        public int NumberOfSamples { get; set; }
    }

    public SeekTableBlock(FlacStream stream, bool last, BlockType type, int size) : base(last, type, size)
    {
        int numberOfPoints = size / 18; // 每个查找点 18 字节
        Tables = new TableItem[numberOfPoints];

        for (int i = 0; i < numberOfPoints; i++)
        {
            Tables[i] = new TableItem
            {
                Id = stream.ReadInt64BE(),
                Offset = stream.ReadInt64BE(),
                NumberOfSamples = stream.ReadInt16BE()
            };
        }
    }
}
