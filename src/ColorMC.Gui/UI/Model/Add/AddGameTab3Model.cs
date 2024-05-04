using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json.Linq;

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
            _fileModel = await Task.Run(() =>
            {
                return new FilesPageModel(SelectPath, true,
                    ["assets", "libraries", "versions", "launcher_profiles.json"]);
            });
            Model.ProgressClose();
            Files = _fileModel.Source;

            var list = DoScan();
            if (list.Count > 0)
            {
                res = await Model.ShowWait(string.Format(App.Lang("AddGameWindow.Tab3.Info4"), list.Count));
                if (res)
                {
                    await Import(list);
                    return;
                }
            }

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
            Group, Tab2GameRequest, Tab2GameOverwirte);
        Model.ProgressClose();

        if (!res)
        {
            Model.Show(App.Lang("AddGameWindow.Tab3.Error1"));
            return;
        }

        var model = (App.MainWindow?.DataContext as MainModel);
        model?.Model.Notify(App.Lang("AddGameWindow.Tab2.Info5"));
        App.MainWindow?.LoadMain();
        WindowClose();
    }

    /// <summary>
    /// 选择文件夹路径
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task SelectLocal()
    {
        var res = await PathBinding.SelectPath(PathType.GamePath);
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

    private List<string> DoScan()
    {
        var list = new List<string>();
        foreach (var item in Files!.Items)
        {
            foreach (var item1 in item.Children)
            {
                if (item1.Name == "versions")
                {
                    foreach (var item2 in item1.Children)
                    {
                        bool find = false;
                        foreach (var item3 in item2.Children)
                        {
                            if (find)
                            {
                                break;
                            }
                            if (item3.Name.EndsWith(".json"))
                            {
                                try
                                {
                                    var obj = JObject.Parse(PathHelper.ReadText(item3.Path)!);
                                    if (obj.ContainsKey("id")
                                        && obj.ContainsKey("arguments")
                                        && obj.ContainsKey("mainClass"))
                                    {
                                        list.Add(item2.Path);
                                        find = true;
                                        break;
                                    }
                                }
                                catch
                                {
                                    
                                }
                            }
                        }
                    }

                    return list;
                }
            }
        }

        return list;
    }

    private async Task Import(List<string> list)
    {
        bool ok = true;
        foreach (var item in list)
        {
            Model.Progress(App.Lang("AddGameWindow.Tab3.Info1"));
            var res = await GameBinding.AddGame(item, Group, Tab2GameRequest, Tab2GameOverwirte);
            Model.ProgressClose();

            if (!res)
            {
                Model.Show(App.Lang("AddGameWindow.Tab3.Error1"));
                ok = false;
                return;
            }
        }

        if (ok)
        {
            var model = (App.MainWindow?.DataContext as MainModel);
            model?.Model.Notify(App.Lang("AddGameWindow.Tab2.Info5"));
            App.MainWindow?.LoadMain();
            WindowClose();
        }
    }
}
