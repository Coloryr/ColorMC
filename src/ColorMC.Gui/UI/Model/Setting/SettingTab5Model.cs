﻿using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Setting;

/// <summary>
/// 设置窗口
/// </summary>
public partial class SettingModel
{
    /// <summary>
    /// Java列表
    /// </summary>
    public ObservableCollection<JavaDisplayModel> JavaList { get; init; } = [];

    /// <summary>
    /// Java名字
    /// </summary>
    [ObservableProperty]
    private string? _javaName;
    /// <summary>
    /// Java路径
    /// </summary>
    [ObservableProperty]
    private string? _javaLocal;

    /// <summary>
    /// 选中的Java
    /// </summary>
    [ObservableProperty]
    private JavaDisplayModel _javaItem;

    /// <summary>
    /// 是否在搜索Java
    /// </summary>
    [ObservableProperty]
    private bool _javaFinding;

    /// <summary>
    /// Java是否在加载
    /// </summary>
    private bool _javaLoaded;
    /// <summary>
    /// 需要的Java版本
    /// </summary>
    private int _needJava;

    /// <summary>
    /// 添加Java
    /// </summary>
    [RelayCommand]
    public void AddJava()
    {
        if (string.IsNullOrWhiteSpace(JavaName) || string.IsNullOrWhiteSpace(JavaLocal))
        {
            Model.Show(App.Lang("SettingWindow.Tab5.Error2"));
            return;
        }

        Model.Progress(App.Lang("SettingWindow.Tab5.Info1"));

        var res = JavaBinding.AddJava(JavaName, JavaLocal);
        Model.ProgressClose();
        if (!res.State)
        {
            Model.Show(res.Message!);
            return;
        }

        JavaName = "";
        JavaLocal = "";

        LoadJava();
    }

    /// <summary>
    /// 选中Java
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task SelectJava()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectFile(top, FileType.Java);
        if (file.Item1 != null)
        {
            JavaLocal = file.Item1;
            var info = JavaBinding.GetJavaInfo(file.Item1);
            if (info != null)
            {
                JavaName = info.Type + "_" + info.Version;
            }
        }
    }

    /// <summary>
    /// 下载Java
    /// </summary>
    private void ShowAddJava()
    {
        WindowManager.ShowAddJava(_needJava);
    }

    /// <summary>
    /// 导入Java压缩包
    /// </summary>
    private async void AddJavaZip()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectFile(top, FileType.JavaZip);
        if (file.Item1 == null || file.Item2 == null)
        {
            return;
        }

        Model.Progress(App.Lang("SettingWindow.Tab5.Info7"));
        string temp = App.Lang("AddGameWindow.Tab1.Info21");
        var res = await JavaBinding.AddJavaZip(file.Item1, file.Item2, (a, b, c) =>
        {
            Dispatcher.UIThread.Post(() => Model.ProgressUpdate($"{temp} {a} {b}/{c}"));
        });
        Model.ProgressClose();
        if (!res.State)
        {
            Model.Show(res.Message!);
        }
        else
        {
            Model.Notify(App.Lang("SettingWindow.Tab5.Info6"));
        }
        LoadJava();
    }

    /// <summary>
    /// 搜索Java
    /// </summary>
    public async void FindJavaDir()
    {
#if Phone
        return;
#endif
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectPath(top, PathType.JavaPath);
        if (file == null)
        {
            return;
        }

        var res = await Model.ShowAsync(string.Format(App.Lang("AddGameWindow.Tab3.Info3"), file));
        if (!res)
        {
            return;
        }

        JavaFinding = true;
        Model.SubTitle = App.Lang("SettingWindow.Tab5.Info8");
        var list = await JavaBinding.FindJava(file);
        Model.SubTitle = null;
        JavaFinding = false;
        if (list == null)
        {
            Model.Show(App.Lang("SettingWindow.Tab5.Error1"));
            return;
        }

        list.ForEach(item => JvmPath.AddItem(item.Type + "_" + item.Version, item.Path));
        LoadJava();
        Model.Notify(App.Lang("SettingWindow.Tab5.Info4"));
    }

    /// <summary>
    /// 搜索Java
    /// </summary>
    public async void FindJava()
    {
#if Phone
        return;
#endif
        JavaFinding = true;
        Model.SubTitle = App.Lang("SettingWindow.Tab5.Info8");
        var list = await JavaBinding.FindJava();
        Model.SubTitle = null;
        JavaFinding = false;
        if (list == null)
        {
            Model.Show(App.Lang("SettingWindow.Tab5.Error1"));
            return;
        }

        list.ForEach(item => JvmPath.AddItem(item.Type + "_" + item.Version, item.Path));
        LoadJava();
        Model.Notify(App.Lang("SettingWindow.Tab5.Info4"));
    }

    /// <summary>
    /// 加载Java列表
    /// </summary>
    public void LoadJava()
    {
        JavaList.Clear();
        JavaList.AddRange(JavaBinding.GetJavas());
    }

    /// <summary>
    /// 删除所有Java
    /// </summary>
    private async void DeleteJava()
    {
        var res = await Model.ShowAsync(App.Lang("SettingWindow.Tab5.Info3"));
        if (!res)
            return;

        JavaBinding.RemoveAllJava();
        LoadJava();
    }

    /// <summary>
    /// 打开Java文件夹
    /// </summary>
    private void OpenJavaPath()
    {
        PathBinding.OpenPath(PathType.JavaPath);
    }

    /// <summary>
    /// 加载Java版本
    /// </summary>
    /// <param name="mainversion"></param>
    public void Load(int mainversion)
    {
        _needJava = mainversion;
        LoadJava();
        if (!_javaLoaded && JavaList.Count == 0)
        {
            _javaLoaded = true;
            FindJava();
        }
    }
}
