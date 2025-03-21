using Avalonia.Controls;
using AvaloniaEdit.Document;
using AvaloniaEdit.Utils;
using ColorMC.Core;
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameConfigEdit;

/// <summary>
/// 配置文件编辑
/// </summary>
public partial class GameConfigEditModel : GameModel
{
    /// <summary>
    /// 配置文本列表
    /// </summary>
    private readonly List<string> _items = [];

    /// <summary>
    /// 存档
    /// </summary>
    public WorldObj? World { get; init; }

    /// <summary>
    /// 显示的配置文件
    /// </summary>
    public ObservableCollection<string> FileList { get; init; } = [];

    /// <summary>
    /// NBT编辑器
    /// </summary>
    public NbtPageModel NbtView;

    /// <summary>
    /// NBT列表
    /// </summary>
    [ObservableProperty]
    private HierarchicalTreeDataGridSource<NbtNodeModel> _source;

    /// <summary>
    /// 选择的文件
    /// </summary>
    [ObservableProperty]
    private int _select = -1;
    /// <summary>
    /// 选择的文件
    /// </summary>
    [ObservableProperty]
    private string _file;
    /// <summary>
    /// 是否是NBT
    /// </summary>
    [ObservableProperty]
    private bool _nbtEnable;
    /// <summary>
    /// 筛选内容
    /// </summary>
    [ObservableProperty]
    private string? _name;
    /// <summary>
    /// 文本编辑器
    /// </summary>
    [ObservableProperty]
    private TextDocument _text;

    /// <summary>
    /// 是否为存档
    /// </summary>
    [ObservableProperty]
    private bool _isWorld;
    /// <summary>
    /// 文件是否被修改了
    /// </summary>
    [ObservableProperty]
    private bool _isEdit;

    /// <summary>
    /// 上一个选中的文件
    /// </summary>
    private string _lastName;

    /// <summary>
    /// 窗口Id
    /// </summary>
    public string UseName { get; private set; }
    /// <summary>
    /// 跳转到第几行
    /// </summary>
    public int TurnTo { get; private set; }

    /// <summary>
    /// 区块数据
    /// </summary>
    public ChunkDataObj? ChunkData;

    public GameConfigEditModel(BaseModel model, GameSettingObj obj, WorldObj? world)
        : base(model, obj)
    {
        UseName = (ToString() ?? "GameConfigEditModel") + ":"
            + obj?.UUID + ":" + world?.LevelName;
        World = world;

        _isWorld = World != null;

        _text = new();
    }

