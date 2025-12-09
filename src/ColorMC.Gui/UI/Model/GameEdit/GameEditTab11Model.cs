using System.Collections.ObjectModel;
using Avalonia.Input;
using AvaloniaEdit.Utils;
using ColorMC.Core.Game;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.Manager;
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
        var dialog = Window.ShowProgress(LangUtils.Get("GameEditWindow.Tab11.Text6"));
        ShaderpackList.Clear();
        ShaderpackList.AddRange(await _obj.GetShaderpacksAsync());
        Window.CloseDialog(dialog);

        ShaderpackEmptyDisplay = ShaderpackList.Count == 0;
        Window.Notify(LangUtils.Get("GameEditWindow.Tab11.Text5"));
    }

    /// <summary>
    /// 导入光影包
    /// </summary>
    private async void ImportShaderpack()
    {
        var top = Window.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var res = await PathBinding.AddFileAsync(top, _obj, FileType.Shaderpack);
        if (res == null)
            return;

        if (res == false)
        {
            Window.Notify(LangUtils.Get("GameEditWindow.Tab11.Text7"));
            return;
        }

        Window.Notify(LangUtils.Get("GameEditWindow.Tab11.Text3"));
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
    public async void DropShaderpack(IDataTransfer data)
    {
        var res = await GameBinding.AddFileAsync(_obj, data, FileType.Shaderpack);
        if (res)
        {
            LoadShaderpack();
        }
    }

    /// <summary>
    /// 删除光影包
    /// </summary>
    /// <param name="obj">光影包</param>
    public async void DeleteShaderpack(ShaderpackObj obj)
    {
        var res = await Window.ShowChoice(LangUtils.Get("GameEditWindow.Tab11.Text4"));
        if (!res)
        {
            return;
        }
        await obj.DeleteAsync();
        Window.Notify(LangUtils.Get("GameEditWindow.Tab10.Text6"));
        LoadShaderpack();
    }

    /// <summary>
    /// 删除选中光影包
    /// </summary>
    public void DeleteShaderpack()
    {
        if (ShaderpackItem is { } item)
        {
            DeleteShaderpack(item);
        }
    }
}
