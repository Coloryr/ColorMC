using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using ColorMC.Core.LaunchPath;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Net.Apis;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

/// <summary>
/// 添加游戏实例
/// </summary>
public partial class AddGameModel : ControlModel
{
    public const string NameTab1 = ":1";
    public const string NameTab2 = ":2";
    public const string NameTab3 = ":3";
    public const string NameBack = "Back";

    /// <summary>
    /// 游戏分组
    /// </summary>
    public ObservableCollection<string> GroupList { get; init; } = [];

    /// <summary>
    /// 实例名字
    /// </summary>
    [ObservableProperty]
    private string? _name;
    /// <summary>
    /// 实例组
    /// </summary>
    [ObservableProperty]
    private string? _group;

    /// <summary>
    /// 云同步启用
    /// </summary>
    [ObservableProperty]
    private bool _cloudEnable;
    /// <summary>
    /// 是否为主界面
    /// </summary>
    [ObservableProperty]
    private bool _main = true;

    /// <summary>
    /// 添加到的分组
    /// </summary>
    public string? DefaultGroup { get; set; }

    /// <summary>
    /// 是否在加载中
    /// </summary>
    private bool _load = false;
    /// <summary>
    /// 是否继续添加
    /// </summary>
    private bool _keep = false;

    public AddGameModel(WindowModel model) : base(model)
    {
        LoadGroup();

        GameVersionUpdate();

        CloudEnable = ColorMCCloudAPI.Connect;
    }

    ///// <summary>
    ///// 添加新的游戏分组
    ///// </summary>
    ///// <returns></returns>
    //[RelayCommand]
    //public async Task AddGroup()
    //{
    //    var dialog = new InputModel(Window.WindowId)
    //    {
    //        Watermark1 = LanguageUtils.Get("Text.Group")
    //    };
    //    var res = await Window.ShowDialogWait(dialog);
    //    if (res is not true)
    //    {
    //        return;
    //    }

    //    if (string.IsNullOrWhiteSpace(dialog.Text1))
    //    {
    //        Window.Show(LanguageUtils.Get("AddGameWindow.Tab1.Text45"));
    //        return;
    //    }

    //    if (!GameBinding.AddGameGroup(dialog.Text1))
    //    {
    //        Window.Show(LanguageUtils.Get("AddGameWindow.Tab1.Text46"));
    //        return;
    //    }

    //    Window.Notify(LanguageUtils.Get("AddGameWindow.Tab1.Text28"));

        
    //    Group = dialog.Text1;
    //}

    /// <summary>
    /// 转到菜单
    /// </summary>
    /// <param name="arg">目标菜单</param>
    [RelayCommand]
    public void GoTab(object? arg)
    {
        Main = false;
        Window.PushBack(Back);
        OnPropertyChanged(arg as string);
    }

    /// <summary>
    /// 转到添加整合包
    /// </summary>
    [RelayCommand]
    public void GoModPack()
    {
        WindowManager.ShowAddModPack(DefaultGroup);
        WindowClose();
    }

    /// <summary>
    /// 转到添加云同步实例
    /// </summary>
    [RelayCommand]
    public void GoCloud()
    {
        Main = false;
        Window.PushBack(BackMain);
        GameCloudDownload();
    }

    /// <summary>
    /// 转到添加服务器实例
    /// </summary>
    [RelayCommand]
    public void GoServer()
    {
        Main = false;
        Window.PushBack(BackMain);
        ServerPackDownload();
    }

    private void LoadGroup() 
    {
        GroupList.Clear();
        GroupList.AddRange(InstancesPath.GroupKeys);
    }

    /// <summary>
    /// 返回主界面
    /// </summary>
    public void BackMain()
    {
        Window.PopBack();
        OnPropertyChanged(NameBack);
        Main = true;
    }

    /// <summary>
    /// 返回
    /// </summary>
    private void Back()
    {
        Window.PopBack();
        Name = null;
        Group = DefaultGroup;
        Version = null;
        LoaderVersion = null;
        LoaderTypeList.Clear();
        LoaderType = -1;
        LoaderLocal = null;
        _loaderTypeList.Clear();
        LoaderVersionList.Clear();
        Type = null;
        ZipLocal = null;
        _fileModel = null;
        SelectPath = null;
        Files = null;
        OnPropertyChanged("Back");
        Main = true;
    }

    public override void Close()
    {
        _load = true;
        Back();
        GameVersionList.Clear();
        LoaderVersionList.Clear();
        LoaderTypeList.Clear();
        _fileModel = null!;
        Files = null!;
    }

