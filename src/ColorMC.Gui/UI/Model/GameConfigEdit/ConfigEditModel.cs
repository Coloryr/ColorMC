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

public partial class GameConfigEditModel : GameModel
{
    private readonly List<string> _items = new();
    private readonly Semaphore _semaphore = new(0, 2);

    public WorldObj? World { get; init; }
    public bool Cancel { get; set; }

    public ObservableCollection<string> FileList { get; init; } = new();
    public ObservableCollection<NbtDataItem> DataList { get; init; } = new();

    public List<string> TypeSource { get; init; } = LanguageBinding.GetNbtName();

    private NbtPageViewModel _nbtView;

    [ObservableProperty]
    private HierarchicalTreeDataGridSource<NbtNodeModel> _source;

    [ObservableProperty]
    private int _select = -1;
    [ObservableProperty]
    private string _file;
    [ObservableProperty]
    private bool _nbtEnable;
    [ObservableProperty]
    private string? _name;
    [ObservableProperty]
    private TextDocument _text;

    [ObservableProperty]
    private string _title;
    [ObservableProperty]
    private string _title1;
    [ObservableProperty]
    private bool _displayAdd;
    [ObservableProperty]
    private bool _displayType;
    [ObservableProperty]
    private string _key;
    [ObservableProperty]
    private int _type;

    [ObservableProperty]
    private bool _displayEdit;
    [ObservableProperty]
    private string _dataType;
    [ObservableProperty]
    private NbtDataItem _dataItem;
    [ObservableProperty]
    private bool _hexEdit;

    [ObservableProperty]
    private bool _isWorld;

    [ObservableProperty]
    private bool _displayFind;
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

    [ObservableProperty]
    private string _findText1;
    [ObservableProperty]
    private string _findText2;

    private bool _isEntity;

    public int TurnTo;

    private ChunkData? _chunkData;

    public GameConfigEditModel(IUserControl con, GameSettingObj obj, WorldObj? world) : base(con, obj)
    {
        World = world;

        _isWorld = World != null;

        _text = new();
    }

    partial void OnHexEditChanged(bool value)
    {
        foreach (var item in DataList)
        {
            item.ChangeHex(value);
        }
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
        {
            return;
        }

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

            _nbtView = new(nbt1, Turn);
            Source = _nbtView.Source;
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

            _nbtView = new(nbt1, Turn);
            Source = _nbtView.Source;
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
        _isEntity = true;
        FindText1 = App.GetLanguage("ConfigEditWindow.Text6");
        FindText2 = App.GetLanguage("ConfigEditWindow.Text11");
        PosClear();
        PosChange();
        DisplayFind = true;
    }

    [RelayCommand]
    public void FindBlock()
    {
        _isEntity = false;
        FindText1 = App.GetLanguage("ConfigEditWindow.Text5");
        FindText2 = App.GetLanguage("ConfigEditWindow.Text7");
        PosClear();
        PosChange();
        DisplayFind = true;
    }

    [RelayCommand]
    public void FindCancel()
    {
        DisplayFind = false;
    }

    [RelayCommand]
    public async Task FindStart()
    {
        var chunkflie = (_isEntity ? "entities/" : "region/") + ChunkFile;
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
            foreach (ChunkNbt item in _chunkData!.Nbt.Cast<ChunkNbt>())
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

            var model = _nbtView.Select(nbt);
            if (model != null)
            {
                if (!string.IsNullOrWhiteSpace(PosName))
                {
                    NbtBase? nbt2 = null;
                    if (nbt.TryGet(_isEntity ? "Entities" : "block_entities")
                        is NbtList nbt1 && nbt1.Count > 0)
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
                        model = _nbtView.Find(model, nbt2);
                        if (model != null)
                        {
                            _nbtView.Select(model);
                        }
                    }
                    else
                    {
                        Show(string.Format(_isEntity
                            ? App.GetLanguage("ConfigEditWindow.Error8")
                            : App.GetLanguage("ConfigEditWindow.Error6"), PosName));
                    }
                }
            }

