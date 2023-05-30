using Avalonia.Input;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ColorMC.Gui.UI.Model.Main;

public partial class GamesModel : ObservableObject
{
    private readonly IUserControl Con;
    private readonly IMainTop Top;
    public ObservableCollection<GameModel> GameList { get; init; } = new();

    private readonly Dictionary<string, GameModel> Items = new();

    public string Header { get; }
    public string Key { get; }

    [ObservableProperty]
    private bool expander = true;

    public GamesModel(IUserControl con, IMainTop top, string key, string name, List<GameSettingObj> list)
    {
        Top = top;
        Con = con;
        GameList.Clear();
        Items.Clear();
        foreach (var item in list)
        {
            var model = new GameModel(Con, Top, item);
            Items.Add(item.UUID, model);
            GameList.Add(model);
        }
        Header = name;
        Key = key;
    }

    public bool DropIn(IDataObject data)
    {
        if (data.Get(BaseBinding.DrapType) is not GameModel c)
            return false;
        if (Items.ContainsValue(c))
            return false;

        return true;
    }

    public void Drop(IDataObject data)
    {
        if (data.Get(BaseBinding.DrapType) is not GameModel c)
            return;

        c.IsDrop = false;

        if (Items.ContainsValue(c))
            return;

        GameBinding.MoveGameGroup(c.Obj, Key);
    }

    public GameModel? Find(string? uuid)
    {
        if (string.IsNullOrWhiteSpace(uuid))
            return null;
        if (Items.TryGetValue(uuid, out var item))
        {
            Expander = true;
            return item;
        }

        return null;
    }

    public void SetItems(List<GameSettingObj> list)
    {
        GameList.Clear();
        Items.Clear();
        foreach (var item in list)
        {
            var model = new GameModel(Con, Top, item);
            Items.Add(item.UUID, model);
            GameList.Add(model);
        }
    }
}