    /// <summary>
    /// 添加完成
    /// </summary>
    private async void Done(Guid uuid)
    {
        Name = "";
        Group = "";

        LoadGroup();

        Window.Notify(LanguageUtils.Get("AddGameWindow.Tab1.Text29"));

        if (_keep)
        {
            return;
        }

        GameBinding.SelectAndReloadGame(uuid);

        var res = await Window.ShowChoice(LanguageUtils.Get("AddGameWindow.Tab1.Text43"));
        if (res != true)
        {
            WindowClose();
        }
        else
        {
            _keep = true;
        }
    }

    /// <summary>
    /// 下载云同步游戏实例
    /// </summary>
    /// <returns></returns>
    public async void GameCloudDownload()
    {
        var dialog = Window.ShowProgress(LanguageUtils.Get("AddGameWindow.Tab1.Text31"));
        var list = await ColorMCCloudAPI.GetListAsync();
        Window.CloseDialog(dialog);
        if (list == null)
        {
            Window.Show(LanguageUtils.Get("AddModPackWindow.Text24"));
            return;
        }
        var list1 = new List<string>();
        list.ForEach(item =>
        {
            if (!string.IsNullOrWhiteSpace(item.Name) && InstancesPath.GetGame(item.UUID) == null)
            {
                list1.Add(item.Name);
            }
        });
        var dialog1 = new SelectModel(Window.WindowId)
        {
            Text = LanguageUtils.Get("AddGameWindow.Tab1.Text32"),
            Items = [.. list1]
        };
        var res = await Window.ShowDialogWait(dialog1);
        if (res is not true)
        {
            return;
        }

        dialog = Window.ShowProgress(LanguageUtils.Get("AddGameWindow.Tab1.Text33"));
        var obj = list[dialog1.Index];
        while (true)
        {
            //替换冲突的名字
            if (InstancesPath.GetGameByName(obj.Name) != null)
            {
                var dialog2 = new ChoiceModel(Window.WindowId)
                {
                    Text = LanguageUtils.Get("AddGameWindow.Tab1.Text34"),
                    CancelVisable = true
                };
                var res1 = await Window.ShowDialogWait(dialog2);
                if (res1 is not true)
                {
                    Window.CloseDialog(dialog);
                    return;
                }
                var dialog3 = new InputModel(Window.WindowId)
                {
                    Watermark1 = LanguageUtils.Get("AddGameWindow.Tab1.Text2"),
                    Text1 = obj.Name
                };
                var res2 = await Window.ShowDialogWait(dialog3);
                if (res2 is not true)
                {
                    Window.CloseDialog(dialog);
                    return;
                }

                obj.Name = dialog3.Text1;
            }
            else
            {
                break;
            }
        }
        //下载游戏实例
        var res3 = await GameBinding.DownloadCloudAsync(obj, Group,
            new OverGameGui(Window));
        Window.CloseDialog(dialog);
        if (!res3.State)
        {
            Window.Show(res3.Data ?? LanguageUtils.Get("AddGameWindow.Tab1.Text56"));
            return;
        }

        WindowManager.ShowGameCloud(InstancesPath.GetGame(obj.UUID)!);
        if (Guid.TryParse(res3.Data!, out var guid))
        {
            Done(guid);
        }
    }

    /// <summary>
    /// 下载服务器实例
    /// </summary>
    public async void ServerPackDownload()
    {
        var dialog = new InputModel(Window.WindowId)
        {
            Watermark1 = LanguageUtils.Get("AddGameWindow.Tab1.Text35")
        };
        var res = await Window.ShowDialogWait(dialog);
        if (res is not true)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(dialog.Text1))
        {
            Window.Show(LanguageUtils.Get("AddGameWindow.Tab1.Text53"));
            return;
        }

        string url = dialog.Text1;

        if (!url.EndsWith('/'))
        {
            url += '/';
        }
        //下载服务器包
        var dialog1 = Window.ShowProgress(LanguageUtils.Get("AddGameWindow.Tab1.Text36"));
        var res1 = await GameBinding.DownloadServerPackAsync(Window, dialog1, Name, Group, url,
            new OverGameGui(Window));
        Window.CloseDialog(dialog1);
        if (!res1.State)
        {
            Window.Show(res1.Data ?? LanguageUtils.Get("AddGameWindow.Tab1.Text56"));
        }
        else
        {
            if (Guid.TryParse(res1.Data!, out var guid))
            {
                Done(guid);
            }
        }
    }
}
