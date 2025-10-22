using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using ColorMC.Core;
using ColorMC.Gui.Manager;
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

    public AddGameControl() : base(nameof(AddGameControl))
    {
        InitializeComponent();

        Title = App.Lang("AddGameWindow.Title");

        AddHandler(DragDrop.DragEnterEvent, DragEnter);
        AddHandler(DragDrop.DragLeaveEvent, DragLeave);
        AddHandler(DragDrop.DropEvent, Drop);
    }

    protected override TopModel GenModel(BaseModel model)
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
        Content1.Child = e.PropertyName switch
        {
            AddGameModel.NameTab1 => _tab1 ??= new(),
            AddGameModel.NameTab2 => _tab2 ??= new(),
            AddGameModel.NameTab3 => _tab3 ??= new(),
            AddGameModel.NameBack => null,
            _ => Content1.Child
        };
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
                    Label1.Text = App.Lang("AddGameWindow.Text2");
                    break;
                default:
                    {
                        if (item.Name.EndsWith(Names.NameZipExt) || item.Name.EndsWith(Names.NameMrpackExt))
                        {
                            Grid2.IsVisible = true;
                            Label1.Text = App.Lang("MainWindow.Text25");
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

    /// <summary>
    /// 设置添加的游戏分组
    /// </summary>
    /// <returns></returns>
    public string? GetGroup()
    {
        return (DataContext as AddGameModel)?.Group;
    }
}
