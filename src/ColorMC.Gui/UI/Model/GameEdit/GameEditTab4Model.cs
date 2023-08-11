using Avalonia.Input;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditTab4Model : GameModel
{
    public ObservableCollection<ModDisplayModel> ModList { get; init; } = new();
    public List<string> FilterList { get; init; } = LanguageBinding.GetFilterName();

    private readonly List<ModDisplayModel> _items = new();

    [ObservableProperty]
    private ModDisplayModel _item;

    [ObservableProperty]
    private string _text;

    [ObservableProperty]
    private int _filter;

    private bool _isSet;

    public GameEditTab4Model(IUserControl con, GameSettingObj obj) : base(con, obj)
    {

    }

    partial void OnTextChanged(string value)
    {
        Load1();
    }

    partial void OnFilterChanged(int value)
    {
        Load1();
    }

    [RelayCommand]
    public void Add()
    {
        App.ShowAdd(Obj, FileType.Mod);
    }

    [RelayCommand]
    public void Open()
    {
        PathBinding.OpPath(Obj, PathType.ModPath);
    }

    [RelayCommand]
    public async Task StartSet()
    {
        if (_isSet)
            return;

        _isSet = true;
        await App.ShowAddSet(Obj);
        _isSet = false;
    }

    [RelayCommand]
    public async Task Import()
    {
        var file = await PathBinding.AddFile(Window, Obj, FileType.Mod);

        if (file == null)
            return;

        if (file == false)
        {
            Progress(App.GetLanguage("GameEditWindow.Tab4.Error2"));
            return;
        }

        Notify(App.GetLanguage("GameEditWindow.Tab4.Info2"));
        await Load();
    }

    [RelayCommand]
    public async Task Check()
    {
        Progress(App.GetLanguage("GameEditWindow.Tab4.Info10"));
        var res = await WebBinding.CheckModUpdate(Obj, _items);
        ProgressClose();
        if (res.Count > 0)
        {
            var res1 = await ShowWait(string.Format(
                App.GetLanguage("GameEditWindow.Tab4.Info11"), res.Count));
            if (res1)
            {
                Progress(App.GetLanguage("GameEditWindow.Tab4.Info12"));
                await WebBinding.DownloadMod(Obj, res);
                ProgressClose();

                await Load();
            }
        }
        else
        {
            Show(App.GetLanguage("GameEditWindow.Tab4.Info13"));
        }
    }

    [RelayCommand]
    public async Task Load()
    {
        Progress(App.GetLanguage("GameEditWindow.Tab4.Info1"));
        _items.Clear();
        var res = await GameBinding.GetGameMods(Obj);
        ProgressClose();
        if (res == null)
        {
            Show(App.GetLanguage("GameEditWindow.Tab4.Error1"));
            return;
        }

        int count = 0;

        _items.AddRange(res);

        var list = res.Where(a => a.Obj.ReadFail == false && !a.Obj.Disable && !string.IsNullOrWhiteSpace(a.Obj.modid)).GroupBy(a => a.Obj.modid);
        count = list.Count(a => a.Count() > 1);
        if (count > 0)
        {
            Show(string.Format(App
                     .GetLanguage("GameEditWindow.Tab4.Info14"), count));
        }
        Load1();
    }

    [RelayCommand]
    public async Task DependTest()
    {
        Progress(App.GetLanguage("GameEditWindow.Tab4.Info15"));
        var res = await GameBinding.ModCheck(_items);
        ProgressClose();
        if (res)
        {
            Notify(App.GetLanguage("GameEditWindow.Tab4.Info16"));
        }
    }

    public async void Drop(IDataObject data)
    {
        var res = await GameBinding.AddFile(Obj, data, FileType.Mod);
        if (res)
        {
            await Load();
        }
    }

    public async void Delete(IEnumerable<ModDisplayModel> items)
    {
        var res = await ShowWait(
            string.Format(App.GetLanguage("GameEditWindow.Tab4.Info9"), items.Count()));
        if (!res)
        {
            return;
        }

        items.ToList().ForEach(item =>
        {
            GameBinding.DeleteMod(item.Obj);
            ModList.Remove(item);
        });

        Notify(App.GetLanguage("GameEditWindow.Tab4.Info3"));
    }

    public async void Delete(ModDisplayModel item)
    {
        var res = await ShowWait(
            string.Format(App.GetLanguage("GameEditWindow.Tab4.Info4"), item.Name));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteMod(item.Obj);
        ModList.Remove(item);

        Notify(App.GetLanguage("GameEditWindow.Tab4.Info3"));
    }

    public void DisE()
    {
        DisE(Item);
    }

    public async void DisE(ModDisplayModel item)
    {
        if (BaseBinding.IsGameRun(Obj))
        {
            return;
        }
        var res = GameBinding.ModEnDi(item.Obj);
        if (!res)
        {
            Progress(App.GetLanguage("GameEditWindow.Tab4.Error3"));
        }
        else
        {
            item.LocalChange();
            item.Enable = !item.Obj.Disable;
            if (item.Enable)
            {
                return;
            }

            var list = GameBinding.ModDisable(item, _items);

            if (list.Count == 0)
            {
                return;
            }

            res = await ShowWait(
                string.Format(App.GetLanguage("GameEditWindow.Tab4.Info17"), list.Count));
            if (res)
            {
                foreach (var item1 in list)
                {
                    if (!item1.Enable)
                    {
                        continue;
                    }
                    GameBinding.ModEnDi(item1.Obj);
                    item1.LocalChange();
                    item1.Enable = !item1.Obj.Disable;
                }
            }
        }
    }

    private void Load1()
    {
        if (string.IsNullOrWhiteSpace(Text))
        {
            ModList.Clear();
            ModList.AddRange(_items);
        }
        else
        {
            string fil = Text.ToLower();
            switch (Filter)
            {
                case 0:
                    var list = from item in _items
                               where item.Name.ToLower().Contains(fil)
                               select item;
                    ModList.Clear();
                    ModList.AddRange(list);
                    break;
                case 1:
                    list = from item in _items
                           where item.Local.ToLower().Contains(fil)
                           select item;
                    ModList.Clear();
                    ModList.AddRange(list);
                    break;
                case 2:
                    list = from item in _items
                           where item.Author.ToLower().Contains(fil)
                           select item;
                    ModList.Clear();
                    ModList.AddRange(list);
                    break;
            }
        }
    }

    public override void Close()
    {
        ModList.Clear();
        _items.Clear();
        _item = null;
    }
}
