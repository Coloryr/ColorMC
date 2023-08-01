using Avalonia.Controls;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;
using ColorMC.Core.Chunk;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Nbt;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameConfigEdit;

public partial class GameConfigEditModel : BaseModel
{
    private readonly List<string> _items = new();
    private readonly Semaphore _semaphore = new(0, 2);

    public GameSettingObj Obj { get; init; }
    public WorldObj? World { get; init; }
    public bool Cancel { get; set; }

    public ObservableCollection<string> FileList { get; init; } = new();
    public ObservableCollection<NbtDataItem> DataList { get; init; } = new();

    public List<string> TypeSource { get; init; } = new()
    {
        "NbtEnd",
        "NbtByte",
        "NbtShort",
        "NbtInt",
        "NbtLong",
        "NbtFloat",
        "NbtDouble",
        "NbtByteArray",
        "NbtString",
        "NbtList",
        "NbtCompound",
        "NbtIntArray",
        "NbtLongArray",
    };

    private NbtPageViewModel nbtView;

    [ObservableProperty]
    private HierarchicalTreeDataGridSource<NbtNodeModel> source;

    [ObservableProperty]
    private int select = -1;
    [ObservableProperty]
    private string file;
    [ObservableProperty]
    private bool nbtEnable;
    [ObservableProperty]
    private string? name;
    [ObservableProperty]
    private TextDocument text;

    [ObservableProperty]
    private string title;
    [ObservableProperty]
    private string title1;
    [ObservableProperty]
    private bool displayAdd;
    [ObservableProperty]
    private bool displayType;
    [ObservableProperty]
    private string key;
    [ObservableProperty]
    private int type;

    [ObservableProperty]
    private bool displayEdit;
    [ObservableProperty]
    private string dataType;
    [ObservableProperty]
    private NbtDataItem dataItem;

    [ObservableProperty]
    private bool _isWorld;

    [ObservableProperty]
    private bool _displayFindPos;
    [ObservableProperty]
    private bool _displayFindEntity;
    [ObservableProperty]
    private string _posName;
    [ObservableProperty]
    private string _chunk;
    [ObservableProperty]
    private string _chunkFile;
    [ObservableProperty]
    private int _posX;
    [ObservableProperty]
    private int _posY;
    [ObservableProperty]
    private int _posZ;

    public int TurnTo;

    private ChunkData _chunkData;

    public GameConfigEditModel(IUserControl con, GameSettingObj obj, WorldObj? world) : base(con)
    {
        Obj = obj;
        World = world;

        _isWorld = World != null;

        text = new();
    }

    partial void OnPosXChanged(int value)
    {
        PosChange();
    }

    partial void OnPosYChanged(int value)
    {
        PosChange();
    }

    partial void OnPosZChanged(int value)
    {
        PosChange();
    }

    partial void OnNameChanged(string? value)
    {
        Load1();
    }

