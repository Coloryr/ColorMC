using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input;
using AvaloniaEdit.Utils;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
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
    [RelayCommand]
    public async Task LoadWorld()
    {
        Model.Progress(App.Lang("GameEditWindow.Tab5.Info5"));
        _worldItems.Clear();

        var res = await GameBinding.GetWorldsAsync(_obj!);
        foreach (var item in res)
        {
            var item1 = new WorldModel(this, item);
            await item1.Load();
            _worldItems.Add(item1);
        }
        LoadWorldDisplay();

        Model.ProgressClose();
        Model.Notify(App.Lang("GameEditWindow.Tab5.Info17"));
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
        var info = new DirectoryInfo(_obj.GetWorldBackupPath());
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
            Model.Show(App.Lang("GameEditWindow.Tab5.Error5"));
            return;
        }
        var res = await Model.Combo(App.Lang("GameEditWindow.Tab5.Info9"), names);
        if (res.Cancel)
        {
            return;
        }
        var item1 = list[res.Index];
        var res1 = await Model.ShowAsync(App.Lang("GameEditWindow.Tab5.Info10"));
        if (!res1)
        {
            return;
        }

        //开始备份
        Model.Progress(App.Lang("GameEditWindow.Tab5.Info11"));
        res1 = await GameBinding.BackupWorld(_obj, item1, Model.ShowAsync);
        Model.ProgressClose();
        if (!res1)
        {
            Model.Show(App.Lang("GameEditWindow.Tab5.Error4"));
        }
        else
        {
            Model.Notify(App.Lang("GameEditWindow.Tab5.Info12"));
            await LoadWorld();
        }
    }

    /// <summary>
    /// 编辑存档
    /// </summary>
    private async void EditWorld()
    {
        Model.Progress(App.Lang("GameEditWindow.Tab5.Info13"));
        var res = await ToolUtils.OpenMapEditAsync();
        Model.ProgressClose();
        if (!res.State)
        {
            Model.Show(res.Message!);
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
        WindowManager.ShowAdd(_obj, FileType.World);
    }

    /// <summary>
    /// 导入存档
    /// </summary>
    private async void ImportWorld()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }

        var file = await PathBinding.AddFile(top, _obj, FileType.World);
        if (file == null)
        {
            return;
        }

        if (file == false)
        {
            Model.Notify(App.Lang("GameEditWindow.Tab5.Error2"));
            return;
        }

        Model.Notify(App.Lang("GameEditWindow.Tab4.Info2"));
        await LoadWorld();
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
    public async void DropWorld(IDataObject data)
    {
        var res = await GameBinding.AddFile(_obj, data, FileType.World);
        if (res)
        {
            await LoadWorld();
        }
    }

    /// <summary>
    /// 选择存档
    /// </summary>
    /// <param name="item"></param>
    public void SetSelectWorld(WorldModel item)
    {
        if (_selectWorld != null)
        {
            _selectWorld.IsSelect = false;
        }
        _selectWorld = item;
        _selectWorld.IsSelect = true;
    }

    /// <summary>
    /// 删除存档
    /// </summary>
    /// <param name="obj"></param>
    public async void DeleteWorld(WorldModel obj)
    {
        var res = await Model.ShowAsync(
            string.Format(App.Lang("GameEditWindow.Tab5.Info1"), obj.Name));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteWorld(obj.World);
        Model.Notify(App.Lang("GameEditWindow.Tab4.Info3"));
        await LoadWorld();
    }

    /// <summary>
    /// 导出存档
    /// </summary>
    /// <param name="obj"></param>
    public async void Export(WorldModel obj)
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }

        Model.Progress(App.Lang("GameEditWindow.Tab5.Info4"));
        var file = await PathBinding.SaveFile(top, FileType.World, [obj]);
        Model.ProgressClose();
        if (file == null)
        {
            return;
        }

        if (file == false)
        {
            Model.Show(App.Lang("GameEditWindow.Tab5.Error1"));
        }
        else
        {
            Model.Notify(App.Lang("GameEditWindow.Tab5.Info3"));
        }
    }

    /// <summary>
    /// 备份存档
    /// </summary>
    /// <param name="obj"></param>
    public async void BackupWorld(WorldModel obj)
    {
        Model.Progress(App.Lang("GameEditWindow.Tab5.Info7"));
        var res = await GameBinding.BackupWorld(obj.World);
        Model.ProgressClose();
        if (res)
        {
            Model.Notify(App.Lang("GameEditWindow.Tab5.Info8"));
        }
        else
        {
            Model.Show(App.Lang("GameEditWindow.Tab5.Error3"));
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

        var res = await GameBinding.Launch(Model, world.World.Game, world.World, GuiConfigUtils.Config.CloseBeforeLaunch);
        if (!res.Res && !string.IsNullOrWhiteSpace(res.Message))
        {
            Model.Show(res.Message!);
        }
    }
}
