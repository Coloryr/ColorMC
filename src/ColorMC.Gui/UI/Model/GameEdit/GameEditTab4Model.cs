using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Input;
using ColorMC.Core.Helpers;
using ColorMC.Core.Net.Apis;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.Objs.Config;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.GameEdit;

/// <summary>
/// 游戏实例编辑
/// </summary>
public partial class GameEditModel
{
    /// <summary>
    /// 根路径
    /// </summary>
    private readonly ObservableCollection<ModNodeModel> _root = [];
    /// <summary>
    /// 显示内容
    /// </summary>
    public HierarchicalTreeDataGridSource<ModNodeModel> ModSource { get; private set; } = new HierarchicalTreeDataGridSource<ModNodeModel>([])
    {
        Columns =
        {
            new HierarchicalExpanderColumn<ModNodeModel>(
                new TextColumn<ModNodeModel, string?>("", x=>x.Group),
                x => x.Children,
                x => x.HasChildren,
                x => x.IsExpanded),
            new TemplateColumn<ModNodeModel>(
                LangUtils.Get("Text.Enable"),
                cellTemplateResourceKey: "ModCell1",
                options: new TemplateColumnOptions<ModNodeModel>
                {
                    CanUserResizeColumn = false
                }),
            new TemplateColumn<ModNodeModel>(
                LangUtils.Get("GameEditWindow.Tab4.Text15"),
                cellTemplateResourceKey: "ModCell8"),
            new TextColumn<ModNodeModel, string>(
                "modid",
                x => x.Modid),
            new TextColumn<ModNodeModel, string?>(
                LangUtils.Get("Text.Name"),
                x => x.Name),
            new TemplateColumn<ModNodeModel>(
                LangUtils.Get("Text.Version"),
                cellTemplateResourceKey: "ModCell3"),
            new TextColumn<ModNodeModel, string?>(
                LangUtils.Get("GameEditWindow.Tab4.Text11"),
                x => x.Loader),
            new TextColumn<ModNodeModel, string?>(
                LangUtils.Get("GameEditWindow.Tab4.Text14"),
                x => x.Side),
            new TemplateColumn<ModNodeModel>(
                LangUtils.Get("Text.DownloadSource"),
                cellTemplateResourceKey: "ModCell4"),
            new TemplateColumn<ModNodeModel>(
                LangUtils.Get("GameEditWindow.Tab4.Text12"),
                cellTemplateResourceKey: "ModCell5"),
            new TemplateColumn<ModNodeModel>(
                LangUtils.Get("GameEditWindow.Tab4.Text13"),
                cellTemplateResourceKey: "ModCell6"),
            new TemplateColumn<ModNodeModel>(
                LangUtils.Get("Text.Path"),
                cellTemplateResourceKey: "ModCell7"),
            new TextColumn<ModNodeModel, string?>(
                LangUtils.Get("Text.Author"),
                x => x.Author),
            new TextColumn<ModNodeModel, string?>(
                LangUtils.Get("GameEditWindow.Tab4.Text10"),
                x => x.Url),
        },
    };

    /// <summary>
    /// 过滤器列表
    /// </summary>
    public string[] ModFilterList { get; init; } = LangUtils.GetFilterName();

    /// <summary>
    /// 模组列表
    /// </summary>
    private readonly List<ModNodeModel> _modItems = [];
    /// <summary>
    /// 显示模组列表
    /// </summary>
    private readonly List<ModNodeModel> _displayModList = [];

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

    partial void OnModFilterChanged(ModFilterType value)
    {
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
        var dialog = Window.ShowProgress(LangUtils.Get("GameEditWindow.Tab4.Text27"));
        var res = await GameBinding.ModCheckAsync(_modItems);
        Window.CloseDialog(dialog);
        if (res)
        {
            Window.Notify(LangUtils.Get("GameEditWindow.Tab4.Text28"));
        }
    }

