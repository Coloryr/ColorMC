namespace ColorMC.Core.Objs.Minecraft;

/// <summary>
/// 世界储存
/// </summary>
public record WorldObj
{
    //public byte Raining { get; set; }
    //public long RandomSeed { get; set; }
    //public int SpawnX { get; set; }
    //public int SpawnY { get; set; }
    //public int SpawnZ { get; set; }
    /// <summary>
    /// 上次游玩
    /// </summary>
    public long LastPlayed { get; set; }
    /// <summary>
    /// 游戏类型
    /// </summary>
    public int GameType { get; set; }
    //public byte MapFeatures { get; set; }
    //public int ThunderTime { get; set; }
    //public int Version { get; set; }
    //public int RainTime { get; set; }
    //public long Time { get; set; }
    //public byte Thundering { get; set; }
    /// <summary>
    /// 极限模式
    /// </summary>
    public byte Hardcore { get; set; }
    //public long SizeOnDisk { get; set; }
    /// <summary>
    /// 世界名字
    /// </summary>
    public string LevelName { get; set; }
    /// <summary>
    /// 难度
    /// </summary>
    public byte Difficulty { get; set; }

    /// <summary>
    /// 路径
    /// </summary>
    public string Local { get; set; }
    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; }
    /// <summary>
    /// 是否损坏
    /// </summary>
    public bool Broken { get; set; }

    public GameSettingObj Game;
}
