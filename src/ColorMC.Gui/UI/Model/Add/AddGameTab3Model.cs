using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

/// <summary>
/// 添加游戏实例
/// 导入文件夹
/// </summary>
public partial class AddGameModel
{
    /// <summary>
    /// 文件列表
    /// </summary>
    [ObservableProperty]
    private HierarchicalTreeDataGridSource<FileTreeNodeModel>? _files;

    /// <summary>
    /// 文件列表
    /// </summary>
    private FilesPage? _fileModel;

    /// <summary>
    /// 文件夹路径
    /// </summary>
    [ObservableProperty]
    private string? _selectPath;

    /// <summary>
    /// 可以导入
    /// </summary>
    [ObservableProperty]
    private bool _canInput;

    /// <summary>
    /// 刷新文件列表
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task RefashFiles()
    {
        if (Directory.Exists(SelectPath))
        {
            var res = await Window.ShowChoice(string.Format(LanguageUtils.Get("AddGameWindow.Tab3.Text6"), SelectPath));
            if (!res)
            {
                return;
            }
            var dialog = Window.ShowProgress(LanguageUtils.Get("AddGameWindow.Tab3.Text5"));

            //测试是否是其他启动器的游戏版本
            var list = GameHelper.ScanVersions(SelectPath);
            if (list.Count > 0)
            {
                res = await Window.ShowChoice(string.Format(LanguageUtils.Get("AddGameWindow.Tab3.Text7"), list.Count));
                if (!res)
                {
                    await ImportAsync(list);
                    return;
                }
            }

            _fileModel = await Task.Run(() =>
            {
                return new FilesPage(SelectPath, true,
                    ["assets", "libraries", "versions", "launcher_profiles.json"]);
            });
            Window.CloseDialog(dialog);
            Files = _fileModel.Source;

            CanInput = true;

            Window.Notify(LanguageUtils.Get("AddGameWindow.Tab3.Text8"));
        }
        else
        {
            CanInput = false;
            Files = null!;
            _fileModel = null!;
            Window.Show(string.Format(LanguageUtils.Get("AddGameWindow.Tab1.Text45"), SelectPath));
        }
    }

    /// <summary>
    /// 添加实例
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task AddFiles()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            Window.Show(LanguageUtils.Get("AddGameWindow.Tab1.Text45"));
            return;
        }

        if (PathHelper.FileHasInvalidChars(Name))
        {
            Window.Show(LanguageUtils.Get("AddGameWindow.Tab1.Text52"));
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectPath))
        {
            Window.Show(LanguageUtils.Get("AddGameWindow.Tab3.Text11"));
            return;
        }

        if (_fileModel == null)
        {
            Window.Show(LanguageUtils.Get("AddGameWindow.Tab3.Text12"));
            return;
        }

        var dialog = Window.ShowProgress(LanguageUtils.Get("AddGameWindow.Tab3.Text4"));
        var zip = new ZipGui(Window, dialog);
        var res = await GameBinding.AddGameAsync(Name, SelectPath, _fileModel.GetUnSelectItems(), Group, true, new OverGameGui(Window), zip);
        zip.Stop();
        Window.CloseDialog(dialog);

        if (!res.State)
        {
            Window.Show(LanguageUtils.Get("AddGameWindow.Tab3.Text9"));
            return;
        }

        Done(res.Game!.UUID);
    }

    /// <summary>
    /// 选择文件夹路径
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task SelectLocal()
    {
        var top = Window.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var res = await PathBinding.SelectPathAsync(top, PathType.GamePath);
        if (string.IsNullOrWhiteSpace(res))
        {
            return;
        }

        if (Directory.Exists(res))
        {
            SelectPath = res;

            await RefashFiles();
        }
        else
        {
            Window.Show(string.Format(LanguageUtils.Get("AddGameWindow.Tab3.Text10"), res));
        }
    }

    /// <summary>
    /// 设置选择的路径
    /// </summary>
    /// <param name="res"></param>
    public async void SetPath(string res)
    {
        SelectPath = res;

        await RefashFiles();
    }

    /// <summary>
    /// 导入实例
    /// </summary>
    /// <param name="list">路径列表</param>
    /// <returns></returns>
    private async Task ImportAsync(List<string> list)
    {
        bool ok = false;
        foreach (var item in list)
        {
            var dialog = Window.ShowProgress(LanguageUtils.Get("AddGameWindow.Tab3.Text4"));
            var zip = new ZipGui(Window, dialog);
            var res = await GameBinding.AddGameAsync(null, item, null, Group, false, new OverGameGui(Window), zip);
            zip.Stop();
            Window.CloseDialog(dialog);

            if (!res.State)
            {
                var res1 = await Window.ShowChoice(LanguageUtils.Get("AddGameWindow.Tab3.Text13"));
                if (!res1)
                {
                    return;
                }
                continue;
            }

            ok = true;
        }

        if (ok)
        {
            Done(null);
        }
        else
        {
            Window.Show(LanguageUtils.Get("AddGameWindow.Tab3.Text14"));
        }
    }
}
