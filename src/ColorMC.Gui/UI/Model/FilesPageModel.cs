using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Model;

public class FilesPageModel
{
    private readonly FileTreeNodeModel _root;
    public HierarchicalTreeDataGridSource<FileTreeNodeModel> Source { get; init; }

    public FilesPageModel(string obj, bool check, List<string>? unselect = null)
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
                        App.Lang("Text.FileName"),
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
                    App.Lang("Text.Size"),
                    x => x.Size,
                    options: new TextColumnOptions<FileTreeNodeModel>
                    {
                        CompareAscending = FileTreeNodeModel.SortAscending(x => x.Size),
                        CompareDescending = FileTreeNodeModel.SortDescending(x => x.Size),
                    }),
                new TextColumn<FileTreeNodeModel, string?>(
                    App.Lang("GameExportWindow.Info5"),
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

    public void SetUnSelectItems(List<string> config)
    {
        foreach (var item in config)
        {
            _root.UnSelect(item);
        }
    }

    public List<string> GetUnSelectItems()
    {
        return _root.GetUnSelectItems();
    }

    public List<string> GetSelectItems(bool getdir = false)
    {
        return _root.GetSelectItems(getdir);
    }

    public void SetSelectItems(List<string> config)
    {
        foreach (var item in config)
        {
            _root.Select(item);
        }
    }

    public void SetSelectItems()
    {
        _root.Select();
    }

    public void SetUnSelectItems()
    {
        _root.UnSelect();
    }
}