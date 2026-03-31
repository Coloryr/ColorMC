using ColorMC.Core.Chunk;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// Nbt查找
/// </summary>
public partial class NbtDialogFindModel : BaseDialogModel
{
    /// <summary>
    /// 查找名字
    /// </summary>
    [ObservableProperty]
    public partial string PosName { get; set; }

    /// <summary>
    /// 区块
    /// </summary>
    [ObservableProperty]
    public partial string Chunk { get; set; }

    /// <summary>
    /// 区块文件
    /// </summary>
    [ObservableProperty]
    public partial string ChunkFile { get; set; }

    /// <summary>
    /// 方块X坐标
    /// </summary>
    [ObservableProperty]
    public partial int? PosX { get; set; } = 0;

    /// <summary>
    /// 方块Y坐标
    /// </summary>
    [ObservableProperty]
    public partial int? PosY { get; set; } = 0;

    /// <summary>
    /// 方块Z坐标
    /// </summary>
    [ObservableProperty]
    public partial int? PosZ { get; set; } = 0;

    /// <summary>
    /// 显示文本1
    /// </summary>
    [ObservableProperty]
    public partial string FindText1 { get; set; }

    /// <summary>
    /// 显示文本2
    /// </summary>
    [ObservableProperty]
    public partial string FindText2 { get; set; }

    /// <summary>
    /// 是否为实体
    /// </summary>
    public bool IsEntity;

    public NbtDialogFindModel(string name) : base(name)
    {
        PosClear();
        PosChange();
    }

    partial void OnPosXChanged(int? value)
    {
        PosChange();
    }

    partial void OnPosYChanged(int? value)
    {
        PosChange();
    }

    partial void OnPosZChanged(int? value)
    {
        PosChange();
    }

    /// <summary>
    /// 清理坐标
    /// </summary>
    private void PosClear()
    {
        PosName = "";
        PosX = 0;
        PosY = 0;
        PosZ = 0;
    }

    /// <summary>
    /// 坐标修改，更改显示
    /// </summary>
    private void PosChange()
    {
        var pos = ChunkUtils.PosToChunk(new(PosX ?? 0, PosZ ?? 0));
        Chunk = $"({pos.X},{pos.Y})";
        pos = ChunkUtils.ChunkToRegion(pos);
        ChunkFile = $"r.{pos.X}.{pos.Y}.mca";
    }
}
