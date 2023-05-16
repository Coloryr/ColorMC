using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class FileTreeNodeModel : ObservableObject
{
    [ObservableProperty]
    private string path;
    [ObservableProperty]
    private string name;
    [ObservableProperty]
    private long? size;
    [ObservableProperty]
    private DateTimeOffset? modified;
    [ObservableProperty]
    private bool hasChildren = true;
    [ObservableProperty]
    private bool isExpanded;
    [ObservableProperty]
    private bool isChecked;
    [ObservableProperty]
    private ObservableCollection<FileTreeNodeModel>? children;

    public bool IsDirectory { get; init; }

    public FileTreeNodeModel(
            string path,
            bool isDirectory,
            bool isRoot = false)
    {
        this.path = path;
        name = isRoot ? path : System.IO.Path.GetFileName(Path);
        isExpanded = isRoot;
        isChecked = true;
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
            LoadChildren();
        }
    }

    partial void OnIsCheckedChanged(bool value)
    {
        if (IsChecked == true)
        {
            if (Children != null)
            {
                foreach (var item in Children)
                {
                    item.IsChecked = true;
                }
            }
        }
        else if (IsChecked == false)
        {
            if (Children != null)
            {
                foreach (var item in Children)
                {
                    item.IsChecked = false;
                }
            }
        }
    }

    private void LoadChildren()
    {
        if (!IsDirectory)
        {
            Children = new();
            return;
        }

        var options = new EnumerationOptions { IgnoreInaccessible = true };
        var result = new ObservableCollection<FileTreeNodeModel>();

        foreach (var d in Directory.EnumerateDirectories(Path, "*", options))
        {
            result.Add(new FileTreeNodeModel(d, true));
        }

        foreach (var f in Directory.EnumerateFiles(Path, "*", options))
        {
            result.Add(new FileTreeNodeModel(f, false));
        }

        if (result.Count == 0)
            HasChildren = false;

        Children = result;
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
}