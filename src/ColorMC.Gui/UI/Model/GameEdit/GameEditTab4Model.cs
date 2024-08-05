using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditModel
{
    public ObservableCollection<ModDisplayModel> ModList { get; init; } = [];
    public string[] ModFilterList { get; init; } = LanguageBinding.GetFilterName();

    private readonly List<ModDisplayModel> _modItems = [];

    [ObservableProperty]
    private ModDisplayModel _modItem;

    [ObservableProperty]
    private string _modText;

    [ObservableProperty]
    private int modFilter;

    [ObservableProperty]
    private bool _displayModFilter = true;

    private bool _isModSet;

    partial void OnModTextChanged(string value)
    {
        LoadMod1();
    }

    partial void OnModFilterChanged(int value)
    {
        LoadMod1();
    }

    [RelayCommand]
    public void ShowModFilter()
    {
        DisplayModFilter = !DisplayModFilter;
    }

    [RelayCommand]
    public void AddMod()
    {
        WindowManager.ShowAdd(_obj, FileType.Mod);
    }

    [RelayCommand]
    public void OpenMod()
    {
        PathBinding.OpenPath(_obj, PathType.ModPath);
    }

    [RelayCommand]
    public async Task LoadMod()
    {
        Model.Progress(App.Lang("GameEditWindow.Tab4.Info1"));
        _modItems.Clear();
        var res = await GameBinding.GetGameMods(_obj);
        Model.ProgressClose();
        if (res == null)
        {
            Model.Show(App.Lang("GameEditWindow.Tab4.Error1"));
            return;
        }

        int count = 0;

        _modItems.AddRange(res);

        var list = res.Where(a => a.Obj.ReadFail == false && !a.Obj.Disable && !string.IsNullOrWhiteSpace(a.Obj.modid)).GroupBy(a => a.Obj.modid);
        var list1 = new List<string>();

        foreach (var item in list)
        {
            if (item.Count() > 1)
            {
                count++;
                list1.Add(item.Key);
            }
        }
        if (list1.Count != 0)
        {
            var res1 = await Model.ShowWait(string.Format(App
                     .Lang("GameEditWindow.Tab4.Info14"), count));
            if (res1)
            {
                DisplayMod(list1);
                return;
            }
        }

        LoadMod1();
    }

    private async void DependTestMod()
    {
        Model.Progress(App.Lang("GameEditWindow.Tab4.Info15"));
        var res = await GameBinding.ModCheck(_modItems);
        Model.ProgressClose();
        if (res)
        {
            Model.Notify(App.Lang("GameEditWindow.Tab4.Info16"));
        }
    }

    public async void StartSetMod()
    {
        if (_isModSet)
            return;

        _isModSet = true;
        await WindowManager.ShowAddSet(_obj);
        _isModSet = false;
    }

    private async void StartAutoSetMod()
    {
        if (_isModSet)
            return;

        var res = await Model.ShowWait(App.Lang("GameEditWindow.Tab4.Info18"));

        _isModSet = true;
        Model.Progress(App.Lang("GameEditWindow.Tab4.Info19"));
        var res1 = await GameBinding.AutoMarkMods(_obj, res);
        Model.ProgressClose();
        if (!res1.State)
        {
            Model.Show(string.Format(App.Lang("GameEditWindow.Tab4.Error4"), res1.Data));
        }
        else
        {
            Model.Show(string.Format(App.Lang("GameEditWindow.Tab4.Info20"), res1.Data));
        }
        _isModSet = false;
    }
    private async void CheckMod()
    {
        Model.Progress(App.Lang("GameEditWindow.Tab4.Info10"));
        var res = await WebBinding.CheckModUpdate(_obj, _modItems);
        Model.ProgressClose();
        if (res.Count > 0)
        {
            var res1 = await Model.ShowWait(string.Format(
                App.Lang("GameEditWindow.Tab4.Info11"), res.Count));
            if (res1)
            {
                Model.Progress(App.Lang("GameEditWindow.Tab4.Info12"));
                await WebBinding.DownloadMod(_obj, res);
                Model.ProgressClose();

                await LoadMod();
            }
        }
        else
        {
            Model.Show(App.Lang("GameEditWindow.Tab4.Info13"));
        }
    }

    private async void ImportMod()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var res = await PathBinding.AddFile(top, _obj, FileType.Schematic);

        if (res == null)
            return;

        if (res == false)
        {
            Model.Show(App.Lang("GameEditWindow.Tab11.Error1"));
            return;
        }

        Model.Show(App.Lang("GameEditWindow.Tab11.Info1"));
        LoadSchematic();
        var file = await PathBinding.AddFile(top, _obj, FileType.Mod);

        if (file == null)
            return;

        if (file == false)
        {
            Model.Progress(App.Lang("GameEditWindow.Tab4.Error2"));
            return;
        }

        Model.Notify(App.Lang("GameEditWindow.Tab4.Info2"));
        await LoadMod();
    }

    public async void DropMod(IDataObject data)
    {
        var res = await GameBinding.AddFile(_obj, data, FileType.Mod);
        if (res)
        {
            await LoadMod();
        }
    }

    public async void DeleteMod(IEnumerable<ModDisplayModel> items)
    {
        var res = await Model.ShowWait(
            string.Format(App.Lang("GameEditWindow.Tab4.Info9"), items.Count()));
        if (!res)
        {
            return;
        }

        items.ToList().ForEach(item =>
        {
            GameBinding.DeleteMod(item.Obj);
            ModList.Remove(item);
        });

        Model.Notify(App.Lang("GameEditWindow.Tab4.Info3"));
    }

    public async void DeleteMod(ModDisplayModel item)
    {
        var res = await Model.ShowWait(
            string.Format(App.Lang("GameEditWindow.Tab4.Info4"), item.Name));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteMod(item.Obj);
        ModList.Remove(item);

        Model.Notify(App.Lang("GameEditWindow.Tab4.Info3"));
    }

    public void DisEMod()
    {
        DisEMod(ModItem);
    }

    public async void DisEMod(ModDisplayModel item)
    {
        if (item == null)
        {
            return;
        }
        if (GameManager.IsGameRun(_obj))
        {
            return;
        }
        var res = GameBinding.ModEnableDisable(item.Obj);
        if (!res.State)
        {
            Model.Show(res.Message!);
        }
        else
        {
            item.LocalChange();
            item.Enable = !item.Obj.Disable;
            if (item.Enable)
            {
                return;
            }

            var list = GameBinding.ModDisable(item, _modItems);

            foreach (var item1 in list.ToArray())
            {
                if (item1.Enable == false)
                {
                    list.Remove(item1);
                }
            }

            if (list.Count == 0)
            {
                return;
            }

            if (await Model.ShowWait(
                string.Format(App.Lang("GameEditWindow.Tab4.Info17"), list.Count)))
            {
                foreach (var item1 in list)
                {
                    if (!item1.Enable)
                    {
                        continue;
                    }
                    GameBinding.ModEnableDisable(item1.Obj);
                    item1.LocalChange();
                    item1.Enable = !item1.Obj.Disable;
                }
            }
        }
    }

    private void DisplayMod(List<string> list)
    {
        ModList.Clear();
        foreach (var item in list)
        {
            var list1 = from item1 in _modItems
                        where item1.Modid == item
                        select item1;
            ModList.AddRange(list1);
        }
    }

    private void LoadMod1()
    {
        if (string.IsNullOrWhiteSpace(ModText))
        {
            ModList.Clear();
            ModList.AddRange(_modItems);
        }
        else
        {
            string fil = ModText.ToLower();
            switch (ModFilter)
            {
                case 0:
                    var list = from item in _modItems
                               where item.Name.Contains(fil, StringComparison.OrdinalIgnoreCase)
                               select item;
                    ModList.Clear();
                    ModList.AddRange(list);
                    break;
                case 1:
                    list = from item in _modItems
                           where item.Local.Contains(fil, StringComparison.OrdinalIgnoreCase)
                           select item;
                    ModList.Clear();
                    ModList.AddRange(list);
                    break;
                case 2:
                    list = from item in _modItems
                           where item.Author.Contains(fil, StringComparison.OrdinalIgnoreCase)
                           select item;
                    ModList.Clear();
                    ModList.AddRange(list);
                    break;
                case 3:
                    list = from item in _modItems
                           where item.Modid?.Contains(fil, StringComparison.OrdinalIgnoreCase) == true
                           select item;
                    ModList.Clear();
                    ModList.AddRange(list);
                    break;
            }
        }
    }
}
