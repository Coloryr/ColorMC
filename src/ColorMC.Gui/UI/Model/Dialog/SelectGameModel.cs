using System;
using System.Collections.ObjectModel;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.LaunchPath;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogHostAvalonia;

namespace ColorMC.Gui.UI.Model.Dialog;

public class SelectGameNode
{
    public ObservableCollection<SelectGameNode>? SubNodes { get; }
    public string Title { get; }
    public Guid UUID { get; }

    public SelectGameNode(string title, Guid index)
    {
        Title = title;
        UUID = index;
    }

    public SelectGameNode(string title, Guid index, ObservableCollection<SelectGameNode> subNodes)
    {
        Title = title;
        UUID = index;
        SubNodes = subNodes;
    }
}

/// <summary>
/// 选择游戏实例
/// </summary>
/// <param name="name">窗口Id</param>
public partial class SelectGameModel : ObservableObject
{
    /// <summary>
    /// 显示文本
    /// </summary>
    [ObservableProperty]
    private string _text;
    /// <summary>
    /// 选择项
    /// </summary>
    [ObservableProperty]
    private SelectGameNode? _select;

    /// <summary>
    /// 项目列表
    /// </summary>
    public ObservableCollection<SelectGameNode> Items { get; init; } = [];

    private readonly string _name;

    public SelectGameModel(string name, bool skipadd)
    {
        _name = name;
        foreach (var item in InstancesPath.Groups)
        {
            if (item.Value.Count == 0)
            {
                continue;
            }
            var list = new ObservableCollection<SelectGameNode>();
            foreach (var item1 in item.Value)
            {
                if (skipadd && GameManager.IsAdd(item1))
                {
                    continue;
                }
                list.Add(new SelectGameNode(item1.Name, item1.UUID));
            }
            if (item.Key == Names.NameDefaultGroup)
            {
                Items.Add(new SelectGameNode(LangUtils.Get("CollectWindow.Text26"), Guid.Empty, list));
            }
            else
            {
                Items.Add(new SelectGameNode(item.Key, Guid.Empty, list));
            }
        }
    }

    partial void OnSelectChanged(SelectGameNode? value)
    {
        if (value == null)
        {
            return;
        }
        else if (value.UUID == Guid.Empty)
        {
            Dispatcher.UIThread.Post(() =>
            {
                Select = null;
            });
        }
    }

    /// <summary>
    /// 取消
    /// </summary>
    [RelayCommand]
    public void Cancel()
    {
        DialogHost.Close(_name, false, this);
    }

    /// <summary>
    /// 同意
    /// </summary>
    [RelayCommand]
    public void Confirm()
    {
        DialogHost.Close(_name, true, this);
    }
}
