using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Model;

/// <summary>
/// 游戏实例树
/// </summary>
public class GamesPage
{
    /// <summary>
    /// 根路径
    /// </summary>
    private readonly GameFileTreeNodeModel _root;
    /// <summary>
    /// 显示内容
    /// </summary>
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
                        LanguageUtils.Get("Text.FileName"),
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
                    LanguageUtils.Get("Text.Size"),
                    x => x.Size,
                    options: new TextColumnOptions<GameFileTreeNodeModel>
                    {
                        CompareAscending = GameFileTreeNodeModel.SortAscending(x => x.Size),
                        CompareDescending = GameFileTreeNodeModel.SortDescending(x => x.Size),
                    }),
                new TextColumn<GameFileTreeNodeModel, string?>(
                    LanguageUtils.Get("GameExportWindow.Text3"),
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

    /// <summary>
    /// 获取所有选中的文件
    /// </summary>
    /// <returns></returns>
    public List<string> GetSelectItems()
    {
        return _root.GetSelectItems(false);
    }

    //public void SetSelectItems()
    //{
    //    _root.Select();
    //}

    //public void SetUnSelectItems()
    //{
    //    _root.UnSelect();
    //}
}
