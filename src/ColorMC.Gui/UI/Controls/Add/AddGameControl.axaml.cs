using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using ColorMC.Core;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Add;
using ColorMC.Gui.Utils;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace ColorMC.Gui.UI.Controls.Add;

/// <summary>
/// �����Ϸʵ������
/// </summary>
public partial class AddGameControl : BaseUserControl
{
    private AddGameTab1Control _tab1;
    private AddGameTab2Control _tab2;
    private AddGameTab3Control _tab3;
    private AddGameTab4Control _tab4;

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

    public override Bitmap GetIcon()
    {
        return ImageManager.GameIcon;
    }

    /// <summary>
    /// ������ӵ��ļ�
    /// </summary>
    /// <param name="path">·��</param>
    /// <param name="isDir">�Ƿ���Ŀ¼</param>
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
    /// Ԥ����Ϸ����
    /// </summary>
    /// <param name="group"></param>
    public void SetGroup(string? group)
    {
        if (DataContext is AddGameModel model)
        {
            model.DefaultGroup ??= group;
            model.Group ??= group;
        }
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == AddGameModel.NameTab1)
        {
            Content1.Child = _tab1 ??= new();
        }
        else if (e.PropertyName == AddGameModel.NameTab2)
        {
            Content1.Child = _tab2 ??= new();
        }
        else if (e.PropertyName == AddGameModel.NameTab3)
        {
            Content1.Child = _tab3 ??= new();
        }
        else if (e.PropertyName == AddGameModel.NameTab4)
        {
            Content1.Child = _tab4 ??= new();
        }
        else if (e.PropertyName == AddGameModel.NameBack)
        {
            Content1.Child = null;
        }
    }

    private void DragEnter(object? sender, DragEventArgs e)
    {
        //�Ƿ����ļ���ק
        if (e.Data.Contains(DataFormats.Files))
        {
            //ȡ���ļ���Ϣ
            var files = e.Data.GetFiles();
            if (files == null || files.Count() > 1)
            {
                return;
            }

            var item = files.ToList()[0];
            if (item == null)
            {
                return;
            }

            //�ж�·�������ļ���ʾ����
            if (item is IStorageFolder forder && Directory.Exists(forder.GetPath()))
            {
                Grid2.IsVisible = true;
                Label1.Text = App.Lang("AddGameWindow.Text2");
            }
            else if (item.Name.EndsWith(Names.NameZipExt) || item.Name.EndsWith(Names.NameMrpackExt))
            {
                Grid2.IsVisible = true;
                Label1.Text = App.Lang("MainWindow.Text25");
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
        if (e.Data.Contains(DataFormats.Files))
        {
            //ȡ���ļ���Ϣ
            var files = e.Data.GetFiles();
            if (files == null || files.Count() > 1)
            {
                return;
            }

            var item = files.ToList()[0];
            if (item == null)
            {
                return;
            }

            //ֻ����֧�ֵ�����
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

    /// <summary>
    /// ������ӵ���Ϸ����
    /// </summary>
    /// <returns></returns>
    public string? GetGroup()
    {
        return (DataContext as AddGameModel)?.Group;
    }
}
