using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
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

    [ObservableProperty]
    private bool _importDisplay;
    [ObservableProperty]
    private bool _langDisplay;

    /// <summary>
    /// 导入路径
    /// </summary>
    [ObservableProperty]
    private string _local;
    /// <summary>
    /// 方块搜索
    /// </summary>
    [ObservableProperty]
    private string? _text;

    [RelayCommand]
    public void ImportBlock()
    {
        ImportDisplay = true;
        LangDisplay = false;
    }

    [RelayCommand]
    public void ImportLang()
    {
        ImportDisplay = false;
        LangDisplay = true;
    }

    [RelayCommand]
    public async Task SelectBlock()
    {
        var top = Window.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectPathAsync(top, PathType.BlockPath);
        if (file != null)
        {
            Local = file;
        }
    }

    [RelayCommand]
    public async Task StartBlock()
    {
        var dialog = new ProgressModel()
        {
            Text = LangUtils.Get("BlockBackpackWindow.Text10")
        };
        Window.ShowDialog(dialog);
        var res = await BlockListUtils.StartImport(Local);
        Window.CloseDialog(dialog);
        if (res)
        {
            Load();
            ImportDisplay = false;
            Window.Notify(LangUtils.Get("BlockBackpackWindow.Text21"));
        }
        else
        {
            Window.Show(LangUtils.Get("BlockBackpackWindow.Text11"));
        }
    }

    [RelayCommand]
    public async Task SelectLang()
    {
        var top = Window.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectFileAsync(top, FileType.Lang);
        if (file.Path != null)
        {
            Local = file.Path;
        }
    }

    [RelayCommand]
    public async Task StartLang()
    {
        var dialog = new ProgressModel()
        {
            Text = LangUtils.Get("BlockBackpackWindow.Text19")
        };
        Window.ShowDialog(dialog);
        var res = await BlockListUtils.StartImportLang(Local);
        Window.CloseDialog(dialog);
        if (res)
        {
            Load();
            LangDisplay = false;
            Window.Notify(LangUtils.Get("BlockBackpackWindow.Text22"));
        }
        else
        {
            Window.Show(LangUtils.Get("BlockBackpackWindow.Text20"));
        }
    }

    [RelayCommand]
    public void CloseView()
    {
        ImportDisplay = false;
        LangDisplay = false;

        Local = "";
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
        var res = await BlockListUtils.StartLoadBlock();
        Window.CloseDialog(dialog);
        if (!res.State)
        {
            await Window.ShowWait(res.Data!);
            WindowClose();
            return;
        }

        var list = await BlockListUtils.BuildBlockList();
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

        LoadBlock();
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
            Blocks.AddRange(_items.Where(item => item.Name?.Contains(Text) == true
                || item.Key.Contains(Text)));
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
