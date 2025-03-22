using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.Manager;
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
    [RelayCommand]
    public async Task LoadResource()
    {
        Model.Progress(App.Lang("GameEditWindow.Tab8.Info3"));
        _resourceItems.Clear();

        var res = await GameBinding.GetResourcepacks(_obj);
        Model.ProgressClose();
        foreach (var item in res)
        {
            _resourceItems.Add(new(this, item));
        }

        LoadResourceDisplay();
        Model.Notify(App.Lang("GameEditWindow.Tab8.Info5"));
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
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.AddFile(top, _obj, FileType.Resourcepack);
        if (file == null)
            return;

        if (file == false)
        {
            Model.Notify(App.Lang("GameEditWindow.Tab8.Error1"));
            return;
        }

        Model.Notify(App.Lang("GameEditWindow.Tab4.Info2"));
        await LoadResource();
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
        var res = await Model.ShowAsync(
            string.Format(App.Lang("GameEditWindow.Tab8.Info1"), obj.Local));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteResourcepack(obj);
        Model.Notify(App.Lang("GameEditWindow.Tab4.Info3"));
        await LoadResource();
    }

    /// <summary>
    /// 拖拽导入资源包
    /// </summary>
    /// <param name="data">资源包</param>
    public async void DropResource(IDataObject data)
    {
        var res = await GameBinding.AddFile(_obj, data, FileType.Resourcepack);
        if (res)
        {
            await LoadResource();
        }
    }

    /// <summary>
    /// 选中资源包
    /// </summary>
    /// <param name="item">资源包</param>
    public void SetSelectResource(ResourcePackModel item)
    {
        if (_lastResource != null)
        {
            _lastResource.IsSelect = false;
        }
        _lastResource = item;
        _lastResource.IsSelect = true;
    }
}