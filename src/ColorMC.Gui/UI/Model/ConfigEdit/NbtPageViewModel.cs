using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Data.Converters;
using Avalonia.Threading;
using ColorMC.Core.Nbt;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.ConfigEdit;

public class NbtPageViewModel : ObservableObject
{
    private readonly NbtNodeModel _root;

    public NbtBase Nbt { get; }
    public HierarchicalTreeDataGridSource<NbtNodeModel> Source { get; }

    public NbtPageViewModel(NbtCompound nbt)
    {
        Nbt = nbt;
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
                            TextSearchValueSelector = x => x.Key,
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

        _root = new NbtNodeModel(null, nbt, null);
        Source.Items = new[] { _root };
    }

    public async void Find(string name)
    {
        NbtNodeModel? data = null;
        await Task.Run(() => data = _root.Find(name));
        if (data == null)
            return;

        var list = new List<int>();
        NbtNodeModel? top = data.Top;
        NbtNodeModel? down = data;
        while (top != null)
        {
            list.Add(top.Children.IndexOf(down));
            top.Expand();
            down = top;
            top = top.Top;
        }
        list.Add(0);
        list.Reverse();

        var list1 = list.ToArray();
        Dispatcher.UIThread.Post(() =>
        {
            var list = (Source.Selection as TreeDataGridRowSelectionModel<NbtNodeModel>)!;
            int a;
            for (a = 0; a < Source.Rows.Count; a++)
            {
                if (Source.Rows[a].Model == data)
                    break;
            }
            list.SelectedIndex = new(list1);
        });
    }

    private static StringNbtTypeConverter? s_NbtTypeConverter;
    public static IMultiValueConverter NbtTypeConverter
    {
        get
        {
            s_NbtTypeConverter ??= new StringNbtTypeConverter();

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