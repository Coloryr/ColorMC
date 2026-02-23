using System.Collections.ObjectModel;
using Avalonia.Input;
using ColorMC.Core.Game;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.GameEdit;

/// <summary>
/// 游戏实例编辑
/// </summary>
public partial class GameEditModel
{
    /// <summary>
    /// 结构文件列表
    /// </summary>
    public ObservableCollection<SchematicDisplayModel> SchematicList { get; set; } = [];

    /// <summary>
    /// 选中的结构文件
    /// </summary>
    [ObservableProperty]
    private SchematicDisplayModel? _schematicItem;

    /// <summary>
    /// 是否没有结构文件
    /// </summary>
    [ObservableProperty]
    private bool _schematicEmptyDisplay;

    /// <summary>
    /// 打开结构文件文件夹
    /// </summary>
    [RelayCommand]
    public void OpenSchematic()
    {
        PathBinding.OpenPath(_obj, PathType.SchematicsPath);
    }

    /// <summary>
    /// 加载结构文件列表
    /// </summary>
    public async void LoadSchematic()
    {
        var dialog = Window.ShowProgress(LangUtils.Get("GameEditWindow.Tab12.Text9"));
        SchematicList.Clear();
        foreach (var item in await _obj.GetSchematicsAsync())
        {
            if (item.Broken)
            {
                item.Name = LangUtils.Get("GameEditWindow.Tab12.Text10");
            }

            SchematicList.Add(new SchematicDisplayModel() { Obj = item });
        }
        Window.CloseDialog(dialog);
        SchematicEmptyDisplay = SchematicList.Count == 0;
        Window.Notify(LangUtils.Get("GameEditWindow.Tab12.Text7"));
    }

    /// <summary>
    /// 显示方块列表
    /// </summary>
    public async void DisplayBlocks()
    {
        if (SchematicItem is not { } sche || SchematicItem.Obj.BlockCount <= 0)
        {
            return;
        }

        var dialog = Window.ShowProgress(LangUtils.Get("LuckBlockWindow.Text5"));
        var res = await BlockListUtils.StartLoadBlock();
        Window.CloseDialog(dialog);
        if (!res.State)
        {
            await Window.ShowWait(res.Data!);
            return;
        }

        var dialog1 = new BlockListModel(Window.WindowId)
        {
            Text = string.Format(LangUtils.Get("GameEditWindow.Tab12.Text17"), sche.Name, sche.Type)
        };

        foreach (var item in sche.Obj.Blocks)
        {
            if (BlockListUtils.Blocks.Tex.TryGetValue(item.Key, out var tex))
            {
                dialog1.Blocks.Add(new BlockItemModel(item.Key, await BlockListUtils.GetBlockName(item.Key), ImageManager.GetBlockIcon(item.Key, tex), item.Value));
            }
            else
            {
                dialog1.Blocks.Add(new BlockItemModel(item.Key, await BlockListUtils.GetBlockName(item.Key), null, item.Value));
            }
        }

        Window.ShowDialog(dialog1);
    }

    /// <summary>
    /// 下载结构文件
    /// </summary>
    private async void AddSchematic()
    {
        var top = Window.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var res = await PathBinding.AddFileAsync(top, _obj, FileType.Schematic);

        switch (res)
        {
            case null:
                return;
            case false:
                Window.Show(LangUtils.Get("GameEditWindow.Tab11.Text7"));
                return;
            default:
                Window.Notify(LangUtils.Get("GameEditWindow.Tab11.Text3"));
                LoadSchematic();
                break;
        }
    }

    /// <summary>
    /// 拖拽加入结构文件
    /// </summary>
    /// <param name="data"></param>
    public async void DropSchematic(IDataTransfer data)
    {
        var res = await GameBinding.AddFileAsync(_obj, data, FileType.Schematic);
        if (res)
        {
            LoadSchematic();
        }
    }

    /// <summary>
    /// 删除结构文件
    /// </summary>
    /// <param name="obj">结构文件</param>
    public async void DeleteSchematic(SchematicObj obj)
    {
        var res = await Window.ShowChoice(LangUtils.Get("GameEditWindow.Tab12.Text8"));
        if (!res)
        {
            return;
        }
        await obj.DeleteAsync();
        Window.Notify(LangUtils.Get("GameEditWindow.Tab10.Text6"));
        LoadSchematic();
    }

    /// <summary>
    /// 删除所选结构文件
    /// </summary>
    public void DeleteSchematic()
    {
        if (SchematicItem is { } item)
        {
            DeleteSchematic(item.Obj);
        }
    }
}
