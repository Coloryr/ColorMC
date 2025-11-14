using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Controls.Main;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Main;

/// <summary>
/// 主界面
/// </summary>
public partial class MainModel
{
    /// <summary>
    /// 游戏实例列表
    /// </summary>
    public ObservableCollection<GameGroupModel> GameGroups { get; init; } = [];
    /// <summary>
    /// 启动项目列表
    /// </summary>
    private readonly Dictionary<string, GameItemModel> Launchs = [];

    /// <summary>
    /// 用于
    /// </summary>
    private readonly Semaphore _semaphore = new(0, 2);

    /// <summary>
    /// 游戏实例
    /// </summary>
    [ObservableProperty]
    private GameItemModel? _game;
    /// <summary>
    /// 锁定游戏实例
    /// </summary>
    [ObservableProperty]
    private GameItemModel? _oneGame;

    /// <summary>
    /// 是否锁定游戏实例
    /// </summary>
    [ObservableProperty]
    private bool _isOneGame;
    /// <summary>
    /// 是否没有游戏实例
    /// </summary>
    [ObservableProperty]
    private bool _isNotGame;
    /// <summary>
    /// 是否在搜索
    /// </summary>
    [ObservableProperty]
    private bool _gameSearch;
    /// <summary>
    /// 是否有上次运行的游戏实例
    /// </summary>
    [ObservableProperty]
    private bool _haveLast;
    /// <summary>
    /// 是否有上次运行图标
    /// </summary>
    [ObservableProperty]
    private bool _haveGameImage;

    /// <summary>
    /// 实例搜索名字
    /// </summary>
    [ObservableProperty]
    private string _gameSearchText;
    /// <summary>
    /// 上次运行的游戏实例名字
    /// </summary>
    [ObservableProperty]
    private string _lastGameName;

    /// <summary>
    /// 上次运行游戏图标
    /// </summary>
    [ObservableProperty]
    private Bitmap? _gameImage;

    /// <summary>
    /// 是否多选
    /// </summary>
    public bool IsMut { get; private set; }

