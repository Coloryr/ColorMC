using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaEdit.Utils;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditTab4Model : GameEditTabModel
{
    public ObservableCollection<ModDisplayModel> ModList { get; init; } = new();
    public List<string> FilterList => BaseBinding.GetFilterName();

    private readonly List<ModDisplayModel> Items = new();

    [ObservableProperty]
    private ModDisplayModel item;

    [ObservableProperty]
    private string text;

    [ObservableProperty]
    private int filter;

    private bool isSet;

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
        BaseBinding.OpPath(Obj.GetModsPath());
    }

    [RelayCommand]
    public async void StartSet()
    {
        if (isSet)
            return;

        isSet = true;
        await App.ShowAddSet(Obj);
        isSet = false;
    }

    [RelayCommand]
    public async void Import()
    {
        var window = Con.Window;
        var file = await GameBinding.AddFile(window as Window, Obj, FileType.Mod);

        if (file == null)
            return;

        if (file == false)
        {
            window.ProgressInfo.Show(App.GetLanguage("GameEditWindow.Tab4.Error2"));
            return;
        }

        window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab4.Info2"));
        Load();
    }

    [RelayCommand]
    public async void Check()
    {
        var window = Con.Window;
        window.ProgressInfo.Show(App.GetLanguage("GameEditWindow.Tab4.Info10"));
        var res = await WebBinding.CheckModUpdate(Obj, Items);
        window.ProgressInfo.Close();
        if (res.Count > 0)
        {
            var res1 = await window.OkInfo.ShowWait(string.Format(
                App.GetLanguage("GameEditWindow.Tab4.Info11"), res.Count));
            if (res1)
            {
                window.ProgressInfo.Show(App.GetLanguage("GameEditWindow.Tab4.Info12"));
                await WebBinding.DownloadMod(Obj, res);
                window.ProgressInfo.Close();

                Load();
            }
        }
        else
        {
            window.OkInfo.Show(App.GetLanguage("GameEditWindow.Tab4.Info13"));
        }
    }

    [RelayCommand]
    public async void Load()
    {
        var window = Con.Window;
        window.ProgressInfo.Show(App.GetLanguage("GameEditWindow.Tab4.Info1"));
        Items.Clear();
        var res = await GameBinding.GetGameMods(Obj);
        window.ProgressInfo.Close();
        if (res == null)
        {
            window.OkInfo.Show(App.GetLanguage("GameEditWindow.Tab4.Error1"));
            return;
        }

        int count = 0;

        Items.AddRange(res);

        var list = res.Where(a => a.Obj.ReadFail == false && !a.Obj.Disable && !string.IsNullOrWhiteSpace(a.Obj.modid)).GroupBy(a => a.Obj.modid);
        count = list.Count(a => a.Count() > 1);
        if (count > 0)
        {
            window.OkInfo.Show(string.Format(App
                    .GetLanguage("GameEditWindow.Tab4.Info14"), count));
        }
        Load1();
    }

    [RelayCommand]
    public async void DependTest()
    {
        var window = Con.Window;
        window.ProgressInfo.Show(App.GetLanguage("GameEditWindow.Tab4.Info15"));
        var res = await GameBinding.ModCheck(Items);
        window.ProgressInfo.Close();
        if (res)
        {
            window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab4.Info16"));
        }
    }

    public async void Drop(IDataObject data)
    {
        var res = await GameBinding.AddFile(Obj, data, FileType.Mod);
        if (res)
        {
            Load();
        }
    }

    public async void Delete(IEnumerable<ModDisplayModel> items)
    {
        var window = Con.Window;
        var res = await window.OkInfo.ShowWait(
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

        window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab4.Info3"));
    }

    public async void Delete(ModDisplayModel item)
    {
        var window = Con.Window;
        var res = await window.OkInfo.ShowWait(
            string.Format(App.GetLanguage("GameEditWindow.Tab4.Info4"), item.Name));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteMod(item.Obj);
        ModList.Remove(item);

        window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab4.Info3"));
    }

    public void DisE()
    {
        DisE(Item);
    }

    public void DisE(ModDisplayModel item)
    {
        if (BaseBinding.IsGameRun(Obj))
        {
            return;
        }
        var res = GameBinding.ModEnDi(item.Obj);
        if (!res)
        {
            var window = Con.Window;
            window.ProgressInfo.Show(App.GetLanguage("GameEditWindow.Tab4.Error3"));
        }
        else
        {
            item.NotifyPropertyChanged(nameof(item.Local));
            item.Enable = item.Obj.Disable;
        }
    }

    private void Load1()
    {
        if (string.IsNullOrWhiteSpace(Text))
        {
            ModList.Clear();
            ModList.AddRange(Items);
        }
        else
        {
            string fil = Text.ToLower();
            switch (Filter)
            {
                case 0:
                    var list = from item in Items
                               where item.Name.ToLower().Contains(fil)
                               select item;
                    ModList.Clear();
                    ModList.AddRange(list);
                    break;
                case 1:
                    list = from item in Items
                           where item.Local.ToLower().Contains(fil)
                           select item;
                    ModList.Clear();
                    ModList.AddRange(list);
                    break;
                case 2:
                    list = from item in Items
                           where item.Author.ToLower().Contains(fil)
                           select item;
                    ModList.Clear();
                    ModList.AddRange(list);
                    break;
            }
        }
    }
}
