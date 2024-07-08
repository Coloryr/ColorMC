using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Main;

public partial class GameGroupModel : TopModel
{
    public ObservableCollection<GameItemModel> GameList { get; init; } = [];

    public string Header { get; }
    public string Key { get; }

    [ObservableProperty]
    private bool _expander = true;

    private readonly IMainTop _top;
    private readonly Dictionary<string, GameItemModel> _items = [];
    private readonly GameItemModel _addItem;

    public GameGroupModel(BaseModel model, IMainTop top, string key, string name,
        List<GameSettingObj> list) : base(model)
    {
        _top = top;
        GameList.Clear();
        _items.Clear();
        foreach (var item in list)
        {
            var model1 = new GameItemModel(Model, _top, item);
            _items.Add(item.UUID, model1);
        }
        _addItem = new(Model, Key == InstancesPath.DefaultGroup ? null : Key);
        Task.Run(() =>
        {
            foreach (var item in _items)
            {
                Thread.Sleep(50);
                Dispatcher.UIThread.Invoke(() =>
                {
                    GameList.Add(item.Value);
                });
            }

            Dispatcher.UIThread.Post(() =>
            {
                GameList.Add(_addItem);
            });
        });

        Header = name;
        Key = key;
    }

    public bool DropIn(IDataObject data)
    {
        return data.Get(BaseBinding.DrapType) is not GameItemModel c
            || !_items.ContainsValue(c);
    }

    public void Drop(IDataObject data)
    {
        if (data.Get(BaseBinding.DrapType) is not GameItemModel c)
        {
            return;
        }

        c.IsDrop = false;

        if (_items.ContainsValue(c))
        {
            return;
        }

        GameBinding.MoveGameGroup(c.Obj, Key);
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
            var model1 = new GameItemModel(Model, _top, item);
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
}
