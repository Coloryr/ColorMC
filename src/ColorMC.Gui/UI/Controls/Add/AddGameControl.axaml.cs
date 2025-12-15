using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using ColorMC.Core;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Animations;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Add;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.Add;

/// <summary>
/// 添加游戏实例窗口
/// </summary>
public partial class AddGameControl : BaseUserControl
{
    private AddGameTab1Control? _tab1;
    private AddGameTab2Control? _tab2;
    private AddGameTab3Control? _tab3;

    private CancellationTokenSource? _animationCts;
    private CancellationTokenSource? _mainMenuCts;

    public AddGameControl() : base(nameof(AddGameControl))
    {
        InitializeComponent();

        Title = LangUtils.Get("AddGameWindow.Title");

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);
    }

    /// <summary>
    /// 子页面进入动画：从下往上弹出 (TranslateY: 150 -> 0)
    /// </summary>
    private async void AnimateSubPageIn()
    {
        _animationCts?.Cancel();
        _animationCts?.Dispose();
        _animationCts = new CancellationTokenSource();
        var token = _animationCts.Token;
        
        Content1.IsVisible = true;

        try
        {
            await SubPageIn.Start(Content1, token);
        }
        catch (OperationCanceledException)
        {
            
        }
    }

    /// <summary>
    /// 子页面退出动画：从上往下沉底 (TranslateY: 0 -> 150)
    /// </summary>
    private async void AnimateSubPageOut()
    {
        _animationCts?.Cancel();
        _animationCts?.Dispose();
        _animationCts = new CancellationTokenSource();
        var token = _animationCts.Token;

        try
        {
            await SubPageOut.Start(Content1, token);

            if (!token.IsCancellationRequested)
            {
                Content1.IsVisible = false;
            }
        }
        catch (OperationCanceledException)
        {
            
        }
    }

    /// <summary>
    /// 主界面进入：缩放 0.9 -> 1.0, 不透明度 0 -> 1
    /// </summary>
    private async void AnimateMainMenuIn()
    {
        _mainMenuCts?.Cancel();
        _mainMenuCts?.Dispose();
        _mainMenuCts = new CancellationTokenSource();
        var token = _mainMenuCts.Token;

        // 确保可见
        MainMenuGrid.IsVisible = true;

        try 
        { 
            await MainMenuIn.Start(MainMenuGrid, token); 
        } 
        catch (OperationCanceledException) 
        { 
        
        }
    }

    /// <summary>
    /// 主界面退出：缩放 1.0 -> 0.9, 不透明度 1 -> 0
    /// </summary>
    private async void AnimateMainMenuOut()
    {
        _mainMenuCts?.Cancel();
        _mainMenuCts?.Dispose();
        _mainMenuCts = new CancellationTokenSource();
        var token = _mainMenuCts.Token;

        try
        {
            await MainMenuOut.Start(MainMenuGrid, token);

            // 动画结束后，如果不希望它挡住鼠标事件（虽然我们有 Binding Main），
            // 可以在这里设置 IsVisible = false。
            // 但为了保持“Zoom Back”的流畅性，保持 Visible 但 Opacity 0 也是可以的。
            // 如果需要彻底隐藏：
            if (!token.IsCancellationRequested)
            {
                MainMenuGrid.IsVisible = false;
            }
        }
        catch (OperationCanceledException) 
        { 
        
        }
    }

    protected override ControlModel GenModel(WindowModel model)
    {
        var amodel = new AddGameModel(model);
        amodel.PropertyChanged += Model_PropertyChanged;
        return amodel;
    }

    public override void Closed()
    {
        WindowManager.AddGameWindow = null;
    }

    /// <summary>
    /// 设置添加的文件
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="isDir">是否是目录</param>
    public void AddFile(string path, bool isDir)
    {
        var model = (DataContext as AddGameModel)!;
        if (isDir)
        {
            model.GoTab(AddGameModel.NameTab3);
            model.SetPath(path);
        }
        else
        {
            model.GoTab(AddGameModel.NameTab2);
            model.SetFile(path);
        }
    }

    /// <summary>
    /// 预设游戏分组
    /// </summary>
    /// <param name="group"></param>
    public void SetGroup(string? group)
    {
        if (DataContext is not AddGameModel model)
        {
            return;
        }

        model.DefaultGroup ??= group;
        model.Group ??= group;
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AddGameModel.Main))
        {
            var model = (AddGameModel)DataContext!;
            if (model.Main)
            {
                AnimateSubPageOut();
                AnimateMainMenuIn();
            }
            else
            {
                AnimateMainMenuOut();
                AnimateSubPageIn();
            }
        }
        else
        {
            Content1.Child = e.PropertyName switch
            {
                AddGameModel.NameTab1 => _tab1 ??= new(),
                AddGameModel.NameTab2 => _tab2 ??= new(),
                AddGameModel.NameTab3 => _tab3 ??= new(),
                _ => Content1.Child
            };
        }
    }

    private void DragEnter(object? sender, DragEventArgs e)
    {
        //是否是文件拖拽
        if (e.DataTransfer.Contains(DataFormat.File))
        {
            //取出文件信息
            var files = e.DataTransfer.TryGetFiles();
            if (files == null || files.Length > 1)
            {
                return;
            }

            var item = files.FirstOrDefault();
            switch (item)
            {
                case null:
                    return;
                //判断路径还是文件显示文字
                case IStorageFolder forder when Directory.Exists(forder.GetPath()):
                    Grid2.IsVisible = true;
                    Label1.Text = LangUtils.Get("AddGameWindow.Text2");
                    break;
                default:
                    {
                        if (item.Name.EndsWith(Names.NameZipExt) || item.Name.EndsWith(Names.NameMrpackExt))
                        {
                            Grid2.IsVisible = true;
                            Label1.Text = LangUtils.Get("MainWindow.Text25");
                        }

                        break;
                    }
            }
        }
    }

    private void DragLeave(object? sender, DragEventArgs e)
    {
        Grid2.IsVisible = false;
    }

    private void Drop(object? sender, DragEventArgs e)
    {
        Grid2.IsVisible = false;
        if (!e.DataTransfer.Contains(DataFormat.File))
        {
            return;
        }

        //取出文件信息
        var files = e.DataTransfer.TryGetFiles();
        if (files == null || files.Length > 1)
        {
            return;
        }

        var item = files.FirstOrDefault();
        if (item == null)
        {
            return;
        }

        //只导入支持的类型
        var model = (DataContext as AddGameModel)!;

        if (item is IStorageFolder forder && Directory.Exists(forder.GetPath()))
        {
            model.GoTab(AddGameModel.NameTab3);
            model.SetPath(item.GetPath()!);
        }
        else if (item.Name.EndsWith(Names.NameZipExt) || item.Name.EndsWith(Names.NameMrpackExt))
        {
            model.GoTab(AddGameModel.NameTab2);
            model.SetFile(item.GetPath()!);
        }
    }
}
