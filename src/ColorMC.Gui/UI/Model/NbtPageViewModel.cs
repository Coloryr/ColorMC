using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Data.Converters;
using Avalonia.Threading;
using ColorMC.Core.Chunk;
using ColorMC.Core.Nbt;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model;

public class NbtPageViewModel : ObservableObject
{
    private readonly NbtNodeModel _root;
    private readonly Action<int> _turn;
    public NbtBase Nbt { get; }
    public HierarchicalTreeDataGridSource<NbtNodeModel> Source { get; }

    public NbtPageViewModel(NbtBase nbt, Action<int> turn)
    {
        Nbt = nbt;
        _turn = turn;
        _root = new NbtNodeModel(null, nbt, null);
        Source = new HierarchicalTreeDataGridSource<NbtNodeModel>(new[] { _root })
        {
            Columns =
            {
                new HierarchicalExpanderColumn<NbtNodeModel>(
                    new TemplateColumn<NbtNodeModel>(
                        "NBT",
                        "NbtCell",
                        null,
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
    }

    public void Find(string name)
    {
        NbtNodeModel? data = _root.Find(name);
        if (data == null)
            return;

        Select(data);
    }

    public NbtNodeModel? Find(NbtNodeModel from, NbtBase nbt)
    {
        NbtNodeModel? model = null;
        foreach (var item in from.Children)
        {
            if (model == null)
            {
                if (item.Nbt == nbt)
                {
                    item.Expand();
                    model = item;
                    break;
                }
                else if (item.HasChildren)
                {
                    model = Find(item, nbt);
                }
            }
            else
            {
                item.UnExpand();
            }
        }

        return model;
    }

    public NbtNodeModel? Select(ChunkNbt nbt)
    {
        NbtNodeModel? data = _root.Find(nbt);
        if (data == null)
            return null;

        Select(data);

        return data;
    }

    public void Select(NbtNodeModel data)
    {
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
            list.SelectedIndex = new(list1);

            var temp = 0;

            foreach(var item in list1)
            {
                temp += item;
            }

            _turn.Invoke(temp);
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