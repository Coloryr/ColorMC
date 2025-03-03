using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 文件子项目
/// </summary>
public partial class FileTreeNodeModel : ObservableObject
{
    /// <summary>
    /// 文件夹下的文件
    /// </summary>
    public ObservableCollection<FileTreeNodeModel> Children { get; init; } = [];
    /// <summary>
    /// 是否为文件夹
    /// </summary>
    public bool IsDirectory { get; init; }

    /// <summary>
    /// 路径
    /// </summary>
    [ObservableProperty]
    private string _path;
    /// <summary>
    /// 名字
    /// </summary>
    [ObservableProperty]
    private string _name;
    /// <summary>
    /// 占用大小
    /// </summary>
    [ObservableProperty]
    private long? _size;
    /// <summary>
    /// 编辑
    /// </summary>
    [ObservableProperty]
    private string? _modified;
    /// <summary>
    /// 是否有子项目
    /// </summary>
    [ObservableProperty]
    private bool _hasChildren = true;
    /// <summary>
    /// 是否展开
    /// </summary>
    [ObservableProperty]
    private bool _isExpanded;
    /// <summary>
    /// 是否选中
    /// </summary>
    [ObservableProperty]
    private bool _isChecked;

    /// <summary>
    /// 父文件夹
    /// </summary>
    private readonly FileTreeNodeModel _par;

    public FileTreeNodeModel(FileTreeNodeModel? par, string path, bool isDirectory, bool check = true, bool isRoot = false)
    {
        _par = par ?? this;
        _path = System.IO.Path.GetFullPath(path + (isDirectory ? "/" : ""));
        _name = isRoot ? path : System.IO.Path.GetFileName(path);
        _isExpanded = isRoot;
        _isChecked = check;
        IsDirectory = isDirectory;
        HasChildren = isDirectory;

        if (!isDirectory)
        {
            var info = new FileInfo(path);
            Size = info.Length;
            _modified = info.LastWriteTimeUtc.ToString("yyyy/MM/dd HH:mm:ss");
        }
        else
        {
            LoadChildren(check);
        }
    }

    partial void OnIsCheckedChanged(bool value)
    {
        //选中时让子项目也选中
        if (IsChecked == true)
        {
            foreach (var item in Children)
            {
                item.IsChecked = true;
            }
            _par.IsAllCheck();
        }
        else if (IsChecked == false)
        {
            foreach (var item in Children)
            {
                item.IsChecked = false;
            }
        }
    }

    /// <summary>
    /// 加载子项目
    /// </summary>
    /// <param name="check"></param>
    private void LoadChildren(bool check)
    {
        Children.Clear();
        if (!IsDirectory)
        {
            return;
        }

        var options = new EnumerationOptions { IgnoreInaccessible = true };

        foreach (var d in Directory.EnumerateDirectories(Path, "*", options))
        {
            Children.Add(new FileTreeNodeModel(this, d, true, check));
        }

        foreach (var f in Directory.EnumerateFiles(Path, "*", options))
        {
            Children.Add(new FileTreeNodeModel(this, f, false, check));
        }

        if (Children.Count == 0)
        {
            HasChildren = false;
        }
    }

    /// <summary>
    /// 排序比较器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static Comparison<FileTreeNodeModel?> SortAscending<T>(Func<FileTreeNodeModel, T> selector)
    {
        return (x, y) =>
        {
            if (x is null && y is null)
                return 0;
            else if (x is null)
                return -1;
            else if (y is null)
                return 1;
            if (x.IsDirectory == y.IsDirectory)
                return Comparer<T>.Default.Compare(selector(x), selector(y));
            else if (x.IsDirectory)
                return -1;
            else
                return 1;
        };
    }

    /// <summary>
    /// 排序比较器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="selector"></param>
    /// <returns></returns>
    public static Comparison<FileTreeNodeModel?> SortDescending<T>(Func<FileTreeNodeModel, T> selector)
    {
        return (x, y) =>
        {
            if (x is null && y is null)
                return 0;
            else if (x is null)
                return 1;
            else if (y is null)
                return -1;
            if (x.IsDirectory == y.IsDirectory)
                return Comparer<T>.Default.Compare(selector(y), selector(x));
            else if (x.IsDirectory)
                return -1;
            else
                return 1;
        };
    }

    /// <summary>
    /// 获取没有选中的文件
    /// </summary>
    /// <returns></returns>
    public List<string> GetUnSelectItems()
    {
        var list = new List<string>();
        if (Children != null)
        {
            foreach (var item in Children)
            {
                list.AddRange(item.GetUnSelectItems());
            }
        }

        if (!IsChecked && !IsDirectory)
        {
            list.Add(System.IO.Path.GetFullPath(Path));
        }

        return list;
    }

    /// <summary>
    /// 获取选中的文件
    /// </summary>
    /// <param name="getdir">同时获取目录</param>
    /// <returns></returns>
    public List<string> GetSelectItems(bool getdir)
    {
        var list = new List<string>();
        if (Children != null)
        {
            foreach (var item in Children)
            {
                list.AddRange(item.GetSelectItems(getdir));
            }
        }

        if (IsDirectory && !getdir)
        {
            return list;
        }

        if (IsChecked)
        {
            list.Add(System.IO.Path.GetFullPath(Path));
        }

        return list;
    }

    /// <summary>
    /// 选中文件
    /// </summary>
    /// <param name="item">文件路径</param>
    /// <returns></returns>
    public bool Select(string item)
    {
        if (Path == item)
        {
            IsChecked = true;
            return true;
        }
        if (Children != null)
        {
            foreach (var item1 in Children)
            {
                if (item1.Select(item))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 选中全部文件
    /// </summary>
    public void Select()
    {
        IsChecked = true;
        if (Children != null)
        {
            foreach (var item1 in Children)
            {
                item1.Select();
            }
        }
    }

    /// <summary>
    /// 取消选中文件
    /// </summary>
    /// <param name="item">文件路径</param>
    /// <returns></returns>
    public bool UnSelect(string item)
    {
        if (Path == item)
        {
            IsChecked = false;
            return true;
        }
        if (Children != null)
        {
            foreach (var item1 in Children)
            {
                if (item1.UnSelect(item))
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// 取消选中全部文件
    /// </summary>
    public void UnSelect()
    {
        IsChecked = false;
        if (Children != null)
        {
            foreach (var item1 in Children)
            {
                item1.UnSelect();
            }
        }
    }

    /// <summary>
    /// 是否全选
    /// </summary>
    public void IsAllCheck()
    {
        if (Children == null)
        {
            return;
        }
        foreach (var item in Children)
        {
            if (!item.IsChecked)
            {
                return;
            }
        }

        IsChecked = true;
    }
}