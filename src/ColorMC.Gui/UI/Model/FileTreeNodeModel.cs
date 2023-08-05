using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace ColorMC.Gui.UI.Model;

public partial class FileTreeNodeModel : ObservableObject
{
    [ObservableProperty]
    private string _path;
    [ObservableProperty]
    private string _name;
    [ObservableProperty]
    private long? _size;
    [ObservableProperty]
    private string? _modified;
    [ObservableProperty]
    private bool _hasChildren = true;
    [ObservableProperty]
    private bool _isExpanded;
    [ObservableProperty]
    private bool _isChecked;

    private FileTreeNodeModel _par;

    public ObservableCollection<FileTreeNodeModel> Children { get; init; } = new();

    public bool IsDirectory { get; init; }

    public FileTreeNodeModel(
        FileTreeNodeModel? par,
        string path,
        bool isDirectory,
        bool check = true,
        bool isRoot = false)
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

    public List<string> GetSelectItems()
    {
        var list = new List<string>();
        if (Children != null)
        {
            foreach (var item in Children)
            {
                list.AddRange(item.GetSelectItems());
            }
        }

        if (IsChecked)
        {
            list.Add(System.IO.Path.GetFullPath(Path));
        }

        return list;
    }

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