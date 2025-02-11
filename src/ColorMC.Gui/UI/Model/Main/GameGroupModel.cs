using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Main;

public partial class GameGroupModel : TopModel
{
    public ObservableCollection<GameItemModel> GameList { get; init; } = [];

    public bool IsPhone { get; } = SystemInfo.Os == OsType.Android;
    public string Header { get; }
    public string Key { get; }

    [ObservableProperty]
    private bool _expander = true;

    public readonly IMutTop Top;

    private readonly Dictionary<string, GameItemModel> _items = [];
    private readonly GameItemModel _addItem;

    public GameGroupModel(BaseModel model, IMutTop top, string key, string name,
        List<GameSettingObj> list) : base(model)
    {
        Top = top;
        Header = name;
        Key = key;
        GameList.Clear();
        _items.Clear();
        foreach (var item in list)
        {
            var model1 = new GameItemModel(Model, Top, item);
            _items.Add(item.UUID, model1);
        }
        _addItem = new(Model, Key == InstancesPath.DefaultGroup ? null : Key);
        Task.Run(() =>
        {
            bool res = false;
            int index = 1;
            if (_items.Count == 5)
            {
                var random = new Random();
                if (random.Next(2000) == 666)
                {
                    res = true;
                }
            }

            var list1 = new List<GameItemModel>();

            foreach (var item in _items)
            {
                if (!GameManager.IsStar(item.Value.Obj))
                {
                    list1.Add(item.Value);
                    continue;
                }
                Thread.Sleep(50);
                Dispatcher.UIThread.Invoke(() =>
                {
                    GameList.Add(item.Value);
                    if (res)
                    {
                        item.Value.Index = index++;
                    }
                });
            }

            foreach (var item in list1)
            {
                Thread.Sleep(50);
                Dispatcher.UIThread.Invoke(() =>
                {
                    GameList.Add(item);
                    if (res)
                    {
                        item.Index = index++;
                    }
                });
            }

            Dispatcher.UIThread.Post(() =>
            {
                GameList.Add(_addItem);
            });
        });
    }

    [RelayCommand]
    public async Task LaunchAll()
    {
        var res = await Model.ShowAsync(App.Lang("MainWindow.Info41"));
        if (!res)
        {
            return;
        }
        Top.Launch(_items.Values);
    }

    public bool DropIn(IDataObject data)
    {
        return data.Get(BaseBinding.DrapType) is not string c
            || !_items.ContainsKey(c);
    }

    public void Drop(IDataObject data)
    {
        if (data.Get(BaseBinding.DrapType) is not string c)
        {
            return;
        }

        var game = Top.GetGame(c);
        if (game == null)
        {
            return;
        }

        game.IsDrop = false;

        if (_items.ContainsKey(c))
        {
            return;
        }

        GameBinding.MoveGameGroup(game.Obj, Key);
    }

    public GameItemModel? Find(string? uuid)
    {
        if (string.IsNullOrWhiteSpace(uuid))
        {
            return null;
        }
        if (_items.TryGetValue(uuid, out var item))
        {
            Expander = true;
            return item;
        }

        return null;
    }

    public void SetItems(List<GameSettingObj> list)
    {
        var remove = new List<string>();
        var ins = new List<GameSettingObj>();

        foreach (var item in list)
        {
            if (_items.ContainsKey(item.UUID))
            {
                continue;
            }
            ins.Add(item);
        }

        foreach (var item in _items.Keys)
        {
            if (list.Any(item1 => item1.UUID == item))
            {
                continue;
            }
            remove.Add(item);
        }

        if (GameList.Count == 0 || GameList.Count - 1 < 0)
        {
            return;
        }
        GameList.RemoveAt(GameList.Count - 1);

        foreach (var item in remove)
        {
            _items.Remove(item);
            var model = GameList.FirstOrDefault(item1 => item1.Obj.UUID == item);
            if (model != null)
            {
                model.Close();
                GameList.Remove(model);
            }
        }

        foreach (var item in ins)
        {
            var model1 = new GameItemModel(Model, Top, item);
            _items.Add(item.UUID, model1);
        }

        Task.Run(() =>
        {
            foreach (var item in ins)
            {
                Thread.Sleep(50);
                Dispatcher.UIThread.Invoke(() =>
                {
                    GameList.Add(_items[item.UUID]);
                });
            }

            Dispatcher.UIThread.Post(() =>
            {
                GameList.Add(_addItem);
            });
        });
    }

    public override void Close()
    {
        foreach (var item in GameList)
        {
            item.Close();
        }
        GameList.Clear();
        _items.Clear();
    }

    public void DisplayAll()
    {
        foreach (var item in GameList)
        {
            item.IsDisplay = true;
        }
    }

    public void Display(string value)
    {
        foreach (var item in GameList)
        {
            if (item.IsNew)
            {
                item.IsDisplay = false;
            }
            else
            {
                item.IsDisplay = item.Name.Contains(value);
            }
        }
    }

    public void IconChange(string uuid)
    {
        foreach (var item in GameList)
        {
            if (item.IsNew)
            {
                continue;
            }
            else if (item.UUID == uuid)
            {
                item.LoadIcon();
            }
        }
    }

    public bool Star(string uuid)
    {
        foreach (var item in GameList)
        {
            if (item.IsNew)
            {
                continue;
            }
            else if (item.UUID == uuid)
            {
                GameList.Move(GameList.IndexOf(item), 0);
                return true;
            }
        }

        return false;
    }

    public bool UnStar(string uuid)
    {
        foreach (var item in GameList)
        {
            if (item.IsNew)
            {
                continue;
            }
            else if (item.UUID == uuid)
            {
                GameList.Move(GameList.IndexOf(item), GameList.Count - 2);
                return true;
            }
        }

        return false;
    }

    public void MutAll()
    {
        foreach (var item in GameList)
        {
            item.IsCheck = true;
        }
    }

    public void SetMinMode(bool minMode)
    {
        MinMode = minMode;
        foreach (var item in _items.Values)
        {
            item.MinMode = minMode;
        }
    }
}
