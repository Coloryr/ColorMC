using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Selection;
using Avalonia.Threading;
using ColorMC.Core.Chunk;
using ColorMC.Core.Nbt;
using ColorMC.Gui.UI.Model.Items;

namespace ColorMC.Gui.UI.Model.GameConfigEdit;

public class NbtPageModel
{
    /// <summary>
    /// Nbt根标签
    /// </summary>
    private readonly NbtNodeModel _root;
    /// <summary>
    /// 转到第几行
    /// </summary>
    private readonly Action<int> _turn;
    /// <summary>
    /// Nbt标签
    /// </summary>
    public NbtBase Nbt { get; }
    /// <summary>
    /// Nbt标签列表
    /// </summary>
    public HierarchicalTreeDataGridSource<NbtNodeModel> Source { get; init; }

    public NbtPageModel(NbtBase nbt, Action<int> turn)
    {
        Nbt = nbt;
        _turn = turn;
        _root = new NbtNodeModel(null, nbt, null);
        Source = new HierarchicalTreeDataGridSource<NbtNodeModel>([_root])
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

    /// <summary>
    /// 查找Nbt标签
    /// </summary>
    /// <param name="name">查找的键</param>
    public void Find(string name)
    {
        NbtNodeModel? data = _root.Find(name);
        if (data == null)
            return;

        Select(data);
    }

    /// <summary>
    /// 查找Nbt标签
    /// </summary>
    /// <param name="from">从那个Nbt标签</param>
    /// <param name="nbt">查找的标签</param>
    /// <returns></returns>
    public static NbtNodeModel? Find(NbtNodeModel from, NbtBase nbt)
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

    /// <summary>
    /// 选中区块
    /// </summary>
    /// <param name="nbt">区块</param>
    /// <returns></returns>
    public NbtNodeModel? Select(ChunkNbt nbt)
    {
        NbtNodeModel? data = _root.Find(nbt);
        if (data == null)
            return null;

        Select(data);

        return data;
    }

    /// <summary>
    /// 选中Nbt标签
    /// </summary>
    /// <param name="data">Nbt标签</param>
    public void Select(NbtNodeModel data)
    {
        var list = new List<int>();
        NbtNodeModel? top = data.Parent;
        NbtNodeModel? down = data;
        while (top != null)
        {
            list.Add(top.Children.IndexOf(down));
            top.Expand();
            down = top;
            top = top.Parent;
        }
        list.Add(0);
        list.Reverse();

        var list1 = list.ToArray();
        Dispatcher.UIThread.Post(() =>
        {
            var list = (Source.Selection as TreeDataGridRowSelectionModel<NbtNodeModel>)!;
            list.SelectedIndex = new();
            list.SelectedIndex = new(list1);

            var temp = 0;

            foreach (var item in list1)
            {
                temp += item;
            }

            _turn.Invoke(temp);
        });
    }
}