    async partial void OnFileChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        Progress(App.GetLanguage("ConfigEditWindow.Info7"));
        _chunkData = null;
        var info = new FileInfo(value);
        if (info.Extension is ".dat" or ".dat_old")
        {
            NbtEnable = true;

            NbtBase? nbt;
            if (World != null)
            {
                nbt = await GameBinding.ReadNbt(World, value);
            }
            else
            {
                nbt = await GameBinding.ReadNbt(Obj, value);
            }

            if (nbt is not NbtCompound nbt1)
            {
                return;
            }

            nbtView = new(nbt1, Turn);
            Source = nbtView.Source;
        }
        else if (info.Extension is ".mca")
        {
            NbtEnable = true;

            if (World != null)
            {
                _chunkData = await GameBinding.ReadMca(World, value);
            }
            else
            {
                _chunkData = await GameBinding.ReadMca(Obj, value);
            }

            if (_chunkData.Nbt is not NbtList nbt1)
            {
                return;
            }

            nbtView = new(nbt1, Turn);
            Source = nbtView.Source;
        }
        else
        {
            NbtEnable = false;

            string text;
            if (World != null)
            {
                text = GameBinding.ReadConfigFile(World, value);
            }
            else
            {
                text = GameBinding.ReadConfigFile(Obj, value);
            }

            Text = new(text);
        }
        ProgressClose();
    }

    [RelayCommand]
    public void FindEntity()
    {
        DisplayFindPos = false;
        DisplayFindEntity = true;
       
        PosClear();
        PosChange();
    }

    [RelayCommand]
    public void FindEntityCancel()
    {
        DisplayFindEntity = false;
    }

    [RelayCommand]
    public async Task FindEntityStart()
    {
        var chunkflie = "entities/" + ChunkFile;
        if (FileList.Contains(chunkflie))
        {
            File = chunkflie;
            var (X, Z) = ChunkUtils.PosToChunk(PosX, PosZ);
            await Task.Run(() =>
            {
                while (_chunkData == null)
                {
                    Thread.Sleep(200);
                }
            });
            ChunkNbt? nbt = null;
            foreach (ChunkNbt item in _chunkData.Nbt.Cast<ChunkNbt>())
            {
                if (item.X == X && item.Z == Z)
                {
                    nbt = item;
                    break;
                }
            }
            if (nbt == null)
            {
                Show(string.Format(App.GetLanguage("ConfigEditWindow.Error7"), Chunk));
                return;
            }

            var model = nbtView.Select(nbt);
            if (model != null)
            {
                if (!string.IsNullOrWhiteSpace(PosName))
                {
                    NbtBase? nbt2 = null;
                    if (nbt.TryGet("Entities") is NbtList nbt1 && nbt1.Count > 0)
                    {
                        foreach (NbtCompound item in nbt1.Cast<NbtCompound>())
                        {
                            if (item.TryGet("id") is NbtString id && id.Value.Contains(PosName))
                            {
                                nbt2 = item;
                                break;
                            }
                        }
                    }
                    if (nbt2 != null)
                    {
                        model = nbtView.Find(model, nbt2);
                        if (model != null)
                        {
                            nbtView.Select(model);
                        }
                    }
                    else
                    {
                        Show(string.Format(App.GetLanguage("ConfigEditWindow.Error9"), PosName));
                    }
                }
            }

            DisplayFindEntity = false;
        }
        else
        {
            Show(string.Format(App.GetLanguage("ConfigEditWindow.Error8"), chunkflie));
        }
    }

    [RelayCommand]
    public void FindBlock()
    {
        DisplayFindEntity = false;
        DisplayFindPos = true;

        PosClear();
        PosChange();
    }

    [RelayCommand]
    public void FindBlockCancel()
    {
        DisplayFindPos =  false;
    }

    [RelayCommand]
    public async Task FindBlockStart()
    {
        var chunkflie = "region/" + ChunkFile;
        if (FileList.Contains(chunkflie))
        {
            File = chunkflie;
            var (X, Z) = ChunkUtils.PosToChunk(PosX, PosZ);
            await Task.Run(() =>
            {
                while (_chunkData == null)
                {
                    Thread.Sleep(200);
                }
            });
            ChunkNbt? nbt = null;
            foreach (ChunkNbt item in _chunkData.Nbt.Cast<ChunkNbt>())
            {
                if (item.X == X && item.Z == Z)
                {
                    nbt = item;
                    break;
                }
            }
            if (nbt == null)
            {
                Show(string.Format(App.GetLanguage("ConfigEditWindow.Error4"), Chunk));
                return;
            }

            var model = nbtView.Select(nbt);
            if (model != null)
            {
                if (!string.IsNullOrWhiteSpace(PosName))
                {
                    NbtBase? nbt2 = null;
                    if (nbt.TryGet("block_entities") is NbtList nbt1 && nbt1.Count > 0)
                    {
                        foreach (NbtCompound item in nbt1.Cast<NbtCompound>())
                        {
                            if (item.TryGet("id") is NbtString id && id.Value.Contains(PosName))
                            {
                                nbt2 = item;
                                break;
                            }
                        }
                    }
                    if (nbt2 != null)
                    {
                        model = nbtView.Find(model, nbt2);
                        if (model != null)
                        {
                            nbtView.Select(model);
                        }
                    }
                    else
                    {
                        Show(string.Format(App.GetLanguage("ConfigEditWindow.Error6"), PosName));
                    }
                }
            }

            DisplayFindPos = false;
        }
        else
        {
            Show(string.Format(App.GetLanguage("ConfigEditWindow.Error5"), chunkflie));
        }
    }

    [RelayCommand]
    public void Open()
    {
        var dir = Obj.GetGamePath();
        BaseBinding.OpFile(Path.GetFullPath(dir + "/" + File));
    }

    [RelayCommand]
    public void Save()
    {
        var info = new FileInfo(File);
        if (info.Extension is ".dat" or ".dat_old")
        {
            if (World != null)
            {
                GameBinding.SaveNbtFile(World, File, nbtView.Nbt);
            }
            else
            {
                GameBinding.SaveNbtFile(Obj, File, nbtView.Nbt);
            }
        }
        else if (info.Extension is ".mca")
        {
            if (World != null)
            {
                GameBinding.SaveMcaFile(World, File, _chunkData);
            }
            else
            {
                GameBinding.SaveMcaFile(Obj, File, _chunkData);
            }
        }
        else
        {
            if (World != null)
            {
                GameBinding.SaveConfigFile(World, File, Text?.Text);
            }
            else
            {
                GameBinding.SaveConfigFile(Obj, File, Text?.Text);
            }
        }

        Notify(App.GetLanguage("Gui.Info10"));
    }

    [RelayCommand]
    public void Load()
    {
        if (Obj == null)
            return;

        _items.Clear();
        if (World != null)
        {
            var list = GameBinding.GetAllConfig(World);
            _items.AddRange(list);
        }
        else
        {
            var list = GameBinding.GetAllConfig(Obj);
            _items.AddRange(list);
        }
        Load1();
    }

    [RelayCommand]
    public void AddConfirm()
    {
        Cancel = false;
        _semaphore.Release();
    }

    [RelayCommand]
    public void AddCancel()
    {
        Cancel = true;
        _semaphore.Release();
    }

    [RelayCommand]
    public void DataEditDone()
    {
        _semaphore.Release();
    }

    public void Pressed(Control con)
    {
        var item = Source.Selection;
        if (item != null)
        {
            _ = new ConfigFlyout1(con, item, this);
        }
    }

    public async void AddItem(NbtNodeModel model)
    {
        if (model.NbtType == NbtType.NbtCompound)
        {
            var list = (model.Nbt as NbtCompound)!;
            Key = "";
            Type = 0;
            DisplayType = true;
            Title = App.GetLanguage("ConfigEditWindow.Info2");
            Title1 = App.GetLanguage("ConfigEditWindow.Info3");
            DisplayAdd = true;
            await Task.Run(_semaphore.WaitOne);
            DisplayAdd = false;
            if (Cancel)
                return;
            if (string.IsNullOrWhiteSpace(Key))
            {
                Show(App.GetLanguage("ConfigEditWindow.Error1"));
                return;
            }
            else if (list.HaveKey(Key))
            {
                Show(App.GetLanguage("ConfigEditWindow.Error2"));
                return;
            }

            model.Add(Key, (NbtType)Type);
        }
        else if (model.NbtType == NbtType.NbtList)
        {
            model.Add("", NbtType.NbtEnd);
        }
    }

    public async void Delete(NbtNodeModel model)
    {
        if (model.Top == null)
            return;

        var res = await ShowWait(App.GetLanguage("ConfigEditWindow.Info1"));
        if (!res)
            return;

        model.Top.Remove(model);
    }

    public async void Delete(IReadOnlyList<NbtNodeModel?> list)
    {
        var list1 = new List<NbtNodeModel?>(list);

        var res = await ShowWait(App.GetLanguage("ConfigEditWindow.Info1"));
        if (!res)
            return;

        foreach (var item in list1)
        {
            item?.Top?.Remove(item);
        }
    }

    public async void SetKey(NbtNodeModel model)
    {
        if (model.Top == null)
            return;

        var list = (model.Top.Nbt as NbtCompound)!;
        Key = model.Key!;
        DisplayType = false;
        Title = App.GetLanguage("ConfigEditWindow.Info5");
        Title1 = App.GetLanguage("ConfigEditWindow.Info3");
        DisplayAdd = true;
        await Task.Run(_semaphore.WaitOne);
        DisplayAdd = false;
        if (Cancel)
            return;
        if (string.IsNullOrWhiteSpace(Key))
        {
            Show(App.GetLanguage("ConfigEditWindow.Error1"));
            return;
        }
        else if (Key == model.Key)
        {
            return;
        }
        else if (list.HaveKey(Key))
        {
            Show(App.GetLanguage("ConfigEditWindow.Error2"));
            return;
        }

        model.EditKey(model.Key!, Key);
    }

    public async void SetValue(NbtNodeModel model)
    {
        if (model.NbtType == NbtType.NbtByteArray)
        {
            DataList.Clear();
            DataType = "Byte";
            var list = (model.Nbt as NbtByteArray)!;
            for (int a = 0; a < list.Value.Count; a++)
            {
                DataList.Add(new()
                {
                    Key = a + 1,
                    Value = list.Value[a]
                });
            }
            DataList.Add(new() { Value = (byte)0 });
            DisplayEdit = true;
            await Task.Run(_semaphore.WaitOne);
            DisplayEdit = false;

            list.Value.Clear();
            foreach (var item in DataList)
            {
                if (item.Key == 0)
                    continue;
                if (item.Value is string str)
                {
                    list.Value.Add(byte.Parse(str));
                }
                else
                {
                    list.Value.Add((byte)item.Value);
                }
            }
        }
        else if (model.NbtType == NbtType.NbtIntArray)
        {
            DisplayEdit = true;
            DataList.Clear();
            DataType = "Int";
            var list = (model.Nbt as NbtIntArray)!;
            for (int a = 0; a < list.Value.Count; a++)
            {
                DataList.Add(new()
                {
                    Key = a + 1,
                    Value = list.Value[a]
                });
            }
            DataList.Add(new() { Value = 0 });
            DisplayEdit = true;
            await Task.Run(_semaphore.WaitOne);
            DisplayEdit = false;

            list.Value.Clear();
            foreach (var item in DataList)
            {
                if (item.Key == 0)
                    continue;
                if (item.Value is string str)
                {
                    list.Value.Add(int.Parse(str));
                }
                else
                {
                    list.Value.Add((int)item.Value);
                }
            }
        }
        else if (model.NbtType == NbtType.NbtLongArray)
        {
            DisplayEdit = true;
            DataList.Clear();
            DataType = "Long";
            var list = (model.Nbt as NbtLongArray)!;
            for (int a = 0; a < list.Value.Count; a++)
            {
                DataList.Add(new()
                {
                    Key = a + 1,
                    Value = list.Value[a]
                });
            }
            DataList.Add(new() { Value = (long)0 });
            DisplayEdit = true;
            await Task.Run(_semaphore.WaitOne);
            DisplayEdit = false;

            list.Value.Clear();
            foreach (var item in DataList)
            {
                if (item.Key == 0)
                    continue;
                if (item.Value is string str)
                {
                    list.Value.Add(long.Parse(str));
                }
                else
                {
                    list.Value.Add((long)item.Value);
                }
            }
        }
        else
        {
            Key = model.Nbt.Value.ToString();
            DisplayType = false;
            Title = App.GetLanguage("ConfigEditWindow.Info6");
            Title1 = App.GetLanguage("ConfigEditWindow.Info4");
            DisplayAdd = true;
            await Task.Run(_semaphore.WaitOne);
            DisplayAdd = false;
            if (Cancel)
                return;
            if (string.IsNullOrWhiteSpace(Key))
            {
                Show(App.GetLanguage("ConfigEditWindow.Error1"));
                return;
            }

            try
            {
                model.SetValue(Key);
            }
            catch
            {
                Show(App.GetLanguage("ConfigEditWindow.Error3"));
            }
        }

        model.Update();
    }

    public void DataEdit()
    {
        try
        {
            if (DataType == "Byte")
            {
                DataItem.Value = byte.Parse(DataItem.Value.ToString()!);
            }
            else if (DataType == "Int")
            {
                DataItem.Value = int.Parse(DataItem.Value.ToString()!);
            }
            else if (DataType == "Long")
            {
                DataItem.Value = long.Parse(DataItem.Value.ToString()!);
            }
        }
        catch
        {
            Show(App.GetLanguage("ConfigEditWindow.Error3"));
            DataItem.Value = 0;
            return;
        }

        if (DataItem.Key == 0)
        {
            DataItem.Key = DataList.Count;
            if (DataType == "Byte")
            {
                DataList.Add(new() { Value = (byte)0 });
            }
            else if (DataType == "Int")
            {
                DataList.Add(new() { Value = 0 });
            }
            else if (DataType == "Long")
            {
                DataList.Add(new() { Value = (long)0 });
            }
        }
    }

    public void Flyout(Control con)
    {
        if (DataItem != null)
        {
            _ = new ConfigFlyout2(con, this, DataItem);
        }
    }

    public void DeleteItem(NbtDataItem item)
    {
        if (item.Key == 0)
            return;

        DataList.Remove(item);
        int a = 1;
        foreach (var item1 in DataList)
        {
            item1.Key = a++;
        }
    }

    public async void Find()
    {
        var data = await ShowOne(App.GetLanguage("ConfigEditWindow.Info3"), false);
        if (data.Cancel)
            return;

        if (string.IsNullOrWhiteSpace(data.Text))
            return;

        nbtView.Find(data.Text);
    }

    private void Load1()
    {
        FileList.Clear();
        if (string.IsNullOrWhiteSpace(Name))
        {
            FileList.AddRange(_items);
        }
        else
        {
            var list = from item in _items
                       where item.Contains(Name)
                       select item;
            FileList.AddRange(list);
        }

        if (FileList.Count != 0)
        {
            Select = 0;
        }
        else
        {
            Text.Text = "";
        }
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
        var pos1 = ChunkUtils.PosToChunk(PosX, PosZ);
        Chunk = $"({pos1.X},{pos1.Z})";
        var pos2 = ChunkUtils.ChunkToRegion(pos1.X, pos1.Z);
        ChunkFile = $"r.{pos2.X}.{pos2.Z}.mca";
    }

    private void Turn(int value)
    {
        TurnTo = value;
        OnPropertyChanged("TurnTo");
    }
}
