using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Avalonia.Input;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditModel : IModEdit
{
    public ObservableCollection<ModDisplayModel> ModList { get; init; } = [];
    public string[] ModFilterList { get; init; } = LanguageBinding.GetFilterName();

    private readonly List<ModDisplayModel> _modItems = [];

    [ObservableProperty]
    private ModDisplayModel _modItem;

    [ObservableProperty]
    private string _modText;

    [ObservableProperty]
    private int _modFilter;

    [ObservableProperty]
    private bool _displayModFilter = true;

    [ObservableProperty]
    private bool _displayModId = true;
    [ObservableProperty]
    private bool _displayModName = true;
    [ObservableProperty]
    private bool _displayModVersion = true;
    [ObservableProperty]
    private bool _displayModLoader = true;
    [ObservableProperty]
    private bool _displayModSide = true;
    [ObservableProperty]
    private bool _displayModText = true;

    [ObservableProperty]
    private bool _enableModText = true;

    private bool _isModSet;

    private GameGuiSettingObj _setting;

    partial void OnModTextChanged(string value)
    {
        LoadMod1();
    }

    partial void OnDisplayModTextChanged(bool value)
    {
        _setting.Mod.EnableText = value;
        GameGuiSetting.WriteConfig(_obj, _setting);
    }

    partial void OnDisplayModIdChanged(bool value)
    {
        _setting.Mod.EnableModId = value;
        GameGuiSetting.WriteConfig(_obj, _setting);
    }

    partial void OnDisplayModNameChanged(bool value)
    {
        _setting.Mod.EnableName = value;
        GameGuiSetting.WriteConfig(_obj, _setting);
    }

    partial void OnDisplayModVersionChanged(bool value)
    {
        _setting.Mod.EnableVersion = value;
        GameGuiSetting.WriteConfig(_obj, _setting);
    }

    partial void OnDisplayModLoaderChanged(bool value)
    {
        _setting.Mod.EnableLoader = value;
        GameGuiSetting.WriteConfig(_obj, _setting);
    }

    partial void OnDisplayModSideChanged(bool value)
    {
        _setting.Mod.EnableSide = value;
        GameGuiSetting.WriteConfig(_obj, _setting);
    }

    partial void OnModFilterChanged(int value)
    {
        EnableModText = value switch
        {
            <= 3 => true,
            _ => false
        };

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
    public void LoadMod()
    {
        LoadMods();
    }

    private void LoadSetting()
    {
        _setting = GameGuiSetting.ReadConfig(_obj);
#pragma warning disable MVVMTK0034
        _displayModText = _setting.Mod.EnableText;
        _displayModId = _setting.Mod.EnableModId;
        _displayModName = _setting.Mod.EnableName;
        _displayModVersion = _setting.Mod.EnableVersion;
        _displayModLoader = _setting.Mod.EnableLoader;
        _displayModSide = _setting.Mod.EnableSide;
#pragma warning restore MVVMTK0034
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
            Model.Notify(string.Format(App.Lang("GameEditWindow.Tab4.Info20"), res1.Data));
        }
        _isModSet = false;
    }

    /// <summary>
    /// 检查模组更新
    /// </summary>
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
                WebBinding.UpgradeMod(_obj, res);
                Model.Notify(App.Lang("GameEditWindow.Tab4.Info22"));
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
        LoadMods();
    }

    public async void DropMod(IDataObject data)
    {
        var res = await GameBinding.AddFile(_obj, data, FileType.Mod);
        if (res)
        {
            LoadMods();
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
            Model.Notify(App.Lang("GameEditWindow.Tab4.Error6"));
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
        ModFilter = 3;
        var builder = new StringBuilder();
        foreach (var item in list)
        {
            builder.Append(item).Append(',');
        }
        ModText = builder.ToString();
    }

    public async void LoadMods()
    {
        Model.Progress(App.Lang("GameEditWindow.Tab4.Info1"));
        _modItems.Clear();
        var res = await GameBinding.GetGameMods(_obj, this);
        Model.ProgressClose();
        if (res == null)
        {
            Model.Show(App.Lang("GameEditWindow.Tab4.Error1"));
            return;
        }

        int count = 0;

        _modItems.AddRange(res);

        var list = res.Where(a => a.Obj.ReadFail == false && !a.Obj.Disable
            && !string.IsNullOrWhiteSpace(a.Obj.ModId)).GroupBy(a => a.Obj.ModId);
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

        foreach (var item in _modItems)
        {
            if (_setting.ModName.TryGetValue(item.Obj.Sha1, out var temp))
            {
                item.Text = temp;
            }
        }

        Model.Notify(App.Lang("GameEditWindow.Tab4.Info23"));

        LoadMod1();
    }

    public void EditModText(ModDisplayModel item)
    {
        if (!_setting.ModName.TryAdd(item.Obj.Sha1, item.Text))
        {
            _setting.ModName[item.Obj.Sha1] = item.Text;
        }

        GameGuiSetting.WriteConfig(_obj, _setting);
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
            var args = fil.Split(',').ToList();
            ModList.Clear();
            switch (ModFilter)
            {
                case 0:
                    foreach (var item in _modItems)
                    {
                        foreach (var item1 in args)
                        {
                            if (string.IsNullOrWhiteSpace(item1))
                            {
                                continue;
                            }
                            if (item.Name.Contains(item1, StringComparison.OrdinalIgnoreCase))
                            {
                                ModList.Add(item);
                                break;
                            }
                        }
                    }
                    break;
                case 1:
                    foreach (var item in _modItems)
                    {
                        foreach (var item1 in args)
                        {
                            if (string.IsNullOrWhiteSpace(item1))
                            {
                                continue;
                            }
                            if (item.Local.Contains(item1, StringComparison.OrdinalIgnoreCase))
                            {
                                ModList.Add(item);
                                break;
                            }
                        }
                    }
                    break;
                case 2:
                    foreach (var item in _modItems)
                    {
                        foreach (var item1 in args)
                        {
                            if (string.IsNullOrWhiteSpace(item1))
                            {
                                continue;
                            }
                            if (item.Author.Contains(item1, StringComparison.OrdinalIgnoreCase))
                            {
                                ModList.Add(item);
                                break;
                            }
                        }
                    }
                    break;
                case 3:
                    foreach (var item in _modItems)
                    {
                        foreach (var item1 in args)
                        {
                            if (string.IsNullOrWhiteSpace(item1))
                            {
                                continue;
                            }
                            if (item.Modid.Contains(item1, StringComparison.OrdinalIgnoreCase))
                            {
                                ModList.Add(item);
                                break;
                            }
                        }
                    }
                    break;
                case 4:
                    foreach (var item in _modItems)
                    {
                        if (item.Enable != true)
                        {
                            continue;
                        }
                        ModList.Add(item);
                    }
                    break;
                case 5:
                    foreach (var item in _modItems)
                    {
                        if (item.Enable != false)
                        {
                            continue;
                        }
                        ModList.Add(item);
                    }
                    break;
            }
        }
    }
}
