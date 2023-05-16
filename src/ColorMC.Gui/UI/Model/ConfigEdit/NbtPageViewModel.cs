using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Data.Converters;
using ColorMC.Core.Nbt;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ColorMC.Gui.UI.Model.ConfigEdit;

public class NbtPageViewModel : ObservableObject
{
    private readonly NbtNodeModel _root;
    public readonly HierarchicalTreeDataGridSource<NbtNodeModel> Source { get; }

    public NbtPageViewModel(NbtCompound nbt)
    {
        Source = new HierarchicalTreeDataGridSource<NbtNodeModel>(Array.Empty<NbtNodeModel>())
        {
            Columns =
            {
                new HierarchicalExpanderColumn<NbtNodeModel>(
                    new TemplateColumn<NbtNodeModel>(
                        "NBT",
                        cellTemplateResourceKey: "NbtCell",
                        new GridLength(1, GridUnitType.Auto),
                        new TemplateColumnOptions<NbtNodeModel>
                        {
                            IsTextSearchEnabled = true,
                            TextSearchValueSelector = x => x.Name,
                            CanUserResizeColumn = true,
                            CanUserSortColumn = false,
                        })
                    {

                    },
                    x => x.Children,
                    x => x.HasChildren,
                    x => x.IsExpanded)
            }
        };

        Source.RowSelection!.SingleSelect = false;

        _root = new NbtNodeModel(null, nbt);
        Source.Items = new[] { _root };
    }

    private static StringNbtTypeConverter? s_NbtTypeConverter;
    public static IMultiValueConverter NbtTypeConverter
    {
        get
        {
            if (s_NbtTypeConverter is null)
            {
                s_NbtTypeConverter = new StringNbtTypeConverter();
            }

            return s_NbtTypeConverter;
        }
    }

    private class StringNbtTypeConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count == 1 &&
                values[0] is NbtType type)
            {
                return type switch
                {
                    NbtType.NbtEnd => "E",
                    NbtType.NbtByte => "B",
                    NbtType.NbtShort => "S",
                    NbtType.NbtInt => "I",
                    NbtType.NbtLong => "L",
                    NbtType.NbtFloat => "F",
                    NbtType.NbtDouble => "D",
                    NbtType.NbtByteArray => "[B]",
                    NbtType.NbtString => "T",
                    NbtType.NbtList => "[ ]",
                    NbtType.NbtCompound => "{ }",
                    NbtType.NbtIntArray => "[I]",
                    NbtType.NbtLongArray => "[L]",
                    _ => ""
                };
            }

            return "";
        }
    }
}