    /// <summary>
    /// 名字修改
    /// </summary>
    /// <param name="value"></param>
    partial void OnNameChanged(string? value)
    {
        Load1();
    }
    /// <summary>
    /// 文件名修改
    /// </summary>
    /// <param name="value"></param>
    async partial void OnFileChanged(string value)
    {
        if (_lastName == value || string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (IsEdit)
        {
            var res = await Model.ShowAsync(App.Lang("ConfigEditWindow.Info8"));
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
        //是不是NBT文件
        if (info.Extension is Names.NameDatExt or Names.
            NameDatOldExt or Names.NameRioExt)
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
        //是不是区块文件
        else if (info.Extension is Names.NameMcaExt)
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
        //其他文件
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

    /// <summary>
    /// 查找实体
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task FindEntity()
    {
        var model = new NbtDialogFindModel(UseName)
        {
            IsEntity = true,
            FindText1 = App.Lang("ConfigEditWindow.Text6"),
            FindText2 = App.Lang("ConfigEditWindow.Text11")
        };
        var res = await DialogHost.Show(model, UseName);
        if (res is not true)
        {
            return;
        }
        FindStart(model);
    }

    /// <summary>
    /// 查找方块
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task FindBlock()
    {
        var model = new NbtDialogFindModel(UseName)
        {
            IsEntity = false,
            FindText1 = App.Lang("ConfigEditWindow.Text5"),
            FindText2 = App.Lang("ConfigEditWindow.Text7")
        };
        var res = await DialogHost.Show(model, UseName);
        if (res is not true)
        {
            return;
        }
        FindStart(model);
    }

    /// <summary>
    /// 打开路径
    /// </summary>
    [RelayCommand]
    public void Open()
    {
        var dir = Obj.GetGamePath();
        PathBinding.OpenFileWithExplorer(Path.GetFullPath(dir + "/" + File));
    }

    /// <summary>
    /// 保存文件
    /// </summary>
    [RelayCommand]
    public void Save()
    {
        var info = new FileInfo(File);
        if (info.Extension is Names.NameDatExt or Names.
            NameDatOldExt or Names.NameRioExt)
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
        else if (info.Extension is Names.NameMcaExt)
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

    /// <summary>
    /// 加载文件列表
    /// </summary>
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

    /// <summary>
    /// 开始查找
    /// </summary>
    /// <param name="fmodel"></param>
    public async void FindStart(NbtDialogFindModel fmodel)
    {
        var chunkflie = (fmodel.IsEntity ? "entities/" : "region/") + fmodel.ChunkFile;
        if (FileList.Contains(chunkflie))
        {
            if (_lastName != File && IsEdit)
            {
                var res = await Model.ShowAsync(App.Lang("ConfigEditWindow.Info8"));
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
            //区块位置
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
                //找到实体或者方块
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

    /// <summary>
    /// 添加一个Nbt标签
    /// </summary>
    /// <param name="model"></param>
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
            var res = await DialogHost.Show(model1, UseName);
            if (res is not true)
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

    /// <summary>
    /// 删除一个Nbt标签
    /// </summary>
    /// <param name="model"></param>
    public async void Delete(NbtNodeModel model)
    {
        if (model.Parent == null)
            return;

        var res = await Model.ShowAsync(App.Lang("ConfigEditWindow.Info1"));
        if (!res)
            return;

        model.Parent.Remove(model);

        IsEdit = true;
    }

    /// <summary>
    /// 删除一些Nbt标签
    /// </summary>
    /// <param name="list"></param>
    public async void Delete(IReadOnlyList<NbtNodeModel?> list)
    {
        var list1 = new List<NbtNodeModel?>(list);

        var res = await Model.ShowAsync(App.Lang("ConfigEditWindow.Info1"));
        if (!res)
        {
            return;
        }

        foreach (var item in list1)
        {
            item?.Parent?.Remove(item);
        }
        Model.Notify(App.Lang("onfigEditWindow.Info13"));
        Edit();
    }

    /// <summary>
    /// 设置Nbt标签键
    /// </summary>
    /// <param name="model"></param>
    public async void SetKey(NbtNodeModel model)
    {
        if (model.Parent == null)
        {
            return;
        }

        var list = (model.Parent.Nbt as NbtCompound)!;
        var model1 = new NbtDialogAddModel(UseName)
        {
            Key = model.Key!,
            DisplayType = false,
            Title = App.Lang("ConfigEditWindow.Info5"),
            Title1 = App.Lang("ConfigEditWindow.Info3")
        };
        var res = await DialogHost.Show(model1, UseName);
        if (res is not true)
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

    /// <summary>
    /// 设置Nbt标签值
    /// </summary>
    /// <param name="model"></param>
    public async void SetValue(NbtNodeModel model)
    {
        //根据类型设置值
        if (model.NbtType == NbtType.NbtByteArray)
        {
            var model1 = new NbtDialogEditModel(Model, UseName)
            {
                DataType = GuiNames.NameTypeByte
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
                DataType = GuiNames.NameTypeInt
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
                DataType = GuiNames.NameTypeLong
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
            var res = await DialogHost.Show(model1, UseName);
            if (res is not true)
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

    /// <summary>
    /// 文件编辑
    /// </summary>
    public void Edit()
    {
        IsEdit = true;
    }

    /// <summary>
    /// 打开Nbt搜索
    /// </summary>
    public async void Find()
    {
        var res = await Model.InputWithEditAsync(App.Lang("ConfigEditWindow.Info3"), false);
        if (res.Cancel)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(res.Text1))
        {
            return;
        }

        NbtView.Find(res.Text1);
    }

    /// <summary>
    /// 筛选配置文件
    /// </summary>
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

    /// <summary>
    /// 转到第几行
    /// </summary>
    /// <param name="value"></param>
    private void Turn(int value)
    {
        TurnTo = value;
        OnPropertyChanged(nameof(TurnTo));
    }

    public override void Close()
    {

    }
}
