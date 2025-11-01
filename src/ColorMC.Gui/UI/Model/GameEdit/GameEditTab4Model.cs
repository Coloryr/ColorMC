using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Avalonia.Input;
using AvaloniaEdit.Utils;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Objs.Config;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.GameEdit;

/// <summary>
/// 游戏实例编辑
/// </summary>
public partial class GameEditModel
{
    /// <summary>
    /// 显示的模组列表
    /// </summary>
    public ObservableCollection<ModDisplayModel> ModList { get; init; } = [];
    /// <summary>
    /// 过滤器列表
    /// </summary>
    public string[] ModFilterList { get; init; } = LanguageBinding.GetFilterName();

    /// <summary>
    /// 模组列表
    /// </summary>
    private readonly List<ModDisplayModel> _modItems = [];

    /// <summary>
    /// 选中的模组
    /// </summary>
    [ObservableProperty]
    private ModDisplayModel _modItem;

    /// <summary>
    /// 模组筛选
    /// </summary>
    [ObservableProperty]
    private string _modText;

    /// <summary>
    /// 选中的模组过滤器
    /// </summary>
    [ObservableProperty]
    private ModFilterType _modFilter;
    /// <summary>
    /// 是否显示modid列
    /// </summary>
    [ObservableProperty]
    private bool _displayModId = true;
    /// <summary>
    /// 是否显示模组名列
    /// </summary>
    [ObservableProperty]
    private bool _displayModName = true;
    /// <summary>
    /// 是否显示模组版本列
    /// </summary>
    [ObservableProperty]
    private bool _displayModVersion = true;
    /// <summary>
    /// 是否显示模组加载器列
    /// </summary>
    [ObservableProperty]
    private bool _displayModLoader = true;
    /// <summary>
    /// 是否显示模组支持侧
    /// </summary>
    [ObservableProperty]
    private bool _displayModSide = true;
    /// <summary>
    /// 是否显示模组备注
    /// </summary>
    [ObservableProperty]
    private bool _displayModText = true;
    /// <summary>
    /// 是否显示模组筛选
    /// </summary>
    [ObservableProperty]
    private bool _enableModText = true;

    /// <summary>
    /// 是否在模组标记模式中
    /// </summary>
    private bool _isModSet;

    /// <summary>
    /// Gui设置
    /// </summary>
    private readonly GameGuiSettingObj _setting;

    partial void OnModTextChanged(string value)
    {
        LoadModDisplay();
    }

    partial void OnDisplayModTextChanged(bool value)
    {
        _setting.Mod.EnableText = value;
        GameManager.WriteConfig(_obj, _setting);
    }

    partial void OnDisplayModIdChanged(bool value)
    {
        _setting.Mod.EnableModId = value;
        GameManager.WriteConfig(_obj, _setting);
    }

    partial void OnDisplayModNameChanged(bool value)
    {
        _setting.Mod.EnableName = value;
        GameManager.WriteConfig(_obj, _setting);
    }

    partial void OnDisplayModVersionChanged(bool value)
    {
        _setting.Mod.EnableVersion = value;
        GameManager.WriteConfig(_obj, _setting);
    }

    partial void OnDisplayModLoaderChanged(bool value)
    {
        _setting.Mod.EnableLoader = value;
        GameManager.WriteConfig(_obj, _setting);
    }

    partial void OnDisplayModSideChanged(bool value)
    {
        _setting.Mod.EnableSide = value;
        GameManager.WriteConfig(_obj, _setting);
    }

    partial void OnModFilterChanged(ModFilterType value)
    {
        EnableModText = value switch
        {
            <= ModFilterType.Modid => true,
            _ => false
        };

        LoadModDisplay();
    }

    /// <summary>
    /// 下载模组
    /// </summary>
    [RelayCommand]
    public void AddMod()
    {
        WindowManager.ShowAdd(_obj, FileType.Mod);
    }

    /// <summary>
    /// 打开模组路径
    /// </summary>
    [RelayCommand]
    public void OpenMod()
    {
        PathBinding.OpenPath(_obj, PathType.ModPath);
    }

    /// <summary>
    /// 加载模组列表
    /// </summary>
    [RelayCommand]
    public void LoadMod()
    {
        LoadMods();
    }

    /// <summary>
    /// 测试模组依赖
    /// </summary>
    private async void DependTestMod()
    {
        Model.Progress(LanguageUtils.Get("GameEditWindow.Tab4.Info15"));
        var res = await GameBinding.ModCheckAsync(_modItems);
        Model.ProgressClose();
        if (res)
        {
            Model.Notify(LanguageUtils.Get("GameEditWindow.Tab4.Info16"));
        }
    }

