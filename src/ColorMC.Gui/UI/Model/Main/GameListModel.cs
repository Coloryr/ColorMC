using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Main;

public partial class MainModel
{
    public ObservableCollection<string> GroupList { get; init; } = [];
    public ObservableCollection<GameGroupModel> GameGroups { get; init; } = [];

    [ObservableProperty]
    private GameItemModel? _game;
    [ObservableProperty]
    private GameItemModel? _oneGame;

    [ObservableProperty]
    private bool _isOneGame;
    [ObservableProperty]
    private bool _isNotGame;
    [ObservableProperty]
    private bool _gameSearch;

    [ObservableProperty]
    private string? _groupItem;
    [ObservableProperty]
    private string _gameSearchText;

    [ObservableProperty]
    private string _lastGameName;
    [ObservableProperty]
    private bool _haveLast;

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

    [RelayCommand]
    public async Task AddGroup()
    {
        var (Cancel, Text) = await Model.ShowInputOne(App.Lang("Text.Group"), false);
        if (Cancel)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Text))
        {
            Model.Show(App.Lang("MainWindow.Error3"));
            return;
        }

        if (!GameBinding.AddGameGroup(Text))
        {
            Model.Show(App.Lang("MainWindow.Error4"));
            return;
        }

        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);
    }

    [RelayCommand]
    public void Confirm()
    {
        _isCancel = false;
        _semaphore.Release();
    }

    [RelayCommand]
    public void Cancel()
    {
        _isCancel = true;
        _semaphore.Release();
    }

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

    public void IconChange(string uuid)
    {
        foreach (var item in GameGroups)
        {
            item.IconChange(uuid);
        }
    }

    public void Search()
    {
        GameSearch = true;
        GameSearchText = "";
    }

    public void SearchClose()
    {
        GameSearch = false;
        GameSearchText = "";
    }

    public Task<(bool, string?)> Set(GameItemModel obj)
    {
        GroupList.Clear();
        GroupList.AddRange(GameBinding.GetGameGroups().Keys);

        DialogHost.Show(this, "MainCon");

        GroupItem = obj.Obj.GroupName;

        return Task.Run(() =>
        {
            _semaphore.WaitOne();
            return (_isCancel, GroupItem);
        });
    }

    public async void EditGroup(GameItemModel obj)
    {
        await Set(obj);
        DialogHost.Close("MainCon");

        if (_isCancel)
        {
            return;
        }

        GameBinding.MoveGameGroup(obj.Obj, GroupItem);
    }

    public void IsDelete()
    {
        Game = null;
        LoadGameItem();
    }

    public void LoadGameItem()
    {
        IsNotGame = GameBinding.IsNotGame;

        var config = GuiConfigUtils.Config.ServerCustom;
        if (config?.PlayMusic == true)
        {
            Model.Title = "ColorMC " + App.Lang("MainWindow.Info33");
            MusicDisplay = true;
        }
        else
        {
            MusicDisplay = false;
        }

        if (config?.LockGame == true)
        {
            GameGroups.Clear();
            GroupList.Clear();
            IsFirst = true;
            var game = GameBinding.GetGame(config?.GameName);
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
                    OneGame = true
                };
                IsOneGame = true;
            }
        }
        else
        {
            IsGameError = false;
            IsOneGame = false;
            var list = GameBinding.GetGameGroups();
            var uuid = ConfigBinding.GetLastLaunch();
            GameItemModel? last = null;
            if (IsFirst)
            {
                IsFirst = false;
                GameGroupModel? DefaultGroup = null;

                foreach (var item in list)
                {
                    if (item.Key == " ")
                    {
                        DefaultGroup = new(Model, this, " ", App.Lang("MainWindow.Info20"), item.Value);
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
            if (last != null && LastGameName == null)
            {
                HaveLast = true;
                LastGameName = string.Format(App.Lang("MainWindow.Text26"), last.Name);
            }
        }

        OnPropertyChanged(SwitchView);
    }

    public void Select(string? uuid)
    {
        Dispatcher.UIThread.Post(() =>
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
        });
    }

    public void Select(GameItemModel? obj)
    {
        HaveLast = false;
        if (Game != null)
        {
            Game.IsSelect = false;
        }
        Game = obj;
        if (Game != null)
        {
            Game.IsSelect = true;
        }
    }

    /// <summary>
    /// 游戏退出
    /// </summary>
    /// <param name="uuid"></param>
    public void GameClose(string uuid)
    {
        if (Launchs.Remove(uuid, out var con))
        {
            con.IsLaunch = false;
        }
    }

    public async void Launch(GameItemModel obj)
    {
        Select(obj);

        IsLaunch = true;
        if (GuiConfigUtils.Config.CloseBeforeLaunch)
        {
            Model.Progress(App.Lang("MainWindow.Info3"));
        }
        var item = Game!;
        var game = item.Obj;
        item.IsLaunch = false;
        item.IsLoad = true;
        Model.Notify(App.Lang(string.Format(App.Lang("MainWindow.Info28"), game.Name)));
        var res = await GameBinding.Launch(Model, game, hide: GuiConfigUtils.Config.CloseBeforeLaunch);
        item.IsLoad = false;
        if (GuiConfigUtils.Config.CloseBeforeLaunch)
        {
            Model.ProgressClose();
        }
        if (res.Res == false)
        {
            if (res.LoginFail && res.User!.AuthType != AuthType.OAuth)
            {
                var res1 = await Model.ShowWait(string.Format(App.Lang("MainWindow.Error8"), res.Message!));
                if (res1)
                {
                    WindowManager.ShowUser(relogin: true);
                }
            }
            else
            {
                Model.Show(res.Message!);
            }
        }
        else
        {
            Model.Notify(App.Lang("MainWindow.Info2"));

            item.IsLaunch = true;
            Launchs.Add(game.UUID, item);

            if (GuiConfigUtils.Config.CloseBeforeLaunch)
            {
                Model.Progress(App.Lang("MainWindow.Info26"));
            }
        }
        IsLaunch = false;
    }

    public async void Launch(ICollection<GameItemModel> list)
    {
        var res = await Model.ShowWait(App.Lang("MainWindow.Info41"));
        if (!res)
        {
            return;
        }

        IsLaunch = true;

        var list1 = new List<GameSettingObj>();
        foreach (var item in list)
        {
            list1.Add(item.Obj);
            item.IsLaunch = false;
            item.IsLoad = true;
        }

        Model.Progress(App.Lang("MainWindow.Info3"));
        var res1 = await GameBinding.Launch(Model, list1);
        Model.ProgressClose();
        if (res1.Message != null)
        {
            Model.Show(res1.Message);
        }

        foreach (var item in list)
        {
            item.IsLoad = false;
            if (res1.Done?.Contains(item.UUID) == true)
            {
                item.IsLaunch = true;
                Launchs.Add(item.UUID, item);
            }
        }

        if (res1.Fail?.ContainsValue(LaunchState.LoginFail) == true
            && res1.User!.AuthType != AuthType.OAuth)
        {
            var res2 = await Model.ShowWait(string.Format(App.Lang("MainWindow.Error8"), res1.Message!));
            if (res2)
            {
                WindowManager.ShowUser(relogin: true);
            }
        }
        IsLaunch = false;
    }

    public GameItemModel? GetGame(string uuid)
    {
        return GameGroups.Select(item => item.Find(uuid)).Where(item => item != null)
            .FirstOrDefault();
    }
}
