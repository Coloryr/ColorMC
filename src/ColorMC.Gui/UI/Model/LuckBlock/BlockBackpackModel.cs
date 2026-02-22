using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AvaloniaEdit.Utils;
using ColorMC.Core.LaunchPath;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.LuckBlock;

/// <summary>
/// 方块背包界面
/// </summary>
public partial class BlockBackpackModel(WindowModel model) : ControlModel(model), IBlockTop
{
    private readonly List<BlockItemModel> _items = [];

    /// <summary>
    /// 方块列表
    /// </summary>
    public ObservableCollection<BlockItemModel> Blocks { get; init; } = [];

    /// <summary>
    /// 是否没有方块列表
    /// </summary>
    [ObservableProperty]
    private bool _isEmpty;

    /// <summary>
    /// 方块搜索
    /// </summary>
    [ObservableProperty]
    private string? _text;

    [RelayCommand]
    public void OpenLuck()
    {
        WindowManager.ShowLuck();
    }

    partial void OnTextChanged(string? value)
    {
        LoadBlock();
    }

    /// <summary>
    /// 加载方块列表
    /// </summary>
    public async void Load()
    {
        _items.Clear();
        var dialog = Window.ShowProgress(LangUtils.Get("LuckBlockWindow.Text5"));
        var res = await BaseBinding.StartLoadBlock();
        Window.CloseDialog(dialog);
        if (!res.State)
        {
            await Window.ShowWait(res.Data!);
            WindowClose();
            return;
        }

        var list = await BaseBinding.BuildUnlockItems();
        if (list == null)
        {
            await Window.ShowWait(LangUtils.Get("LuckBlockWindow.Text9"));
            WindowClose();
            return;
        }

        foreach (var item in list)
        {
            item.Top = this;
            _items.Add(item);
        }

        IsEmpty = !_items.Any();
        if (!IsEmpty)
        {
            LoadBlock();
        }
    }

    private void LoadBlock()
    {
        Blocks.Clear();
        if (string.IsNullOrWhiteSpace(Text))
        {
            Blocks.AddRange(_items);
        }
        else
        {
            Blocks.AddRange(_items.Where(item => item.Name.Contains(Text) || item.Key.Contains(Text)));
        }
    }

    public override void Close()
    {
        Blocks.Clear();
    }

    /// <summary>
    /// 右键使用改方块
    /// </summary>
    /// <param name="model">方块</param>
    public async void Use(BlockItemModel model)
    {
        var list = InstancesPath.Games;
        var names = list.Select(item => item.Name);
        var dialog = new SelectModel(Window.WindowId)
        {
            Text = LangUtils.Get("BlockBackpackWindow.Text1"),
            Items = [.. names]
        };
        var res = await Window.ShowDialogWait(dialog);
        if (res is not true || dialog.Index == -1)
        {
            return;
        }

        var game = list[dialog.Index];
        GameBinding.SetGameIconBlock(game, model.Key);
    }
}
