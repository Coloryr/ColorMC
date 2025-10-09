using System.Collections.ObjectModel;
using Avalonia.Input;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UIBinding;
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
    public ObservableCollection<SchematicObj> SchematicList { get; set; } = [];

    /// <summary>
    /// 选中的结构文件
    /// </summary>
    [ObservableProperty]
    private SchematicObj? _schematicItem;

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
        Model.Progress(App.Lang("GameEditWindow.Tab12.Info3"));
        SchematicList.Clear();
        SchematicList.AddRange(await GameBinding.GetSchematics(_obj));
        Model.ProgressClose();
        SchematicEmptyDisplay = SchematicList.Count == 0;
        Model.Notify(App.Lang("GameEditWindow.Tab12.Info1"));
    }

    /// <summary>
    /// 下载结构文件
    /// </summary>
    private async void AddSchematic()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var res = await PathBinding.AddFile(top, _obj, FileType.Schematic);

        switch (res)
        {
            case null:
                return;
            case false:
                Model.Show(App.Lang("GameEditWindow.Tab11.Error1"));
                return;
            default:
                Model.Notify(App.Lang("GameEditWindow.Tab11.Info1"));
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
        var res = await GameBinding.AddFile(_obj, data, FileType.Schematic);
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
        var res = await Model.ShowAsync(App.Lang("GameEditWindow.Tab12.Info2"));
        if (!res)
        {
            return;
        }
        await GameBinding.DeleteSchematic(obj);
        Model.Notify(App.Lang("GameEditWindow.Tab10.Info5"));
        LoadSchematic();
    }

    /// <summary>
    /// 删除所选结构文件
    /// </summary>
    public void DeleteSchematic()
    {
        if (SchematicItem is { } item)
        {
            DeleteSchematic(item);
        }
    }
}