            DisplayFind = false;
        }
        else
        {
            Show(string.Format(_isEntity
                 ? App.GetLanguage("ConfigEditWindow.Error7")
                 : App.GetLanguage("ConfigEditWindow.Error5"), chunkflie));
        }
    }

    [RelayCommand]
    public void Open()
    {
        var dir = Obj.GetGamePath();
        PathBinding.OpFile(Path.GetFullPath(dir + "/" + File));
    }

    [RelayCommand]
    public void Save()
    {
        var info = new FileInfo(File);
        if (info.Extension is ".dat" or ".dat_old")
        {
            if (World != null)
            {
                GameBinding.SaveNbtFile(World, File, _nbtView.Nbt);
            }
            else
            {
                GameBinding.SaveNbtFile(Obj, File, _nbtView.Nbt);
            }
        }
        else if (info.Extension is ".mca")
        {
            if (World != null)
            {
                GameBinding.SaveMcaFile(World, File, _chunkData!);
            }
            else
            {
                GameBinding.SaveMcaFile(Obj, File, _chunkData!);
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
            {
                return;
            }
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
        {
            return;
        }

        foreach (var item in list1)
        {
            item?.Top?.Remove(item);
        }
    }

    public async void SetKey(NbtNodeModel model)
    {
        if (model.Top == null)
        {
            return;
        }

        var list = (model.Top.Nbt as NbtCompound)!;
        Key = model.Key!;
        DisplayType = false;
        Title = App.GetLanguage("ConfigEditWindow.Info5");
        Title1 = App.GetLanguage("ConfigEditWindow.Info3");
        DisplayAdd = true;
        await Task.Run(_semaphore.WaitOne);
        DisplayAdd = false;
        if (Cancel)
        {
            return;
        }
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
                DataList.Add(new(a + 1, list.Value[a], HexEdit));
            }
            DataList.Add(new(0, (byte)0, HexEdit));
            DisplayEdit = true;
            await Task.Run(_semaphore.WaitOne);
            DisplayEdit = false;

            list.Value.Clear();
            foreach (var item in DataList)
            {
                if (item.Key == 0)
                {
                    continue;
                }

                list.Value.Add((byte)item.GetValue());
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
                DataList.Add(new(a + 1, list.Value[a], HexEdit));
            }
            DataList.Add(new(0, 0, HexEdit));
            DisplayEdit = true;
            await Task.Run(_semaphore.WaitOne);
            DisplayEdit = false;

            list.Value.Clear();
            foreach (var item in DataList)
            {
                if (item.Key == 0)
                {
                    continue;
                }

                list.Value.Add((int)item.GetValue());
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
                DataList.Add(new(a + 1, list.Value[a], HexEdit));
            }
            DataList.Add(new(0, (long)0, HexEdit));
            DisplayEdit = true;
            await Task.Run(_semaphore.WaitOne);
            DisplayEdit = false;

            list.Value.Clear();
            foreach (var item in DataList)
            {
                if (item.Key == 0)
                {
                    continue;
                }

                list.Value.Add((long)item.GetValue());
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
            {
                return;
            }
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
                DataItem.Value = (byte)DataItem.GetValue();
            }
            else if (DataType == "Int")
            {
                DataItem.Value = (int)DataItem.GetValue();
            }
            else if (DataType == "Long")
            {
                DataItem.Value = (long)DataItem.GetValue();
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
                DataList.Add(new(0, (byte)0, HexEdit));
            }
            else if (DataType == "Int")
            {
                DataList.Add(new(0, 0, HexEdit));
            }
            else if (DataType == "Long")
            {
                DataList.Add(new(0, (long)0, HexEdit));
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
        {
            return;
        }

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
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(data.Text))
        {
            return;
        }

        _nbtView.Find(data.Text);
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
        var (X, Z) = ChunkUtils.PosToChunk(PosX, PosZ);
        Chunk = $"({X},{Z})";
        (X, Z) = ChunkUtils.ChunkToRegion(X, Z);
        ChunkFile = $"r.{X}.{Z}.mca";
    }

    private void Turn(int value)
    {
        TurnTo = value;
        OnPropertyChanged("TurnTo");
    }

    public override void Close()
    {
        _nbtView = null!;
        _source = null!;
    }
}
