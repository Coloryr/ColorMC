using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ColorMC.Gui.UI.Model.GameEdit;

public class FilesPageViewModel : ObservableObject
{
    private readonly FileTreeNodeModel _root;
    public HierarchicalTreeDataGridSource<FileTreeNodeModel> Source { get; }
   
    public FilesPageViewModel(string obj, List<string>? unselect = null)
    {
        Source = new HierarchicalTreeDataGridSource<FileTreeNodeModel>(Array.Empty<FileTreeNodeModel>())
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
                        App.GetLanguage("GameEditWindow.Tab6.Info4"),
                        cellTemplateResourceKey: "FileNameCell",
                        new GridLength(1, GridUnitType.Star),
                        new TemplateColumnOptions<FileTreeNodeModel>
                        {
                            CompareAscending = FileTreeNodeModel.SortAscending(x => x.Name),
                            CompareDescending = FileTreeNodeModel.SortDescending(x => x.Name),
                            IsTextSearchEnabled = true,
                            TextSearchValueSelector = x => x.Name
                        })
                    {

                    },
                    x => x.Children,
                    x => x.HasChildren,
                    x => x.IsExpanded),
                new TextColumn<FileTreeNodeModel, long?>(
                    App.GetLanguage("GameEditWindow.Tab6.Info5"),
                    x => x.Size,
                    options: new TextColumnOptions<FileTreeNodeModel>
                    {
                        CompareAscending = FileTreeNodeModel.SortAscending(x => x.Size),
                        CompareDescending = FileTreeNodeModel.SortDescending(x => x.Size),
                    }),
                new TextColumn<FileTreeNodeModel, DateTimeOffset?>(
                    App.GetLanguage("GameEditWindow.Tab6.Info6"),
                    x => x.Modified,
                    options: new TextColumnOptions<FileTreeNodeModel>
                    {
                        CompareAscending = FileTreeNodeModel.SortAscending(x => x.Modified),
                        CompareDescending = FileTreeNodeModel.SortDescending(x => x.Modified),
                    }),
            }
        };

        Source.RowSelection!.SingleSelect = false;

        _root = new FileTreeNodeModel(obj, true, true);
        Source.Items = new[] { _root };

        if (unselect != null)
        {
            foreach (var item in unselect)
            {
                foreach (var item1 in _root.Children!)
                {
                    if (item1.Path.EndsWith(item))
                    {
                        item1.IsChecked = false;
                    }
                }
            }
        }
    }

    public List<string> GetUnSelectItems()
    {
        return _root.GetUnSelectItems();
    }


    private static IconConverter? s_iconConverter;
    public static IMultiValueConverter FileIconConverter
    {
        get
        {
            if (s_iconConverter is null)
            {
                s_iconConverter = new IconConverter();
            }

            return s_iconConverter;
        }
    }

    private class IconConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count == 2 &&
                values[0] is bool isDirectory &&
                values[1] is bool isExpanded)
            {
                if (!isDirectory)
                    return "[F]";
                else
                    return isExpanded ? "{O}" : "{ }";
            }

            return null;
        }
    }
}