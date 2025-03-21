using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls;
using ColorMC.Gui.UI.Model.Items;
using System.Collections.Generic;

namespace ColorMC.Gui.UI.Model;

public class GamesPage
{
    private readonly GameFileTreeNodeModel _root;
    public HierarchicalTreeDataGridSource<GameFileTreeNodeModel> Source { get; init; }

    public GamesPage()
    {
        _root = new GameFileTreeNodeModel(null, null, true, null, true);
        Source = new HierarchicalTreeDataGridSource<GameFileTreeNodeModel>(_root)
        {
            Columns =
            {
                new TemplateColumn<GameFileTreeNodeModel>(
                    null,
                    cellTemplateResourceKey: "GameFileNameCell1",
                    options: new TemplateColumnOptions<GameFileTreeNodeModel>
                    {
                        CanUserResizeColumn = false
                    }),
                new HierarchicalExpanderColumn<GameFileTreeNodeModel>(
                    new TemplateColumn<GameFileTreeNodeModel>(
                        App.Lang("Text.FileName"),
                        "GameFileNameCell",
                        null,
                        new GridLength(1, GridUnitType.Star),
                        new TemplateColumnOptions<GameFileTreeNodeModel>
                        {
                            CompareAscending = GameFileTreeNodeModel.SortAscending(x => x.Name),
                            CompareDescending = GameFileTreeNodeModel.SortDescending(x => x.Name),
                            IsTextSearchEnabled = true,
                            TextSearchValueSelector = x => x.Name
                        }),
                    x => x.Children,
                    x => x.HasChildren,
                    x => x.IsExpanded),
                new TextColumn<GameFileTreeNodeModel, long?>(
                    App.Lang("Text.Size"),
                    x => x.Size,
                    options: new TextColumnOptions<GameFileTreeNodeModel>
                    {
                        CompareAscending = GameFileTreeNodeModel.SortAscending(x => x.Size),
                        CompareDescending = GameFileTreeNodeModel.SortDescending(x => x.Size),
                    }),
                new TextColumn<GameFileTreeNodeModel, string?>(
                    App.Lang("GameExportWindow.Info5"),
                    x => x.Modified,
                    options: new TextColumnOptions<GameFileTreeNodeModel>
                    {
                        CompareAscending = GameFileTreeNodeModel.SortAscending(x => x.Modified),
                        CompareDescending = GameFileTreeNodeModel.SortDescending(x => x.Modified),
                    }),
            }
        };

        Source.RowSelection!.SingleSelect = false;
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

    public List<string> GetSelectItems()
    {
        return _root.GetSelectItems(false);
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
