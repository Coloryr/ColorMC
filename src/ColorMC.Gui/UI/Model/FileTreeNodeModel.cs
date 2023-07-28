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
    private DateTimeOffset? _modified;
    [ObservableProperty]
    private bool _hasChildren = true;
    [ObservableProperty]
    private bool _isExpanded;
    [ObservableProperty]
    private bool _isChecked;

    public ObservableCollection<FileTreeNodeModel> Children { get; init; } = new();

    public bool IsDirectory { get; init; }

    public FileTreeNodeModel(
            string path,
            bool isDirectory,
            bool check = true,
            bool isRoot = false)
    {
        _path = path;
        _name = isRoot ? path : System.IO.Path.GetFileName(Path);
        _isExpanded = isRoot;
        _isChecked = check;
        IsDirectory = isDirectory;
        HasChildren = isDirectory;

        if (!isDirectory)
        {
            var info = new FileInfo(path);
            Size = info.Length;
            Modified = info.LastWriteTimeUtc;
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
            Children.Add(new FileTreeNodeModel(d, true, check));
        }

        foreach (var f in Directory.EnumerateFiles(Path, "*", options))
        {
            Children.Add(new FileTreeNodeModel(f, false, check));
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

        if (IsChecked && !IsDirectory)
        {
            list.Add(System.IO.Path.GetFullPath(Path));
        }

        return list;
    }
}