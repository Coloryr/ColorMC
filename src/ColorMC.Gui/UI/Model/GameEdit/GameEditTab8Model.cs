using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Input;
using AvaloniaEdit.Utils;
using ColorMC.Core.Game;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.GameEdit;

/// <summary>
/// 游戏实例编辑
/// </summary>
public partial class GameEditModel
{
    /// <summary>
    /// 资源包列表
    /// </summary>
    public ObservableCollection<ResourcePackModel> ResourcePackList { get; init; } = [];
    /// <summary>
    /// 资源包
    /// </summary>
    private readonly List<ResourcePackModel> _resourceItems = [];
    /// <summary>
    /// 选中的资源包
    /// </summary>
    private ResourcePackModel? _lastResource;

    /// <summary>
    /// 资源筛选
    /// </summary>
    [ObservableProperty]
    private string _resourceText;
    /// <summary>
    /// 是否没有资源文件
    /// </summary>
    [ObservableProperty]
    private bool _resourceEmptyDisplay;

    partial void OnResourceTextChanged(string value)
    {
        LoadResourceDisplay();
    }

    /// <summary>
    /// 加载资源列表
    /// </summary>
    /// <returns></returns>
    public async void LoadResource()
    {
        var dialog = Window.ShowProgress(LangUtils.Get("GameEditWindow.Tab8.Text5"));
        _resourceItems.Clear();

        var res = await _obj.GetResourcepacksAsync(false);
        Window.CloseDialog(dialog);
        foreach (var item in res)
        {
            _resourceItems.Add(new(this, item));
        }

        LoadResourceDisplay();
        Window.Notify(LangUtils.Get("GameEditWindow.Tab8.Text7"));
    }

    /// <summary>
    /// 加载资源列表
    /// </summary>
    private void LoadResourceDisplay()
    {
        ResourcePackList.Clear();
        if (string.IsNullOrWhiteSpace(ResourceText))
        {
            ResourcePackList.AddRange(_resourceItems);
        }
        else
        {
            ResourcePackList.AddRange(_resourceItems.Where(item => item.Local.Contains(ResourceText)));
        }

        ResourceEmptyDisplay = ResourcePackList.Count == 0;
    }

    /// <summary>
    /// 下载资源包
    /// </summary>
    private void AddResource()
    {
        WindowManager.ShowAdd(_obj, FileType.Resourcepack);
    }

    /// <summary>
    /// 导入资源
    /// </summary>
    private async void ImportResource()
    {
        var top = Window.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.AddFileAsync(top, _obj, FileType.Resourcepack);
        if (file == null)
            return;

        if (file == false)
        {
            Window.Notify(LangUtils.Get("GameEditWindow.Tab8.Text8"));
            return;
        }

        Window.Notify(LangUtils.Get("GameEditWindow.Tab4.Text20"));
        LoadResource();
    }

    /// <summary>
    /// 打开资源列表
    /// </summary>
    private void OpenResource()
    {
        PathBinding.OpenPath(_obj, PathType.ResourcepackPath);
    }

    /// <summary>
    /// 删除资源
    /// </summary>
    /// <param name="obj">资源包</param>
    public async void DeleteResource(ResourcepackObj obj)
    {
        var res = await Window.ShowChoice(
            string.Format(LangUtils.Get("GameEditWindow.Tab8.Text4"), obj.Local));
        if (!res)
        {
            return;
        }

        await obj.Delete();
        Window.Notify(LangUtils.Get("Text.DeleteDone"));
        LoadResource();
    }

    /// <summary>
    /// 拖拽导入资源包
    /// </summary>
    /// <param name="data">资源包</param>
    public async void DropResource(IDataTransfer data)
    {
        var res = await GameBinding.AddFileAsync(_obj, data, FileType.Resourcepack);
        if (res)
        {
            LoadResource();
        }
    }

    /// <summary>
    /// 选中资源包
    /// </summary>
    /// <param name="item">资源包</param>
    public void SetSelectResource(ResourcePackModel item)
    {
        _lastResource?.IsSelect = false;
        _lastResource = item;
        _lastResource.IsSelect = true;
    }
}