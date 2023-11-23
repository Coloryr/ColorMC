using Avalonia.Input;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ColorMC.Gui.UI.Model.Main;

public partial class GamesModel : TopModel
{
    public ObservableCollection<GameItemModel> GameList { get; init; } = new();

    public string Header { get; }
    public string Key { get; }

    [ObservableProperty]
    private bool _expander = true;

    private readonly IMainTop _top;
    private readonly Dictionary<string, GameItemModel> _items = new();

    public GamesModel(BaseModel model, IMainTop top, string key, string name,
        List<GameSettingObj> list) : base(model)
    {
        _top = top;
        GameList.Clear();
        _items.Clear();
        foreach (var item in list)
        {
            var model1 = new GameItemModel(model, _top, item);
            _items.Add(item.UUID, model1);
            GameList.Add(model1);
        }
        GameList.Add(new(model));
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
        foreach (var item in GameList)
        {
            item.TopClose();
        }
        GameList.Clear();
        _items.Clear();
        foreach (var item in list)
        {
            var model = new GameItemModel(Model, _top, item);
            _items.Add(item.UUID, model);
            GameList.Add(model);
        }
        GameList.Add(new(Model));
    }

    protected override void Close()
    {
        foreach (var item in GameList)
        {
            item.TopClose();
        }
        GameList.Clear();
        _items.Clear();
    }
}
