using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

/// <summary>
/// 添加整合包
/// </summary>
public partial class AddModPackControlModel : AddBaseModel
{
    /// <summary>
    /// 分类
    /// </summary>
    private readonly Dictionary<int, string> _categories = [];

    /// <summary>
    /// 是否继续添加
    /// </summary>
    private bool _keep = false;
    /// <summary>
    /// 添加到的游戏分组
    /// </summary>
    private string? _group;

    private readonly string _useName;

    public AddModPackControlModel(WindowModel model) : base(model)
    {
        _useName = ToString() ?? "AddModPackControlModel";

        SourceTypeList.AddRange([SourceType.CurseForge, SourceType.Modrinth]);
        SourceTypeList.ForEach(item => SourceList.Add(item.GetName()));

        EventManager.ModpackInstall += EventManager_ModpackInstall;
        EventManager.ModpackStop += EventManager_ModpackStop;
    }

    private void EventManager_ModpackStop(SourceItemObj obj, bool res)
    {
        foreach (var item in DisplayList)
        {
            if (obj.CheckProject(item.Obj))
            {
                item.NowDownload = false;
                item.IsDownload = res;
            }
        }
    }

    private void EventManager_ModpackInstall(SourceItemObj obj)
    {
        foreach (var item in DisplayList)
        {
            if (obj.CheckProject(item.Obj))
            {
                item.NowDownload = true;
            }
        }
    }

    /// <summary>
    /// 选中项目
    /// </summary>
    [RelayCommand]
    public void Select()
    {
        if (_lastSelect == null)
        {
            Window.Show(LangUtils.Get("AddModPackWindow.Text22"));
            return;
        }

        DisplayItems();
    }

    /// <summary>
    /// 加载搜索源数据
    /// </summary>
    public override async void LoadSourceData()
    {
        if (_load)
        {
            return;
        }

        SourceLoad = false;
        _load = true;

        IsSelect = false;

        CategorieList.Clear();
        SortTypeList.Clear();

        GameVersionList.Clear();
        _categories.Clear();

        ClearList();

        switch (Source)
        {
            case 0:
            case 1:
                SortTypeList.AddRange(Source == 0 ?
                    LangUtils.GetCurseForgeSortTypes() :
                    LangUtils.GetModrinthSortTypes());

                var dialog = Window.ShowProgress(LangUtils.Get("AddModPackWindow.Text20"));
                var list = Source == 0 ?
                    await CurseForgeHelper.GetGameVersionsAsync() :
                    await ModrinthHelper.GetGameVersionAsync();
                var list1 = Source == 0 ?
                    await CurseForgeHelper.GetCategoriesAsync(FileType.Modpack) :
                    await ModrinthHelper.GetCategoriesAsync(FileType.Modpack);
                Window.CloseDialog(dialog);
                if (list == null || list1 == null)
                {
                    _load = false;
                    LoadFail();
                    return;
                }
                GameVersionList.AddRange(list);

                _categories.Add(0, "");
                var a = 1;
                foreach (var item in list1)
                {
                    _categories.Add(a++, item.Key);
                }

                var list2 = new List<string>()
                {
                    ""
                };

                list2.AddRange(list1.Values);

                CategorieList.AddRange(list2);

                Categorie = 0;
                GameVersion = GameVersionList.FirstOrDefault();
                SortType = Source == 0 ? 1 : 0;

                LoadDisplayList();
                break;
        }

        SourceLoad = true;
        _load = false;
    }

    /// <summary>
    /// 添加完成
    /// </summary>
    private async void Done(Guid uuid)
    {
        Window.Notify(LangUtils.Get("AddGameWindow.Tab1.Text29"));

        if (_keep)
        {
            return;
        }

        (WindowManager.MainWindow?.DataContext as MainModel)?.Select(uuid);

        if (!HaveDownload)
        {
            var res = await Window.ShowChoice(LangUtils.Get("AddGameWindow.Tab1.Text43"));
            if (!res)
            {
                Dispatcher.UIThread.Post(WindowClose);
            }
            else
            {
                _keep = true;
            }
        }
    }

    /// <summary>
    /// 加载搜索源信息失败
    /// </summary>
    private async void LoadFail()
    {
        var res = await Window.ShowChoice(LangUtils.Get("AddModPackWindow.Text25"));
        if (res)
        {
            LoadSourceData();
            return;
        }

        if (Source < SourceList.Count)
        {
            res = await Window.ShowChoice(LangUtils.Get("AddModPackWindow.Text21"));
            if (res)
            {
                Source++;
            }
        }
    }

    /// <summary>
    /// 加载项目列表
    /// </summary>
    public override async void LoadDisplayList()
    {
        //MO不允许少文字搜索
        var type = SourceTypeList[Source];

        if (type == SourceType.Modrinth && Categorie == 4 && Text?.Length < 3)
        {
            Window.Show(LangUtils.Get("AddModPackWindow.Text27"));
            return;
        }

        var dialog = Window.ShowProgress(LangUtils.Get("AddModPackWindow.Text18"));
        var page = Page ?? 1;
        page -= 1;
        var res = await WebBinding.GetModPackListAsync(type,
            GameVersion, Text, page, SortType,
            Categorie < 0 ? "" : _categories[Categorie]);

        //制作分页
        MaxPage = res.TotalCount / 20;

        PageLoad();

        if (res.List == null)
        {
            Window.Show(LangUtils.Get("AddModPackWindow.Text23"));
            Window.CloseDialog(dialog);
            return;
        }

        DisplayList.Clear();

        foreach (var item in res.List)
        {
            item.IsDownload = InstancesPath.Games.Any(item1 => item1.Modpack && item1.PID == item.Obj.Pid);
            item.Add = this;
            item.Window = Window;
            item.NowDownload = GameManager.TestDowload(item.Obj);
            DisplayList.Add(item);
        }

        OnPropertyChanged(nameof(DisplayList));

        _lastSelect = null;

        EmptyDisplay = DisplayList.Count == 0;

        Window.CloseDialog(dialog);
        Window.Notify(LangUtils.Get("AddResourceWindow.Text24"));
    }

    public override void Close()
    {
        base.Close();
        _close = true;
        _load = true;
        Window.RemoveChoiseData(_useName);

        _lastSelect = null;

        EventManager.ModpackInstall -= EventManager_ModpackInstall;
        EventManager.ModpackStop -= EventManager_ModpackStop;
    }

    /// <summary>
    /// 转到下载类型
    /// </summary>
    /// <param name="type">下载源</param>
    /// <param name="pid">项目ID</param>
    public async void GoFile(SourceType type, string pid)
    {
        Source = (int)type;
        await Task.Run(() =>
        {
            while ((!Display || _load) && !_close)
            {
                Thread.Sleep(100);
            }
        });

        _load = true;

        var res = await WebBinding.GetModpackAsync(type, pid);
        if (res == null)
        {
            Window.Show(LangUtils.Get("AddModPackWindow.Text23"));
            _load = false;
            return;
        }
        Last = res;
        DisplayItemInfo = true;

        _load = false;
    }

    public void SetGroup(string? group)
    {
        _group = group;
    }
}
