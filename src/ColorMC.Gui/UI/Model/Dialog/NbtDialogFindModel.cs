using ColorMC.Core.Chunk;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Dialog;

public partial class NbtDialogFindModel : ObservableObject
{
    [ObservableProperty]
    private string _posName;
    [ObservableProperty]
    private string _chunk;
    [ObservableProperty]
    private string _chunkFile;
    [ObservableProperty]
    private int? _posX = 0;
    [ObservableProperty]
    private int? _posY = 0;
    [ObservableProperty]
    private int? _posZ = 0;

    [ObservableProperty]
    private string _findText1;
    [ObservableProperty]
    private string _findText2;

    public bool IsEntity;
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

    private void PosClear()
    {
        PosName = "";
        PosX = 0;
        PosY = 0;
        PosZ = 0;
    }

    private void PosChange()
    {
        var (X, Z) = ChunkUtils.PosToChunk(PosX ?? 0, PosZ ?? 0);
        Chunk = $"({X},{Z})";
        (X, Z) = ChunkUtils.ChunkToRegion(X, Z);
        ChunkFile = $"r.{X}.{Z}.mca";
    }
}
