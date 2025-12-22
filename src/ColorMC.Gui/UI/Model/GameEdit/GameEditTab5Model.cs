using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Input;
using AvaloniaEdit.Utils;
using ColorMC.Core.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.GameEdit;

/// <summary>
/// 游戏实例编辑
/// </summary>
public partial class GameEditModel
{
    /// <summary>
    /// 存档列表
    /// </summary>
    public ObservableCollection<WorldModel> WorldList { get; init; } = [];

    /// <summary>
    /// 存档项目
    /// </summary>
    private readonly List<WorldModel> _worldItems = [];

    /// <summary>
    /// 选择的存档
    /// </summary>
    private WorldModel? _selectWorld;

    /// <summary>
    /// 是否没有存档
    /// </summary>
    [ObservableProperty]
    private bool _worldEmptyDisplay;
    /// <summary>
    /// 存档筛选
    /// </summary>
    [ObservableProperty]
    private string _worldText;

    partial void OnWorldTextChanged(string value)
    {
        LoadWorldDisplay();
    }

    /// <summary>
    /// 加载存档列表
    /// </summary>
    /// <returns></returns>
    public async void LoadWorld()
    {
        var dialog = Window.ShowProgress(LangUtils.Get("GameEditWindow.Tab5.Text10"));
        _worldItems.Clear();

        var res = await _obj.GetSavesAsync();
        foreach (var item in res)
        {
            var item1 = new WorldModel(this, item);
            await item1.Load();
            _worldItems.Add(item1);
        }
        LoadWorldDisplay();

        Window.CloseDialog(dialog);
        Window.Notify(LangUtils.Get("GameEditWindow.Tab5.Text21"));
    }

    /// <summary>
    /// 打开存档目录
    /// </summary>
    private void OpenBackupWorld()
    {
        PathBinding.OpenPath(_obj, PathType.WorldBackPath);
    }

    /// <summary>
    /// 还原备份存档
    /// </summary>
    private async void BackupWorld()
    {
        var info = new DirectoryInfo(_obj.GetSaveBackupPath());
        if (!info.Exists)
        {
            info.Create();
        }

        //选存档
        var list = info.GetFiles();
        var names = new List<string>();
        foreach (var item in list)
        {
            names.Add(item.Name);
        }
        if (names.Count == 0)
        {
            Window.Show(LangUtils.Get("GameEditWindow.Tab5.Text25"));
            return;
        }
        var dialog = new SelectModel(Window.WindowId)
        {
            Text = LangUtils.Get("GameEditWindow.Tab5.Text13"),
            Items = [.. names]
        };
        var res = await Window.ShowDialogWait(dialog);
        if (res is not true)
        {
            return;
        }
        var item1 = list[dialog.Index];
        var res1 = await Window.ShowChoice(LangUtils.Get("GameEditWindow.Tab5.Text14"));
        if (!res1)
        {
            return;
        }

        //开始备份
        var dialog1 = Window.ShowProgress(LangUtils.Get("GameEditWindow.Tab5.Text15"));
        var zip = new ZipGui(Window, dialog1);
        res1 = await _obj.UnzipBackupWorldAsync(item1.FullName, zip);
        zip.Stop();
        Window.CloseDialog(dialog1);
        if (!res1)
        {
            Window.Show(LangUtils.Get("GameEditWindow.Tab5.Text24"));
        }
        else
        {
            Window.Notify(LangUtils.Get("GameEditWindow.Tab5.Text16"));
            LoadWorld();
        }
    }

    /// <summary>
    /// 编辑存档
    /// </summary>
    private async void EditWorld()
    {
        var dialog = Window.ShowProgress(LangUtils.Get("GameEditWindow.Tab5.Text17"));
        var res = await ToolUtils.OpenMapEditAsync();
        Window.CloseDialog(dialog);
        if (!res.State)
        {
            Window.Show(res.Data!);
        }
    }

    /// <summary>
    /// 打开存档路径
    /// </summary>
    private void OpenWorld()
    {
        PathBinding.OpenPath(_obj, PathType.SavePath);
    }