    /// <summary>
    /// 开始标记模组
    /// </summary>
    public async void StartSetMod()
    {
        var dialog = new InputModel(Window.WindowId)
        {
            Watermark1 = LangUtils.Get("GameEditWindow.Tab4.Text35"),
            Watermark2 = LangUtils.Get("GameEditWindow.Tab4.Text36"),
            Text2Visable = true
        };
        var res = await Window.ShowDialogWait(dialog);
        if (res is not true)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(dialog.Text1) && string.IsNullOrWhiteSpace(dialog.Text2))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(dialog.Text1) || string.IsNullOrWhiteSpace(dialog.Text2))
        {
            Window.Show(LangUtils.Get("GameEditWindow.Tab4.Text45"));
            return;
        }

        var dialog1 = Window.ShowProgress(LangUtils.Get("GameEditWindow.Tab4.Text37"));
        var res1 = await GameBinding.MarkModsAsync(_obj, dialog.Text1, dialog.Text2);
        Window.CloseDialog(dialog1);
        if (!res1)
        {
            Window.Show(LangUtils.Get("GameEditWindow.Tab4.Text46"));
            return;
        }
        else
        {
            Window.Notify(LangUtils.Get("GameEditWindow.Tab4.Text38"));
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

        var res = await Window.ShowChoice(LangUtils.Get("GameEditWindow.Tab4.Text30"));

        _isModSet = true;
        var dialog = Window.ShowProgress(LangUtils.Get("GameEditWindow.Tab4.Text31"));
        var res1 = await ModrinthHelper.AutoMarkAsync(_obj, res);
        Window.CloseDialog(dialog);
        if (!res1.State)
        {
            Window.Show(string.Format(LangUtils.Get("GameEditWindow.Tab4.Text41"), res1.Data));
        }
        else
        {
            Window.Notify(string.Format(LangUtils.Get("GameEditWindow.Tab4.Text32"), res1.Data));
        }
        _isModSet = false;
    }

    /// <summary>
    /// 检查模组更新
    /// </summary>
    private async void CheckMod()
    {
        var dialog = Window.ShowProgress(LangUtils.Get("GameEditWindow.Tab4.Text23"));
        var res = await WebBinding.CheckModUpdateAsync(_obj, _modItems);
        Window.CloseDialog(dialog);
        if (res.Count > 0)
        {
            var res1 = await Window.ShowChoice(string.Format(
                LangUtils.Get("GameEditWindow.Tab4.Text24"), res.Count));
            if (res1)
            {
                WebBinding.UpgradeMod(_obj, res);
                Window.Notify(LangUtils.Get("GameEditWindow.Tab4.Text33"));
            }
        }
        else
        {
            Window.Show(LangUtils.Get("GameEditWindow.Tab4.Text25"));
        }
    }

    /// <summary>
    /// 导入模组
    /// </summary>
    private async void ImportMod()
    {
        var top = Window.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var res = await PathBinding.AddFileAsync(top, _obj, FileType.Schematic);

        if (res == null)
            return;

        if (res == false)
        {
            Window.Show(LangUtils.Get("GameEditWindow.Tab11.Text7"));
            return;
        }

        Window.Show(LangUtils.Get("GameEditWindow.Tab11.Text3"));
        LoadSchematic();
        var file = await PathBinding.AddFileAsync(top, _obj, FileType.Mod);

        if (file == null)
        {
            return;
        }

        if (file == false)
        {
            Window.Show(LangUtils.Get("GameEditWindow.Tab4.Text40"));
            return;
        }

        Window.Notify(LangUtils.Get("GameEditWindow.Tab4.Text20"));
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
    public async void DeleteMod(IEnumerable<ModNodeModel> items)
    {
        var res = await Window.ShowChoice(
            string.Format(LangUtils.Get("GameEditWindow.Tab4.Text22"), items.Count()));
        if (!res)
        {
            return;
        }

        bool error = false;

        var deletelist = new List<ModNodeModel>();

        items.ToList().ForEach(item =>
        {
            try
            {
                GameBinding.DeleteMod(item.Obj);
                deletelist.Add(item);
            }
            catch
            {
                error = true;
            }
        });

        if (error)
        {
            Window.Show(LangUtils.Get("GameEditWindow.Tab4.Text44"));
            return;
        }

        foreach (var item in deletelist)
        {
            _displayModList.Remove(item);
            foreach (var item1 in _root)
            {
                item1.Children.Remove(item);
            }
        }

        Window.Notify(LangUtils.Get("Text.DeleteDone"));
    }

    /// <summary>
    /// 删除模组
    /// </summary>
    /// <param name="item">模组</param>
    public async void DeleteMod(ModNodeModel item)
    {
        var res = await Window.ShowChoice(
            string.Format(LangUtils.Get("GameEditWindow.Tab4.Text21"), item.Name));
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
            Window.Show(LangUtils.Get("GameEditWindow.Tab4.Text44"));
            return;
        }
        _displayModList.Remove(item);
        foreach (var item1 in _root)
        {
            item1.Children.Remove(item);
        }

        Window.Notify(LangUtils.Get("Text.DeleteDone"));
    }

    /// <summary>
    /// 禁用/启用模组
    /// </summary>
    /// <param name="item">模组</param>
    public async void DisableEnableMod(ModNodeModel item)
    {
        if (item == null)
        {
            return;
        }
        if (GameManager.IsGameRun(_obj))
        {
            Window.Notify(LangUtils.Get("GameEditWindow.Tab4.Text43"));
            return;
        }
        var res = GameBinding.ModEnableDisable(item.Obj);
        if (!res.State)
        {
            Window.Show(res.Data!);
        }
        else
        {
            item.LocalChange();
            item.Enable = !item.Obj.Disable;
            if (item.Enable == true)
            {
                return;
            }

            var dislist = new List<ModNodeModel>
            {
                item
            };

            var list = GameBinding.ModDisable(item, _modItems);

            //自动禁用依赖的模组
            foreach (var item1 in list.ToArray())
            {
                if (item1.Enable == false)
                {
                    list.Remove(item1);
                }
            }

            if (list.Count != 0)
            {
                if (await Window.ShowChoice(string.Format(LangUtils.Get("GameEditWindow.Tab4.Text29"), list.Count)))
                {
                    foreach (var item1 in list)
                    {
                        if (item1.Enable != true)
                        {
                            continue;
                        }
                        GameBinding.ModEnableDisable(item1.Obj);
                        item1.LocalChange();
                        item1.Enable = !item1.Obj.Disable;
                        dislist.Add(item1);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 禁用/启用模组
    /// </summary>
    /// <param name="item">模组</param>
    public async void DisableEnableMod(IEnumerable<ModNodeModel> items)
    {
        if (!items.Any())
        {
            return;
        }
        if (GameManager.IsGameRun(_obj))
        {
            Window.Notify(LangUtils.Get("GameEditWindow.Tab4.Text43"));
            return;
        }
        var dislist = new List<ModNodeModel>();
        foreach (var item in items)
        {
            var res = GameBinding.ModEnableDisable(item.Obj);
            if (!res.State)
            {
                Window.Show(res.Data!);
                break;
            }
            item.LocalChange();
            item.Enable = !item.Obj.Disable;
            dislist.Add(item);
        }
    }

    /// <summary>
    /// 显示模组信息
    /// </summary>
    /// <param name="list"></param>
    private void DisplayMod(List<string> list)
    {
        _displayModList.Clear();
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
        var dialog = Window.ShowProgress(LangUtils.Get("GameEditWindow.Tab4.Text19"));
        _modItems.Clear();
        var res = await GameBinding.GetGameModsAsync(_obj);
        Window.CloseDialog(dialog);
        if (res == null)
        {
            Window.Show(LangUtils.Get("GameEditWindow.Tab4.Text39"));
            return;
        }

        res.ForEach(item =>
        {
            var obj1 = _obj.Mods.Values.FirstOrDefault(a => a.Sha1 == item.Sha1);
            _modItems.Add(new ModNodeModel(item, obj1, this));
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
            var res1 = await Window.ShowChoice(string.Format(LangUtils.Get("GameEditWindow.Tab4.Text26"), count));
            if (res1)
            {
                DisplayMod(list1);
                return;
            }
        }

        foreach (var item in _modItems)
        {
            if (_setting.Mod.ModName.TryGetValue(item.Obj.Sha1, out var temp))
            {
                item.Text = temp;
            }
        }

        Window.Notify(LangUtils.Get("GameEditWindow.Tab4.Text34"));

        LoadModDisplay();
    }

    /// <summary>
    /// 编辑模组注释
    /// </summary>
    /// <param name="item"></param>
    public void EditModText(ModDisplayModel item)
    {
        if (!_setting.Mod.ModName.TryAdd(item.Obj.Sha1, item.Text))
        {
            _setting.Mod.ModName[item.Obj.Sha1] = item.Text;
        }

        GameManager.WriteConfig(_obj, _setting);
    }

    /// <summary>
    /// 加载模组列表
    /// </summary>
    private void LoadModDisplay()
    {
        _displayModList.Clear();
        //没有筛选内容
        if (string.IsNullOrWhiteSpace(ModText))
        {
            _displayModList.AddRange(_modItems);
            LoadTree();
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
                            _displayModList.Add(item);
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
                            _displayModList.Add(item);
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
                            _displayModList.Add(item);
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
                            _displayModList.Add(item);
                            break;
                        }
                    }
                }
                break;
        }

        LoadTree();
    }

    /// <summary>
    /// 模组树重载
    /// </summary>
    public void ModTreeUpdate()
    {
        foreach (var item in _root)
        {
            item.UpdateGroup();
        }
    }

    /// <summary>
    /// 加载树
    /// </summary>
    private void LoadTree()
    {
        var fail = new ModNodeModel(LangUtils.Get("GameEditWindow.Tab4.Text47"));
        var enable = new ModNodeModel(LangUtils.Get("GameEditWindow.Tab4.Text48"));
        var disable = new ModNodeModel(LangUtils.Get("GameEditWindow.Tab4.Text49"));

        var group = _setting.Mod.Groups;

        var map = new Dictionary<string, string>();
        var map1 = new Dictionary<string, ModNodeModel>();

        foreach (var item in group)
        {
            foreach (var item1 in item.Value)
            {
                map.Add(item1, item.Key);
            }
        }

        foreach (var item in _displayModList)
        {
            if (map.TryGetValue(item.Obj.Sha1, out var group1))
            {
                if (map1.TryGetValue(group1, out var group2))
                {
                    group2.Children.Add(item);
                }
                else
                {
                    var group3 = new ModNodeModel(group1);
                    group3.Children.Add(item);
                    map1.Add(group1, group3);
                }
            }
            else if (item.Obj.ReadFail)
            {
                fail.Children.Add(item);
            }
            else if (item.Enable == true)
            {
                enable.Children.Add(item);
            }
            else
            {
                disable.Children.Add(item);
            }
        }

        _root.Clear();
        _root.Add(fail);
        foreach (var item in map1.Values)
        {
            _root.Add(item);
        }
        _root.Add(enable);
        _root.Add(disable);

        foreach (var item in _root)
        {
            item.UpdateGroup();
        }

        ModSource.Items = _root;

        ModSource.RowSelection?.SingleSelect = false;
    }

    /// <summary>
    /// 设置备注
    /// </summary>
    /// <param name="mods">模组列表</param>
    /// <returns></returns>
    public async void SetText(IEnumerable<ModNodeModel> mods)
    {
        string text = "";
        if (mods.Count() == 1)
        {
            text = mods.First().Text ?? "";
        }

        var dialog = new InputModel(Window.WindowId)
        {
            Text1 = text,
            Watermark1 = LangUtils.Get("GameEditWindow.Tab4.Text50")
        };
        if (await Window.ShowDialogWait(dialog) is not true)
        {
            return;
        }
        text = dialog.Text1;

        foreach (var item in mods)
        {
            item.Text = text;
            if (!_setting.Mod.ModName.TryAdd(item.Obj.Sha1, text))
            {
                _setting.Mod.ModName[item.Obj.Sha1] = text;
            }
        }

        GameManager.WriteConfig(_obj, _setting);
    }

    /// <summary>
    /// 设置模组在线信息
    /// </summary>
    /// <param name="mod"></param>
    public async void SetProjectId(ModNodeModel mod)
    {
        var dialog = new InputModel(Window.WindowId)
        {
            Text1 = mod.PID ?? "",
            Text2 = mod.FID ?? "",
            Watermark1 = LangUtils.Get("GameEditWindow.Tab4.Text12"),
            Watermark2 = LangUtils.Get("GameEditWindow.Tab4.Text13"),
            Text2Visable = true
        };
        if (await Window.ShowDialogWait(dialog) is not true)
        {
            return;
        }

        var pid = string.IsNullOrWhiteSpace(dialog.Text1);
        var fid = string.IsNullOrWhiteSpace(dialog.Text2);
        if (pid && fid)
        {
            return;
        }
        else if (pid || fid)
        {
            Window.Show(LangUtils.Get("GameEditWindow.Tab4.Text51"));
            return;
        }

        var dialog1 = Window.ShowProgress(LangUtils.Get("GameEditWindow.Tab4.Text52"));
        var info = await GameBinding.TestProject(_obj, dialog.Text1, dialog.Text2);
        if (info == null)
        {
            Window.CloseDialog(dialog1);
            Window.Show(LangUtils.Get("GameEditWindow.Tab4.Text51"));
        }
        else
        {
            info.File = Path.GetFileName(mod.Local);
            mod.Obj1 = info;
            if (!_obj.Mods.TryAdd(info.ModId, info))
            {
                _obj.Mods[info.ModId] = info;
            }
            mod.Update();
        }
    }

    /// <summary>
    /// 设置分组
    /// </summary>
    /// <param name="mods"></param>
    public async void SetGroup(IEnumerable<ModNodeModel> mods)
    {
        string text = "";
        if (mods.Count() == 1)
        {
            var mod = mods.First();
            foreach (var item in _setting.Mod.Groups)
            {
                if (item.Value.Contains(mod.Obj.Sha1))
                {
                    text = item.Key;
                    break;
                }
            }
        }

        var dialog = new SelectModel(Window.WindowId)
        {
            Text = LangUtils.Get("GameEditWindow.Tab4.Text53"),
            SelectText = text,
            IsEdit = true,
        };
        foreach (var item in _setting.Mod.Groups)
        {
            dialog.Items.Add(item.Key);
        }

        if (await Window.ShowDialogWait(dialog) is not true)
        {
            return;
        }

        foreach (var item1 in _setting.Mod.Groups.ToArray())
        {
            foreach (var item in mods)
            {
                item1.Value.Remove(item.Obj.Sha1);
            }

            if (item1.Value.Count == 0)
            { 
                _setting.Mod.Groups.Remove(item1.Key);
            }
        }

        if (_setting.Mod.Groups.TryGetValue(dialog.SelectText, out var list))
        {
            foreach (var item in mods)
            {
                list.Add(item.Obj.Sha1);
            }
        }
        else
        {
            _setting.Mod.Groups.Add(dialog.SelectText, [.. mods.Select(item => item.Obj.Sha1)]);
        }

        GameManager.WriteConfig(_obj, _setting);

        LoadTree();
    }
}
