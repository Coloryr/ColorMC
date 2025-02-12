using System.Collections.ObjectModel;
using Avalonia.Input;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.GameEdit;

/// <summary>
/// 游戏实例编辑
/// </summary>
public partial class GameEditModel
{
    /// <summary>
    /// 光影包列表
    /// </summary>
    public ObservableCollection<ShaderpackObj> ShaderpackList { get; init; } = [];

    /// <summary>
    /// 选中的光影包
    /// </summary>
    [ObservableProperty]
    private ShaderpackObj? _shaderpackItem;

    /// <summary>
    /// 是否没有光影包
    /// </summary>
    [ObservableProperty]
    private bool _shaderpackEmptyDisplay;

    /// <summary>
    /// 打开光影包路径
    /// </summary>
    private void OpenShaderpack()
    {
        PathBinding.OpenPath(_obj, PathType.ShaderpacksPath);
    }

    /// <summary>
    /// 加载光影包列表
    /// </summary>
    public async void LoadShaderpack()
    {
        Model.Progress(App.Lang("GameEditWindow.Tab11.Info4"));
        ShaderpackList.Clear();
        ShaderpackList.AddRange(await GameBinding.GetShaderpacks(_obj));
        Model.ProgressClose();

        ShaderpackEmptyDisplay = ShaderpackList.Count == 0;
        Model.Notify(App.Lang("GameEditWindow.Tab11.Info3"));
    }

    /// <summary>
    /// 导入光影包
    /// </summary>
    private async void ImportShaderpack()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var res = await PathBinding.AddFile(top, _obj, FileType.Shaderpack);
        if (res == null)
            return;

        if (res == false)
        {
            Model.Notify(App.Lang("GameEditWindow.Tab11.Error1"));
            return;
        }

        Model.Notify(App.Lang("GameEditWindow.Tab11.Info1"));
        LoadShaderpack();
    }

    /// <summary>
    /// 下载光影包
    /// </summary>
    private void AddShaderpack()
    {
        WindowManager.ShowAdd(_obj, FileType.Shaderpack);
    }

    /// <summary>
    /// 拖拽添加光影包
    /// </summary>
    /// <param name="data"></param>
    public async void DropShaderpack(IDataObject data)
    {
        var res = await GameBinding.AddFile(_obj, data, FileType.Shaderpack);
        if (res)
        {
            LoadShaderpack();
        }
    }

    /// <summary>
    /// 删除光影包
    /// </summary>
    /// <param name="obj"></param>
    public async void DeleteShaderpack(ShaderpackObj obj)
    {
        var res = await Model.ShowAsync(App.Lang("GameEditWindow.Tab11.Info2"));
        if (!res)
        {
            return;
        }
        GameBinding.DeleteShaderpack(obj);
        Model.Notify(App.Lang("GameEditWindow.Tab10.Info5"));
        LoadShaderpack();
    }
}
