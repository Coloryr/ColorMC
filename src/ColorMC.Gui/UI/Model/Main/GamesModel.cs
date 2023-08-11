using Avalonia.Input;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ColorMC.Gui.UI.Model.Main;

public partial class GamesModel : BaseModel
{
    public ObservableCollection<GameItemModel> GameList { get; init; } = new();

    public string Header { get; }
    public string Key { get; }

    [ObservableProperty]
    private bool _expander = true;

    private readonly IMainTop _top;
    private readonly Dictionary<string, GameItemModel> _items = new();

    public GamesModel(IUserControl con, IMainTop top, string key, string name,
        List<GameSettingObj> list) : base(con)
    {
        _top = top;
        GameList.Clear();
        _items.Clear();
        foreach (var item in list)
        {
            var model = new GameItemModel(con, _top, item);
            _items.Add(item.UUID, model);
            GameList.Add(model);
        }
        Header = name;
        Key = key;
    }

    public bool DropIn(IDataObject data)
    {
        if (data.Get(BaseBinding.DrapType) is not GameItemModel c)
            return false;
        if (_items.ContainsValue(c))
            return false;

        return true;
    }

    public void Drop(IDataObject data)
    {
        if (data.Get(BaseBinding.DrapType) is not GameItemModel c)
            return;

        c.IsDrop = false;

        if (_items.ContainsValue(c))
            return;

        GameBinding.MoveGameGroup(c.Obj, Key);
    }

    public GameItemModel? Find(string? uuid)
    {
        if (string.IsNullOrWhiteSpace(uuid))
            return null;
        if (_items.TryGetValue(uuid, out var item))
        {
            Expander = true;
            return item;
        }

        return null;
    }

    public void SetItems(List<GameSettingObj> list)
    {
        GameList.Clear();
        _items.Clear();
        foreach (var item in list)
        {
            var model = new GameItemModel(Control, _top, item);
            _items.Add(item.UUID, model);
            GameList.Add(model);
        }
    }

    public override void Close()
    {
        GameList.Clear();
    }
}