    /// <summary>
    /// 实例搜索
    /// </summary>
    /// <param name="value"></param>
    partial void OnGameSearchTextChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            foreach (var item in GameGroups)
            {
                item.DisplayAll();
            }
        }
        else
        {
            foreach (var item in GameGroups)
            {
                item.Display(value);
            }
        }
    }
    /// <summary>
    /// 游戏实例切换
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    partial void OnGameChanged(GameItemModel? oldValue, GameItemModel? newValue)
    {
        oldValue?.Unselect();
        newValue?.Select();

        HaveGame = newValue != null;

        if (IsSimple)
        {
            LoadSimple();
        }
    }

    /// <summary>
    /// 运行上次游戏实例
    /// </summary>
    [RelayCommand]
    public void LaunchLast()
    {
        if (!HaveLast)
        {
            return;
        }

        if (Game != null)
        {
            Launch(Game);
        }
        HaveLast = false;
    }

    /// <summary>
    /// 开始多选
    /// </summary>
    public void StartMut()
    {
        if (IsMut)
        {
            return;
        }
        IsMut = true;
        foreach (var item in GameGroups)
        {
            item.Expander = true;
            foreach (var item1 in item.GameList)
            {
                item1.StartCheck();
            }
        }
    }

    /// <summary>
    /// 开始多选
    /// </summary>
    /// <param name="model"></param>
    public void StartMut(GameGroupModel model)
    {
        StartMut();
        foreach (var item in model.GameList)
        {
            item.IsCheck = true;
        }
    }

    /// <summary>
    /// 获取多选
    /// </summary>
    /// <returns></returns>
    public List<GameItemModel> GetMut()
    {
        var list = new List<GameItemModel>();
        foreach (var item in GameGroups)
        {
            foreach (var item1 in item.GameList)
            {
                if (item1.IsCheck && !item1.IsNew)
                {
                    list.Add(item1);
                }
            }
        }
        return list;
    }

    /// <summary>
    /// 结束多选
    /// </summary>
    /// <returns></returns>
    public List<GameItemModel> EndMut()
    {
        if (!IsMut)
        {
            return [];
        }
        var list = new List<GameItemModel>();
        IsMut = false;
        foreach (var item in GameGroups)
        {
            foreach (var item1 in item.GameList)
            {
                item1.StopCheck();
                if (item1.IsCheck && !item1.IsNew)
                {
                    list.Add(item1);
                }
            }
        }
        return list;
    }

    /// <summary>
    /// 多选编辑实例
    /// </summary>
    public void MutEdit()
    {
        var list = EndMut();
        foreach (var item in list)
        {
            WindowManager.ShowGameEdit(item.Obj);
        }
    }

    /// <summary>
    /// 多选删除实例
    /// </summary>
    public async void MutDelete()
    {
        var list = EndMut();
        if (list.Count == 0)
        {
            return;
        }

        var res = await Model.ShowAsync(string.Format(LanguageUtils.Get("MainWindow.Text78"), list.Count));
        if (!res)
        {
            return;
        }
        Model.Progress(LanguageUtils.Get("GameEditWindow.Tab1.Text35"));
        InstancesPath.DisableWatcher = true;
        await Parallel.ForEachAsync(list, async (item, cancel) =>
        {
            if (GameManager.IsAdd(item.Obj))
            {
                Model.Show(LanguageUtils.Get("GameEditWindow.Tab1.Text46"));
                return;
            }
            await GameBinding.DeleteGameAsync(item.Obj);
        });
        InstancesPath.DisableWatcher = false;
        Model.ProgressClose();
        Model.InputClose();
        LoadGameItem();
    }

    /// <summary>
    /// 多选启动
    /// </summary>
    public void MutLaunch()
    {
        var list = EndMut();
        if (list.Count == 0)
        {
            return;
        }

        Launch(list);
    }

    /// <summary>
    /// 多选修改游戏分组
    /// </summary>
    public async void MutEditGroup()
    {
        var list = EndMut();
        if (list.Count == 0)
        {
            return;
        }

        var model = new AddGroupModel(Model, null);
        var res = await DialogHost.Show(model, MainControl.DialogName);
        if (res is not true)
        {
            return;
        }

        foreach (var item in list)
        {
            GameBinding.MoveGameGroup(item.Obj, model.GroupItem);
        }
    }

    /// <summary>
    /// 搜索游戏实例
    /// </summary>
    public void Search()
    {
        GameSearch = true;
        GameSearchText = "";
    }

    /// <summary>
    /// 关闭搜索实例
    /// </summary>
    public void SearchClose()
    {
        GameSearch = false;
        GameSearchText = "";
    }

    /// <summary>
    /// 编辑游戏分组
    /// </summary>
    /// <param name="obj"></param>
    public async void EditGroup(GameItemModel obj)
    {
        var model = new AddGroupModel(Model, obj.Obj.GroupName);
        var res = await DialogHost.Show(model, MainControl.DialogName);
        if (res is not true)
        {
            return;
        }

        GameBinding.MoveGameGroup(obj.Obj, model.GroupItem);
    }

    /// <summary>
    /// 加载游戏项目
    /// </summary>
    public void LoadGameItem()
    {
        IsNotGame = InstancesPath.IsNotGame;

        var config = GuiConfigUtils.Config.ServerCustom;

        GameItemModel? last = null;

        if (config?.LockGame == true)
        {
            GameGroups.Clear();
            IsFirst = true;
            var game = InstancesPath.GetGame(config?.GameName);
            if (game == null)
            {
                IsGameError = true;
                IsOneGame = false;
            }
            else
            {
                IsGameError = false;
                OneGame = new(Model, this, game)
                {
                    OneGame = true,
                    IsSelect = true
                };
                IsOneGame = true;
            }
        }
        else
        {
            IsGameError = false;
            IsOneGame = false;
            var list = InstancesPath.Groups;
            var uuid = ConfigBinding.GetLastLaunch();

            if (IsFirst)
            {
                //第一次加载项目
                IsFirst = false;
                GameGroupModel? DefaultGroup = null;

                foreach (var item in list)
                {
                    if (item.Key == " ")
                    {
                        DefaultGroup = new(Model, this, " ", LanguageUtils.Get("MainWindow.Text68"), item.Value);
                        if (list.Count > 0)
                        {
                            DefaultGroup.Expander = false;
                        }
                        last ??= DefaultGroup.Find(uuid);
                    }
                    else
                    {
                        var group = new GameGroupModel(Model, this, item.Key, item.Key, item.Value);
                        GameGroups.Add(group);
                        if (list.Count > 0)
                        {
                            group.Expander = false;
                        }
                        last ??= group.Find(uuid);
                    }
                }

                if (DefaultGroup != null)
                {
                    GameGroups.Add(DefaultGroup);
                }
                Select(last);
            }
            else
            {
                //过滤新旧项目
                var list1 = new List<GameGroupModel>(GameGroups);
                foreach (var item in list1)
                {
                    if (list.TryGetValue(item.Key, out var value))
                    {
                        item.SetItems(value);
                        list.Remove(item.Key);
                    }
                    else
                    {
                        GameGroups.Remove(item);
                    }
                }
                foreach (var item in list)
                {
                    var group = new GameGroupModel(Model, this, item.Key, item.Key, item.Value);
                    GameGroups.Add(group);
                    if (list.Count > 0)
                    {
                        group.Expander = false;
                    }
                    last ??= group.Find(uuid);
                }

                foreach (var item in list1)
                {
                    last ??= item.Find(uuid);
                }

                Select(last);
            }
            //是否有上次运行项目
            if (last != null && LastGameName == null)
            {
                if (GuiConfigUtils.Config.Card.Last)
                {
                    HaveLast = true;
                }
                LastGameName = string.Format(LanguageUtils.Get("MainWindow.Text26"), last.Name);
                var file = last.Obj.GetIconFile();
                if (File.Exists(file))
                {
                    try
                    {
                        GameImage = new Bitmap(file);
                        HaveGameImage = true;
                    }
                    catch
                    {

                    }
                }
            }
        }

        OnPropertyChanged(SwitchView);
        if (IsSimple)
        {
            Dispatcher.UIThread.Post(() =>
            {
                Game = null;
                Game = last;
            });
        }
        MinModeChange();
    }

    /// <summary>
    /// 选择项目
    /// </summary>
    /// <param name="uuid">游戏实例UUID</param>
    public void Select(string? uuid)
    {
        if (uuid == null)
        {
            Select(obj: null);
        }
        GameItemModel? model;
        foreach (var item in GameGroups)
        {
            model = item.Find(uuid);
            if (model != null)
            {
                Select(model);
                return;
            }
        }
    }

    /// <summary>
    /// 选择游戏项目
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public void Select(GameItemModel? obj)
    {
        if (HaveLast == true && obj?.UUID != ConfigBinding.GetLastLaunch())
        {
            HaveLast = false;
        }

        Game = obj;
    }

    /// <summary>
    /// 游戏退出
    /// </summary>
    /// <param name="uuid">游戏实例UUID</param>
    public void GameClose(string uuid)
    {
        if (Launchs.Remove(uuid, out var con))
        {
            con.IsLaunch = false;
        }
    }

    /// <summary>
    /// 启动游戏实例
    /// </summary>
    /// <param name="obj">游戏实例</param>
    public async void Launch(GameItemModel obj)
    {
        Select(obj);

        IsLaunch = true;
        if (GuiConfigUtils.Config.CloseBeforeLaunch)
        {
            Model.Progress(LanguageUtils.Get("MainWindow.Text66"));
        }
        var item = Game!;
        var game = item.Obj;
        item.IsLaunch = false;
        item.IsLoad = true;
        Model.Notify(LanguageUtils.Get(string.Format(LanguageUtils.Get("MainWindow.Text72"), game.Name)));
        var res = await GameBinding.LaunchAsync(Model, game, hide: GuiConfigUtils.Config.CloseBeforeLaunch);
        item.IsLoad = false;
        if (GuiConfigUtils.Config.CloseBeforeLaunch)
        {
            Model.ProgressClose();
        }
        if (res.Res == false)
        {
            if (res.LoginFail && res.User!.AuthType != AuthType.OAuth)
            {
                var res1 = await Model.ShowAsync(string.Format(LanguageUtils.Get("MainWindow.Text86"), res.Message!));
                if (res1)
                {
                    WindowManager.ShowUser(relogin: true);
                }
            }
            else if (!string.IsNullOrWhiteSpace(res.Message))
            {
                Model.Show(res.Message!);
            }
        }
        else
        {
            Model.Notify(LanguageUtils.Get("MainWindow.Text65"));

            item.IsLaunch = true;
            Launchs.Add(game.UUID, item);

            if (GuiConfigUtils.Config.CloseBeforeLaunch)
            {
                Model.Progress(LanguageUtils.Get("MainWindow.Text71"));
            }
        }
        IsLaunch = false;
    }

    /// <summary>
    /// 启动游戏实例
    /// </summary>
    /// <param name="list">游戏实例列表</param>
    public async void Launch(ICollection<GameItemModel> list)
    {
        if (IsLaunch)
        {
            return;
        }
        IsLaunch = true;

        if (list.Count == 1)
        {
            Launch(list.First());
            return;
        }

        var list1 = new List<GameSettingObj>();
        foreach (var item in list)
        {
            list1.Add(item.Obj);
            item.IsLaunch = false;
            item.IsLoad = true;
        }

        Model.Progress(LanguageUtils.Get("MainWindow.Text66"));
        var res1 = await GameBinding.LaunchAsync(Model, list1);
        Model.ProgressClose();
        if (res1.Message != null)
        {
            Model.Show(res1.Message);
            return;
        }

        //游戏实例列表
        foreach (var item in list)
        {
            item.IsLoad = false;
            if (res1.States.TryGetValue(item.UUID, out var res2))
            {
                if (res2.Res)
                {
                    item.IsLaunch = true;
                    Launchs.Add(item.UUID, item);
                }
            }
        }

        foreach (var item in res1.States)
        {
            if (item.Value.LoginFail && res1.User!.AuthType != AuthType.OAuth)
            {
                var res2 = await Model.ShowAsync(string.Format(LanguageUtils.Get("MainWindow.Text86"), item.Value.Message!));
                if (res2)
                {
                    WindowManager.ShowUser(relogin: true);
                }
            }
        }

        IsLaunch = false;
    }

    /// <summary>
    /// 启动游戏实例
    /// </summary>
    /// <param name="list">游戏实例列表</param>
    public void Launch(ICollection<string> list)
    {
        var list1 = new List<GameItemModel>();
        foreach (var item in list)
        {
            if (GetGame(item) is { } game)
            {
                list1.Add(game);
            }
        }
        if (list1.Count > 0)
        {
            Launch(list1);
        }
    }

    /// <summary>
    /// 获取游戏实例
    /// </summary>
    /// <param name="uuid">游戏实例UUID</param>
    /// <returns>游戏实例显示模型</returns>
    public GameItemModel? GetGame(string? uuid)
    {
        if (uuid == null)
        {
            return null;
        }
        foreach (var item in GameGroups)
        {
            if (item.Find(uuid) is { } game)
            {
                return game;
            }
        }

        return null;
    }

    /// <summary>
    /// 标星游戏实例
    /// </summary>
    /// <param name="model">游戏实例</param>
    public void DoStar(GameItemModel model)
    {
        model.IsStar = !model.IsStar;
        if (model.IsStar)
        {
            GameManager.AddStar(model.Obj);
            foreach (var group in GameGroups)
            {
                if (group.Star(model.Obj.UUID))
                {
                    return;
                }
            }
        }
        else
        {
            GameManager.RemoveStar(model.Obj);
            foreach (var group in GameGroups)
            {
                if (group.UnStar(model.Obj.UUID))
                {
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 导出启动指令
    /// </summary>
    /// <param name="obj"></param>
    public void ExportCmd(GameSettingObj obj)
    {
        GameBinding.ExportCmd(obj, Model, CancellationToken.None);
    }
}
