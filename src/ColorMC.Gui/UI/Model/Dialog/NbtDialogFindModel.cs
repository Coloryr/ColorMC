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
    /// <summary>
    /// 是否取消
    /// </summary>
    public bool Cancel;

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

    [RelayCommand]
    public void FindCancel()
    {
        Cancel = true;
        DialogHost.Close(_useName);
    }

    [RelayCommand]
    public void FindStart()
    {
        Cancel = false;
        DialogHost.Close(_useName);
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

    private void PosChange()
    {
        var pos = ChunkUtils.PosToChunk(new(PosX ?? 0, PosZ ?? 0));
        Chunk = $"({pos.X},{pos.Y})";
        pos = ChunkUtils.ChunkToRegion(pos);
        ChunkFile = $"r.{pos.X}.{pos.Y}.mca";
    }
}