    /// <summary>
    /// 下载存档
    /// </summary>
    private void AddWorld()
    {
        WindowManager.ShowAdd(_obj, FileType.Save);
    }

    /// <summary>
    /// 导入存档
    /// </summary>
    private async void ImportWorld()
    {
        var top = Window.GetTopLevel();
        if (top == null)
        {
            return;
        }

        var file = await PathBinding.AddFileAsync(top, _obj, FileType.Save);
        if (file == null)
        {
            return;
        }

        if (file == false)
        {
            Window.Notify(LangUtils.Get("GameEditWindow.Tab5.Text23"));
            return;
        }

        Window.Notify(LangUtils.Get("GameEditWindow.Tab4.Text20"));
        LoadWorld();
    }

    /// <summary>
    /// 加载存档显示列表
    /// </summary>
    public void LoadWorldDisplay()
    {
        WorldList.Clear();
        if (string.IsNullOrWhiteSpace(WorldText))
        {
            WorldList.AddRange(_worldItems);
        }
        else
        {
            WorldList.AddRange(_worldItems.Where(item => item.Name.Contains(WorldText)));
        }

        WorldEmptyDisplay = WorldList.Count == 0;
    }

    /// <summary>
    /// 拖拽导入存档
    /// </summary>
    /// <param name="data"></param>
    public async void DropWorld(IDataTransfer data)
    {
        var res = await GameBinding.AddFileAsync(_obj, data, FileType.Save);
        if (res)
        {
            LoadWorld();
        }
    }

    /// <summary>
    /// 选择存档
    /// </summary>
    /// <param name="item"></param>
    public void SetSelectWorld(WorldModel item)
    {
        _selectWorld?.IsSelect = false;
        _selectWorld = item;
        _selectWorld.IsSelect = true;
    }

    /// <summary>
    /// 删除存档
    /// </summary>
    /// <param name="obj"></param>
    public async void DeleteWorld(WorldModel obj)
    {
        var res = await Window.ShowChoice(
            string.Format(LangUtils.Get("GameEditWindow.Tab5.Text7"), obj.Name));
        if (!res)
        {
            return;
        }

        await obj.World.DeleteAsync();
        Window.Notify(LangUtils.Get("Text.DeleteDone"));
        LoadWorld();
    }

    /// <summary>
    /// 导出存档
    /// </summary>
    /// <param name="obj"></param>
    public async void Export(WorldModel obj)
    {
        var top = Window.GetTopLevel();
        if (top == null)
        {
            return;
        }

        var dialog = Window.ShowProgress(LangUtils.Get("GameEditWindow.Tab5.Text9"));
        var file = await PathBinding.SaveFileAsync(top, FileType.Save, [obj]);
        Window.CloseDialog(dialog);
        if (file == null)
        {
            return;
        }

        if (file == false)
        {
            Window.Show(LangUtils.Get("GameEditWindow.Tab5.Text22"));
        }
        else
        {
            Window.Notify(LangUtils.Get("GameEditWindow.Tab5.Text8"));
        }
    }

    /// <summary>
    /// 备份存档
    /// </summary>
    /// <param name="obj"></param>
    public async void BackupWorld(WorldModel obj)
    {
        var dialog = Window.ShowProgress(LangUtils.Get("GameEditWindow.Tab5.Text11"));
        var res = await GameBinding.BackupWorldAsync(obj.World);
        Window.CloseDialog(dialog);
        if (res)
        {
            Window.Notify(LangUtils.Get("GameEditWindow.Tab5.Text12"));
        }
        else
        {
            Window.Show(LangUtils.Get("GameEditWindow.Tab5.Text26"));
        }
    }

    /// <summary>
    /// 启动存档
    /// </summary>
    /// <param name="world">存档</param>
    public async void LaunchWorld(WorldModel world)
    {
        if (GameManager.IsGameRun(world.World.Game))
        {
            return;
        }

        var res = await GameBinding.LaunchAsync(world.World.Game, Window, null, world.World, GuiConfigUtils.Config.CloseBeforeLaunch);
        if (!res.Res && !string.IsNullOrWhiteSpace(res.Message))
        {
            Window.Show(res.Message!);
        }
    }
}
