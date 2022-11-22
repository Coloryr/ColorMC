using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Objs.Game;

public record WorldObj
{
    public byte Raining { get; set; }
    public long RandomSeed { get; set; }
    public int SpawnX { get; set; }
    public int SpawnY { get; set; }
    public int SpawnZ { get; set; }
    public long LastPlayed { get; set; }
    public int GameType { get; set; }
    public byte MapFeatures { get; set; }
    public int ThunderTime { get; set; }
    public int Version { get; set; }
    public int RainTime { get; set; }
    public long Time { get; set; }
    public byte Thundering { get; set; }
    public byte Hardcore { get; set; }
    public long SizeOnDisk { get; set; }
    public string LevelName { get; set; }

    public string Local { get; set; }
    public byte[] Icon { get; set; }
    public GameSettingObj Game { get; set; }
}
