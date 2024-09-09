using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddGameModel
{
    /// <summary>
    /// 文件列表
    /// </summary>
    private FilesPageModel? _fileModel;

    /// <summary>
    /// 文件夹路径
    /// </summary>
    [ObservableProperty]
    private string? _selectPath;

    /// <summary>
    /// 文件列表
    /// </summary>
    [ObservableProperty]
    private HierarchicalTreeDataGridSource<FileTreeNodeModel>? _files;

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
            var res = await Model.ShowWait(string.Format(App.Lang("AddGameWindow.Tab3.Info3"), SelectPath));
            if (!res)
            {
                return;
            }
            Model.Progress(App.Lang("AddGameWindow.Tab3.Info2"));

            var list = GameHelper.ScanVersions(SelectPath);
            if (list.Count > 0)
            {
                res = await Model.ShowWait(string.Format(App.Lang("AddGameWindow.Tab3.Info4"), list.Count));
                if (res)
                {
                    await Import(list);
                    return;
                }
            }

            _fileModel = await Task.Run(() =>
            {
                return new FilesPageModel(SelectPath, true,
                    ["assets", "libraries", "versions", "launcher_profiles.json"]);
            });
            Model.ProgressClose();
            Files = _fileModel.Source;

            CanInput = true;
        }
        else
        {
            CanInput = false;
            Files = null!;
            _fileModel = null!;
            Model.Show(string.Format(App.Lang("AddGameWindow.Tab1.Error2"), SelectPath));
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
            Model.Show(App.Lang("AddGameWindow.Tab1.Error2"));
            return;
        }

        if (PathHelper.FileHasInvalidChars(Name))
        {
            Model.Show(App.Lang("AddGameWindow.Tab1.Error13"));
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectPath))
        {
            Model.Show(App.Lang("AddGameWindow.Tab3.Error3"));
            return;
        }

        if (_fileModel == null)
        {
            Model.Show(App.Lang("AddGameWindow.Tab3.Error4"));
            return;
        }

        Model.Progress(App.Lang("AddGameWindow.Tab3.Info1"));
        var res = await GameBinding.AddGame(Name, SelectPath, _fileModel.GetUnSelectItems(),
            Group, GameRequest, GameOverwirte, Update, true);
        Model.ProgressClose();

        if (!res.State)
        {
            Model.Show(App.Lang("AddGameWindow.Tab3.Error1"));
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
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var res = await PathBinding.SelectPath(top, PathType.GamePath);
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
            Model.Show(string.Format(App.Lang("AddGameWindow.Tab3.Error2"), res));
        }
    }

    public async void SetPath(string res)
    {
        SelectPath = res;

        await RefashFiles();
    }

    private async Task Import(List<string> list)
    {
        BaseBinding.IsAddGames = true;
        bool ok = false;
        foreach (var item in list)
        {
            Model.Progress(App.Lang("AddGameWindow.Tab3.Info1"));
            var res = await GameBinding.AddGame(null, item, null, Group, GameRequest, GameOverwirte, Update, false);
            Model.ProgressClose();

            if (!res.State)
            {
                var res1 = await Model.ShowWait(App.Lang("AddGameWindow.Tab3.Error5"));
                if (!res1)
                {
                    return;
                }
                continue;
            }

            ok = true;
        }
        BaseBinding.IsAddGames = false;

        if (ok)
        {
            Done(null);
        }
    }

    private void Update(string text, int size, int all)
    {
        if (text.Length > 40)
        {
            text = "..." + text[^40..];
        }
        Dispatcher.UIThread.Post(() =>
        {
            Model.ProgressUpdate(text);
            Model.ProgressUpdate((double)size / all * 100);
        });
    }
}