    /// <summary>
    /// 开始标记模组
    /// </summary>
    public async void StartSetMod()
    {
        var res = await Model.InputAsync(LanguageUtils.Get("GameEditWindow.Tab4.Info24"),
            LanguageUtils.Get("GameEditWindow.Tab4.Info25"));
        if (res.Cancel)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(res.Text1) && string.IsNullOrWhiteSpace(res.Text2))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(res.Text1) || string.IsNullOrWhiteSpace(res.Text2))
        {
            Model.Show(LanguageUtils.Get("GameEditWindow.Tab4.Error8"));
            return;
        }

        Model.Progress(LanguageUtils.Get("GameEditWindow.Tab4.Info26"));
        var res1 = await GameBinding.MarkModsAsync(_obj, res.Text1, res.Text2);
        Model.ProgressClose();
        if (!res1)
        {
            Model.Show(LanguageUtils.Get("GameEditWindow.Tab4.Error9"));
            return;
        }
        else
        {
            Model.Notify(LanguageUtils.Get("GameEditWindow.Tab4.Info27"));
        }
    }

    /// <summary>
    /// 自动标记模组
    /// </summary>
    private async void StartAutoSetMod()
    {
        if (_isModSet)
        {
            return;
        }

        var res = await Model.ShowAsync(LanguageUtils.Get("GameEditWindow.Tab4.Info18"));

        _isModSet = true;
        Model.Progress(LanguageUtils.Get("GameEditWindow.Tab4.Info19"));
        var res1 = await ModrinthHelper.AutoMarkAsync(_obj, res);
        Model.ProgressClose();
        if (!res1.State)
        {
            Model.Show(string.Format(LanguageUtils.Get("GameEditWindow.Tab4.Error4"), res1.Data));
        }
        else
        {
            Model.Notify(string.Format(LanguageUtils.Get("GameEditWindow.Tab4.Info20"), res1.Data));
        }
        _isModSet = false;
    }

    /// <summary>
    /// 检查模组更新
    /// </summary>
    private async void CheckMod()
    {
        Model.Progress(LanguageUtils.Get("GameEditWindow.Tab4.Info10"));
        var res = await WebBinding.CheckModUpdateAsync(_obj, _modItems);
        Model.ProgressClose();
        if (res.Count > 0)
        {
            var res1 = await Model.ShowAsync(string.Format(
                LanguageUtils.Get("GameEditWindow.Tab4.Info11"), res.Count));
            if (res1)
            {
                WebBinding.UpgradeMod(_obj, res);
                Model.Notify(LanguageUtils.Get("GameEditWindow.Tab4.Info22"));
            }
        }
        else
        {
            Model.Show(LanguageUtils.Get("GameEditWindow.Tab4.Info13"));
        }
    }

    /// <summary>
    /// 导入模组
    /// </summary>
    private async void ImportMod()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var res = await PathBinding.AddFileAsync(top, _obj, FileType.Schematic);

        if (res == null)
            return;

        if (res == false)
        {
            Model.Show(LanguageUtils.Get("GameEditWindow.Tab11.Error1"));
            return;
        }

        Model.Show(LanguageUtils.Get("GameEditWindow.Tab11.Info1"));
        LoadSchematic();
        var file = await PathBinding.AddFileAsync(top, _obj, FileType.Mod);

        if (file == null)
            return;

        if (file == false)
        {
            Model.Progress(LanguageUtils.Get("GameEditWindow.Tab4.Error2"));
            return;
        }

        Model.Notify(LanguageUtils.Get("GameEditWindow.Tab4.Info2"));
        LoadMods();
    }

    /// <summary>
    /// 拖拽导入模组
    /// </summary>
    /// <param name="data"></param>
    public async void DropMod(IDataTransfer data)
    {
        var res = await GameBinding.AddFileAsync(_obj, data, FileType.Mod);
        if (res)
        {
            LoadMods();
        }
    }

    /// <summary>
    /// 删除模组
    /// </summary>
    /// <param name="items">一些模组</param>
    public async void DeleteMod(IEnumerable<ModDisplayModel> items)
    {
        var res = await Model.ShowAsync(
            string.Format(LanguageUtils.Get("GameEditWindow.Tab4.Info9"), items.Count()));
        if (!res)
        {
            return;
        }

        bool error = false;

        items.ToList().ForEach(item =>
        {
            try
            {
                GameBinding.DeleteMod(item.Obj);
                ModList.Remove(item);
            }
            catch
            {
                error = true;
            }
        });

        if (error)
        {
            Model.Show(LanguageUtils.Get("GameEditWindow.Tab4.Error7"));
            return;
        }

        Model.Notify(LanguageUtils.Get("GameEditWindow.Tab4.Info3"));
    }

    /// <summary>
    /// 删除模组
    /// </summary>
    /// <param name="item">模组</param>
    public async void DeleteMod(ModDisplayModel item)
    {
        var res = await Model.ShowAsync(
            string.Format(LanguageUtils.Get("GameEditWindow.Tab4.Info4"), item.Name));
        if (!res)
        {
            return;
        }

        try
        {
            GameBinding.DeleteMod(item.Obj);
        }
        catch
        {
            Model.Show(LanguageUtils.Get("GameEditWindow.Tab4.Error7"));
            return;
        }
        ModList.Remove(item);

        Model.Notify(LanguageUtils.Get("GameEditWindow.Tab4.Info3"));
    }

    /// <summary>
    /// 禁用/启用模组
    /// </summary>
    public void DisEMod()
    {
        DisEMod(ModItem);
    }

    /// <summary>
    /// 禁用/启用模组
    /// </summary>
    /// <param name="item">模组</param>
    public async void DisEMod(ModDisplayModel item)
    {
        if (item == null)
        {
            return;
        }
        if (GameManager.IsGameRun(_obj))
        {
            Model.Notify(LanguageUtils.Get("GameEditWindow.Tab4.Error6"));
            return;
        }
        var res = GameBinding.ModEnableDisable(item.Obj);
        if (!res.State)
        {
            Model.Show(res.Data!);
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

            //自动禁用依赖的模组
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

            if (await Model.ShowAsync(
                string.Format(LanguageUtils.Get("GameEditWindow.Tab4.Info17"), list.Count)))
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

    /// <summary>
    /// 显示模组信息
    /// </summary>
    /// <param name="list"></param>
    private void DisplayMod(List<string> list)
    {
        ModList.Clear();
        ModFilter = ModFilterType.Modid;
        var builder = new StringBuilder();
        foreach (var item in list)
        {
            builder.Append(item).Append(',');
        }
        ModText = builder.ToString();
    }

    /// <summary>
    /// 加载模组列表
    /// </summary>
    public async void LoadMods()
    {
        Model.Progress(LanguageUtils.Get("GameEditWindow.Tab4.Info1"));
        _modItems.Clear();
        var res = await GameBinding.GetGameModsAsync(_obj);
        Model.ProgressClose();
        if (res == null)
        {
            Model.Show(LanguageUtils.Get("GameEditWindow.Tab4.Error1"));
            return;
        }

        res.ForEach(item =>
        {
            var obj1 = _obj.Mods.Values.FirstOrDefault(a => a.Sha1 == item.Sha1);
            _modItems.Add(new ModDisplayModel(item, obj1, this));
        });

        int count = 0;

        //自动处理modid冲突
        var list = res.Where(a => a.ReadFail == false && !a.Disable
            && !string.IsNullOrWhiteSpace(a.ModId)).GroupBy(a => a.ModId);
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
            var res1 = await Model.ShowAsync(string.Format(App
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

        Model.Notify(LanguageUtils.Get("GameEditWindow.Tab4.Info23"));

        LoadModDisplay();
    }

    /// <summary>
    /// 编辑模组注释
    /// </summary>
    /// <param name="item"></param>
    public void EditModText(ModDisplayModel item)
    {
        if (!_setting.ModName.TryAdd(item.Obj.Sha1, item.Text))
        {
            _setting.ModName[item.Obj.Sha1] = item.Text;
        }

        GameManager.WriteConfig(_obj, _setting);
    }

    /// <summary>
    /// 加载模组列表
    /// </summary>
    private void LoadModDisplay()
    {
        ModList.Clear();
        switch (ModFilter)
        {
            case ModFilterType.Enabled:
                ModList.AddRange(_modItems.Where(item => item.Enable));
                return;
            case ModFilterType.Disabled:
                ModList.AddRange(_modItems.Where(item => !item.Enable));
                return;
            case ModFilterType.Newer:
                ModList.AddRange(_modItems.Where(item => item.IsNew));
                return;
        }
        //没有筛选内容
        if (string.IsNullOrWhiteSpace(ModText))
        {
            ModList.AddRange(_modItems);
            return;
        }
        string fil = ModText.ToLower();
        var args = fil.Split(',').ToList();
        //根据选择的筛选选择模组
        switch (ModFilter)
        {
            case ModFilterType.Name:
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
            case ModFilterType.FileName:
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
            case ModFilterType.Author:
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
            case ModFilterType.Modid:
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
        }
    }
}
