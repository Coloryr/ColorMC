using ColorMC.Core.Chunk;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Dialog;

/// <summary>
/// Nbt查找
/// </summary>
public partial class NbtDialogFindModel : ObservableObject
{
    /// <summary>
    /// 查找名字
    /// </summary>
    [ObservableProperty]
    private string _posName;
    /// <summary>
    /// 区块
    /// </summary>
    [ObservableProperty]
    private string _chunk;
    /// <summary>
    /// 区块文件
    /// </summary>
    [ObservableProperty]
    private string _chunkFile;
    /// <summary>
    /// 方块X坐标
    /// </summary>
    [ObservableProperty]
    private int? _posX = 0;
    /// <summary>
    /// 方块Y坐标
    /// </summary>
    [ObservableProperty]
    private int? _posY = 0;
    /// <summary>
    /// 方块Z坐标
    /// </summary>
    [ObservableProperty]
    private int? _posZ = 0;
    /// <summary>
    /// 显示文本1
    /// </summary>
    [ObservableProperty]
    private string _findText1;
    /// <summary>
    /// 显示文本2
    /// </summary>
    [ObservableProperty]
    private string _findText2;

    /// <summary>
    /// 是否为实体
    /// </summary>
    public bool IsEntity;

    private readonly string _useName;

    public NbtDialogFindModel(string usename)
    {
        _useName = usename;

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
    /// 取消
    /// </summary>
    [RelayCommand]
    public void FindCancel()
    {
        DialogHost.Close(_useName, false);
    }
    /// <summary>
    /// 同意
    /// </summary>
    [RelayCommand]
    public void FindStart()
    {
        DialogHost.Close(_useName, true);
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
