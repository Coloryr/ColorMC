using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Writers.Zip;

namespace ColorMC.Gui.UI.Model;

/// <summary>
/// 文件列表树
/// </summary>
public class ZipPage
{
    /// <summary>
    /// 根路径
    /// </summary>
    private readonly ZipTreeNodeModel _root;

    public IWritableArchive<ZipWriterOptions> Zip { get; private set; }

    /// <summary>
    /// 显示内容
    /// </summary>
    public HierarchicalTreeDataGridSource<ZipTreeNodeModel> Source { get; init; }

    public ZipPage(string local)
    {
        Zip = ZipArchive.OpenArchive(local);

        _root = new ZipTreeNodeModel(Zip);
        Source = new HierarchicalTreeDataGridSource<ZipTreeNodeModel>(_root)
        {
            Columns =
            {
                new TemplateColumn<ZipTreeNodeModel>(
                    null,
                    cellTemplateResourceKey: "ZipNameCell1",
                    options: new TemplateColumnOptions<ZipTreeNodeModel>
                    {
                        CanUserResizeColumn = false
                    }),
                new HierarchicalExpanderColumn<ZipTreeNodeModel>(
                    new TemplateColumn<ZipTreeNodeModel>(
                        LangUtils.Get("Text.FileName"),
                        "ZipNameCell",
                        null,
                        new GridLength(1, GridUnitType.Star),
                        new TemplateColumnOptions<ZipTreeNodeModel>
                        {
                            CompareAscending = ZipTreeNodeModel.SortAscending(x => x.Name),
                            CompareDescending = ZipTreeNodeModel.SortDescending(x => x.Name),
                            IsTextSearchEnabled = true,
                            TextSearchValueSelector = x => x.Name
                        }),
                    x => x.Children,
                    x => x.HasChildren,
                    x => x.IsExpanded),
                new TextColumn<ZipTreeNodeModel, long?>(
                    LangUtils.Get("Text.Size"),
                    x => x.Size,
                    options: new TextColumnOptions<ZipTreeNodeModel>
                    {
                        CompareAscending = ZipTreeNodeModel.SortAscending(x => x.Size),
                        CompareDescending = ZipTreeNodeModel.SortDescending(x => x.Size),
                    }),
                new TextColumn<ZipTreeNodeModel, long?>(
                    LangUtils.Get("AddGameWindow.Tab2.Text17"),
                    x => x.CompressedSize,
                    options: new TextColumnOptions<ZipTreeNodeModel>
                    {
                        CompareAscending = ZipTreeNodeModel.SortAscending(x => x.CompressedSize),
                        CompareDescending = ZipTreeNodeModel.SortDescending(x => x.CompressedSize),
                    }),
            }
        };

        Source.RowSelection!.SingleSelect = false;
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
    public List<IArchiveEntry> GetUnSelectItems()
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

    public void Dispose()
    {
        if (Zip != null)
        {
            Zip.Dispose();
        }
    }
}