using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Model;

/// <summary>
/// 文件列表树
/// </summary>
public class FilesPage
{
    /// <summary>
    /// 根路径
    /// </summary>
    private readonly FileTreeNodeModel _root;
    /// <summary>
    /// 显示内容
    /// </summary>
    public HierarchicalTreeDataGridSource<FileTreeNodeModel> Source { get; init; }

    public FilesPage(string obj, bool check, List<string>? unselect = null)
    {
        _root = new FileTreeNodeModel(null, obj, true, check, true);
        Source = new HierarchicalTreeDataGridSource<FileTreeNodeModel>(_root)
        {
            Columns =
            {
                new TemplateColumn<FileTreeNodeModel>(
                    null,
                    cellTemplateResourceKey: "FileNameCell1",
                    options: new TemplateColumnOptions<FileTreeNodeModel>
                    {
                        CanUserResizeColumn = false
                    }),
                new HierarchicalExpanderColumn<FileTreeNodeModel>(
                    new TemplateColumn<FileTreeNodeModel>(
                        LangUtils.Get("Text.FileName"),
                        "FileNameCell",
                        null,
                        new GridLength(1, GridUnitType.Star),
                        new TemplateColumnOptions<FileTreeNodeModel>
                        {
                            CompareAscending = FileTreeNodeModel.SortAscending(x => x.Name),
                            CompareDescending = FileTreeNodeModel.SortDescending(x => x.Name),
                            IsTextSearchEnabled = true,
                            TextSearchValueSelector = x => x.Name
                        }),
                    x => x.Children,
                    x => x.HasChildren,
                    x => x.IsExpanded),
                new TextColumn<FileTreeNodeModel, long?>(
                    LangUtils.Get("Text.Size"),
                    x => x.Size,
                    options: new TextColumnOptions<FileTreeNodeModel>
                    {
                        CompareAscending = FileTreeNodeModel.SortAscending(x => x.Size),
                        CompareDescending = FileTreeNodeModel.SortDescending(x => x.Size),
                    }),
                new TextColumn<FileTreeNodeModel, string?>(
                    LangUtils.Get("GameExportWindow.Text3"),
                    x => x.Modified,
                    options: new TextColumnOptions<FileTreeNodeModel>
                    {
                        CompareAscending = FileTreeNodeModel.SortAscending(x => x.Modified),
                        CompareDescending = FileTreeNodeModel.SortDescending(x => x.Modified),
                    }),
            }
        };

        Source.RowSelection!.SingleSelect = false;

        if (unselect != null)
        {
            foreach (var item in unselect)
            {
                foreach (var item1 in _root.Children!)
                {
                    if (item1.Name == item || item1.Path == item)
                    {
                        item1.IsChecked = false;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 设置未选择的文件
    /// </summary>
    /// <param name="config"></param>
    public void SetUnSelectItems(List<string> config)
    {
        foreach (var item in config)
        {
            _root.UnSelect(item);
        }
    }
    /// <summary>
    /// 获取所有未选择的文件
    /// </summary>
    /// <returns></returns>
    public List<string> GetUnSelectItems()
    {
        return _root.GetUnSelectItems();
    }
    /// <summary>
    /// 获取所有选中的文件
    /// </summary>
    /// <param name="getdir">是否获取目录</param>
    /// <returns></returns>
    public List<string> GetSelectItems(bool getdir = false)
    {
        return _root.GetSelectItems(getdir);
    }
    /// <summary>
    /// 设置选中的文件
    /// </summary>
    /// <param name="config"></param>
    public void SetSelectItems(List<string> config)
    {
        foreach (var item in config)
        {
            _root.Select(item);
        }
    }
    /// <summary>
    /// 设置所有文件选中
    /// </summary>
    public void SetSelectItems()
    {
        _root.Select();
    }
    /// <summary>
    /// 设置所有文件不选中
    /// </summary>
    public void SetUnSelectItems()
    {
        _root.UnSelect();
    }
}