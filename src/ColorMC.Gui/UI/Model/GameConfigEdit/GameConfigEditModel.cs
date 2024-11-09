using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;
using ColorMC.Core.Chunk;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Nbt;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Chunk;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.GameConfigEdit;

public partial class GameConfigEditModel : GameModel
{
    private readonly List<string> _items = [];

    public WorldObj? World { get; init; }

    public ObservableCollection<string> FileList { get; init; } = [];

    public NbtPageModel NbtView;

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
    private bool _isWorld;
    [ObservableProperty]
    private bool _isEdit;

    [ObservableProperty]
    private string _useName;

    private string _lastName;

    public int TurnTo;

    public ChunkDataObj? ChunkData;

    public GameConfigEditModel(BaseModel model, GameSettingObj obj, WorldObj? world)
        : base(model, obj)
    {
        UseName = (ToString() ?? "GameConfigEditModel")
            + " game:" + obj?.UUID + " world:" + world?.LevelName;
        World = world;

        _isWorld = World != null;

        _text = new();
    }

    partial void OnNameChanged(string? value)
    {
        Load1();
    }

    async partial void OnFileChanged(string value)
    {
        if (_lastName == value || string.IsNullOrWhiteSpace(value))
        {
            return;
        }
        if (IsEdit)
        {
            var res = await Model.ShowWait(App.Lang("ConfigEditWindow.Info8"));
            if (!res)
            {
                File = _lastName;
                return;
            }
        }

        _lastName = value;
        Model.Progress(App.Lang("ConfigEditWindow.Info7"));
        ChunkData = null;
        var info = new FileInfo(value);
        if (info.Extension is ".dat" or ".dat_old" or ".rio")
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

            Model.ProgressClose();

            if (nbt is not NbtCompound nbt1)
            {
                Model.Show(App.Lang("ConfigEditWindow.Error9"));
                return;
            }

            NbtView = new(nbt1, Turn);
            Source = NbtView.Source;
        }
        else if (info.Extension is ".mca")
        {
            NbtEnable = true;

            if (World != null)
            {
                ChunkData = await GameBinding.ReadMca(World, value);
            }
            else
            {
                ChunkData = await GameBinding.ReadMca(Obj, value);
            }

            Model.ProgressClose();

            if (ChunkData?.Nbt is not NbtList nbt1)
            {
                Model.Show(App.Lang("ConfigEditWindow.Error10"));
                return;
            }

            NbtView = new(nbt1, Turn);
            Source = NbtView.Source;
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

            Model.ProgressClose();

            Text = new(text);
        }
        IsEdit = false;
        Model.Notify(App.Lang("ConfigEditWindow.Info10"));
    }

    [RelayCommand]
    public async Task FindEntity()
    {
        var model = new NbtDialogFindModel(UseName)
        {
            IsEntity = true,
            FindText1 = App.Lang("ConfigEditWindow.Text6"),
            FindText2 = App.Lang("ConfigEditWindow.Text11")
        };
        await DialogHost.Show(model, UseName);
        if (model.Cancel)
        {
            return;
        }
        FindStart(model);
    }

    [RelayCommand]
    public async Task FindBlock()
    {
        var model = new NbtDialogFindModel(UseName)
        {
            IsEntity = false,
            FindText1 = App.Lang("ConfigEditWindow.Text5"),
            FindText2 = App.Lang("ConfigEditWindow.Text7")
        };
        await DialogHost.Show(model, UseName);
        if (model.Cancel)
        {
            return;
        }
        FindStart(model);
    }

    [RelayCommand]
    public void Open()
    {
        var dir = Obj.GetGamePath();
        PathBinding.OpenFileWithExplorer(Path.GetFullPath(dir + "/" + File));
    }

    [RelayCommand]
    public void Save()
    {
        var info = new FileInfo(File);
        if (info.Extension is ".dat" or ".dat_old")
        {
            if (World != null)
            {
                GameBinding.SaveNbtFile(World, File, NbtView.Nbt);
            }
            else
            {
                GameBinding.SaveNbtFile(Obj, File, NbtView.Nbt);
            }
        }
        else if (info.Extension is ".mca")
        {
            if (World != null)
            {
                GameBinding.SaveMcaFile(World, File, ChunkData!);
            }
            else
            {
                GameBinding.SaveMcaFile(Obj, File, ChunkData!);
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

        Model.Notify(App.Lang("ConfigEditWindow.Info9"));
        IsEdit = false;
    }

    [RelayCommand]
    public void Load()
    {
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
        Model.Notify(App.Lang("ConfigEditWindow.Info11"));
        Load1();
    }

    public async void FindStart(NbtDialogFindModel fmodel)
    {
        var chunkflie = (fmodel.IsEntity ? "entities/" : "region/") + fmodel.ChunkFile;
        if (FileList.Contains(chunkflie))
        {
            if (_lastName != File && IsEdit)
            {
                var res = await Model.ShowWait(App.Lang("ConfigEditWindow.Info8"));
                if (!res)
                {
                    return;
                }
                IsEdit = false;
            }
            File = chunkflie;
            var pos = ChunkUtils.PosToChunk(new(fmodel.PosX ?? 0, fmodel.PosZ ?? 0));
            await Task.Run(() =>
            {
                while (ChunkData == null)
                {
                    Thread.Sleep(200);
                }
            });
            ChunkNbt? nbt = null;
            foreach (ChunkNbt item in ChunkData!.Nbt.Cast<ChunkNbt>())
            {
                if (item.X == pos.X && item.Z == pos.Y)
                {
                    nbt = item;
                    break;
                }
            }
            if (nbt == null)
            {
                Model.Show(string.Format(App.Lang("ConfigEditWindow.Error4"), fmodel.Chunk));
                return;
            }

            var model = NbtView.Select(nbt);
            if (model != null)
            {
                if (!string.IsNullOrWhiteSpace(fmodel.PosName))
                {
                    NbtBase? nbt2 = null;
                    if (nbt.TryGet(fmodel.IsEntity ? "Entities" : "block_entities")
                        is NbtList nbt1 && nbt1.Count > 0)
                    {
                        foreach (NbtCompound item in nbt1.Cast<NbtCompound>())
                        {
                            if (item.TryGet("id") is NbtString id && id.Value.Contains(fmodel.PosName))
                            {
                                nbt2 = item;
                                break;
                            }
                        }
                    }
                    if (nbt2 != null)
                    {
                        model = NbtPageModel.Find(model, nbt2);
                        if (model != null)
                        {
                            NbtView.Select(model);
                        }
                    }
                    else
                    {
                        Model.Show(string.Format(fmodel.IsEntity
                            ? App.Lang("ConfigEditWindow.Error8")
                            : App.Lang("ConfigEditWindow.Error6"), fmodel.PosName));
                    }
                }
            }
        }
        else
        {
            Model.Show(string.Format(fmodel.IsEntity
                 ? App.Lang("ConfigEditWindow.Error7")
                 : App.Lang("ConfigEditWindow.Error5"), chunkflie));
        }
    }

    public async void AddItem(NbtNodeModel model)
    {
        if (model.NbtType == NbtType.NbtCompound)
        {
            var list = (model.Nbt as NbtCompound)!;

            var model1 = new NbtDialogAddModel(UseName)
            {
                Type = 0,
                DisplayType = true,
                Title = App.Lang("ConfigEditWindow.Info2"),
                Title1 = App.Lang("ConfigEditWindow.Info3"),
            };
            await DialogHost.Show(model1, UseName);
            if (model1.Cancel)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(model1.Key))
            {
                Model.Show(App.Lang("ConfigEditWindow.Error1"));
                return;
            }
            else if (list.HaveKey(model1.Key))
            {
                Model.Show(App.Lang("ConfigEditWindow.Error2"));
                return;
            }

            model.Add(model1.Key, model1.Type);
        }
        else if (model.NbtType == NbtType.NbtList)
        {
            model.Add("", NbtType.NbtEnd);
        }
        Model.Notify(App.Lang("onfigEditWindow.Info12"));
        Edit();
    }

    public async void Delete(NbtNodeModel model)
    {
        if (model.Top == null)
            return;

        var res = await Model.ShowWait(App.Lang("ConfigEditWindow.Info1"));
        if (!res)
            return;

        model.Top.Remove(model);

        IsEdit = true;
    }

    public async void Delete(IReadOnlyList<NbtNodeModel?> list)
    {
        var list1 = new List<NbtNodeModel?>(list);

        var res = await Model.ShowWait(App.Lang("ConfigEditWindow.Info1"));
        if (!res)
        {
            return;
        }

        foreach (var item in list1)
        {
            item?.Top?.Remove(item);
        }
        Model.Notify(App.Lang("onfigEditWindow.Info13"));
        Edit();
    }

    public async void SetKey(NbtNodeModel model)
    {
        if (model.Top == null)
        {
            return;
        }

        var list = (model.Top.Nbt as NbtCompound)!;
        var model1 = new NbtDialogAddModel(UseName)
        {
            Key = model.Key!,
            DisplayType = false,
            Title = App.Lang("ConfigEditWindow.Info5"),
            Title1 = App.Lang("ConfigEditWindow.Info3")
        };
        await DialogHost.Show(model1, UseName);
        if (model1.Cancel)
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(model1.Key))
        {
            Model.Show(App.Lang("ConfigEditWindow.Error1"));
            return;
        }
        else if (model1.Key == model.Key)
        {
            return;
        }
        else if (list.HaveKey(model1.Key))
        {
            Model.Show(App.Lang("ConfigEditWindow.Error2"));
            return;
        }

        model.EditKey(model.Key!, model1.Key);
        Model.Notify(App.Lang("onfigEditWindow.Info14"));
        Edit();
    }

    public async void SetValue(NbtNodeModel model)
    {
        if (model.NbtType == NbtType.NbtByteArray)
        {
            var model1 = new NbtDialogEditModel(Model, UseName)
            {
                DataType = "Byte"
            };
            var list = (model.Nbt as NbtByteArray)!;
            for (int a = 0; a < list.Value.Count; a++)
            {
                model1.DataList.Add(new(a + 1, list.Value[a], model1.HexEdit));
            }
            model1.DataList.Add(new(0, (byte)0, model1.HexEdit));
            await DialogHost.Show(model1, UseName);

            list.Value.Clear();
            foreach (var item in model1.DataList)
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
            var model1 = new NbtDialogEditModel(Model, UseName)
            {
                DataType = "Int"
            };
            var list = (model.Nbt as NbtIntArray)!;
            for (int a = 0; a < list.Value.Count; a++)
            {
                model1.DataList.Add(new(a + 1, list.Value[a], model1.HexEdit));
            }
            model1.DataList.Add(new(0, 0, model1.HexEdit));
            await DialogHost.Show(model1, UseName);

            list.Value.Clear();
            foreach (var item in model1.DataList)
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
            var model1 = new NbtDialogEditModel(Model, UseName)
            {
                DataType = "Long"
            };
            var list = (model.Nbt as NbtLongArray)!;
            for (int a = 0; a < list.Value.Count; a++)
            {
                model1.DataList.Add(new(a + 1, list.Value[a], model1.HexEdit));
            }
            model1.DataList.Add(new(0, (long)0, model1.HexEdit));
            await DialogHost.Show(model1, UseName);

            list.Value.Clear();
            foreach (var item in model1.DataList)
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
            var model1 = new NbtDialogAddModel(UseName)
            {
                Key = model.Nbt.Value,
                DisplayType = false,
                Title = App.Lang("ConfigEditWindow.Info6"),
                Title1 = App.Lang("ConfigEditWindow.Info4"),
            };
            await DialogHost.Show(model1, UseName);
            if (model1.Cancel)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(model1.Key))
            {
                Model.Show(App.Lang("ConfigEditWindow.Error1"));
                return;
            }

            try
            {
                model.SetValue(model1.Key);
            }
            catch
            {
                Model.Show(App.Lang("ConfigEditWindow.Error3"));
            }
        }
        Model.Notify(App.Lang("onfigEditWindow.Info14"));
        model.Update();
        Edit();
    }

    public void Edit()
    {
        IsEdit = true;
    }

    public async void Find()
    {
        var data = await Model.ShowInputOne(App.Lang("ConfigEditWindow.Info3"), false);
        if (data.Cancel)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(data.Text))
        {
            return;
        }

        NbtView.Find(data.Text);
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

    private void Turn(int value)
    {
        TurnTo = value;
        OnPropertyChanged("TurnTo");
    }

    public override void Close()
    {

    }
}
