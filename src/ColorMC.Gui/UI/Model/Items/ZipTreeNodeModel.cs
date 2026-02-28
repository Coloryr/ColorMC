using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Writers.Zip;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// ZIP文件树节点模型
/// </summary>
public partial class ZipTreeNodeModel : ObservableObject
{
    /// <summary>
    /// 文件夹下的文件
    /// </summary>
    public ObservableCollection<ZipTreeNodeModel> Children { get; init; } = [];

    /// <summary>
    /// 是否为文件夹
    /// </summary>
    public bool IsDirectory { get; init; }

    /// <summary>
    /// 路径（ZIP内的相对路径）
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
    /// 占用大小
    /// </summary>
    [ObservableProperty]
    private long? _compressedSize;
    
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
    private readonly ZipTreeNodeModel _parent;

    /// <summary>
    /// 原始ZIP条目
    /// </summary>
    public readonly IArchiveEntry? Entry;

    /// <summary>
    /// 构造函数（用于创建根节点）
    /// </summary>
    public ZipTreeNodeModel()
    {
        _parent = this;
        _path = "/";
        _name = "/";
        _isChecked = true;
        _isExpanded = true;
        IsDirectory = true;
        HasChildren = false;
    }

    /// <summary>
    /// 构造函数（从ZIP条目创建节点）
    /// </summary>
    private ZipTreeNodeModel(ZipTreeNodeModel? parent, IArchiveEntry entry, bool isChecked = true)
    {
        _parent = parent ?? this;
        Entry = entry;

        if (entry.Key != null)
        {
            // 处理路径
            string fullPath = entry.Key.Replace('\\', '/').Trim('/');
            _path = string.IsNullOrEmpty(fullPath) ? "/" : "/" + fullPath;

            // 判断是否为目录（ZIP条目中目录通常以/结尾）
            IsDirectory = entry.IsDirectory || entry.Key.EndsWith('/');

            // 获取名称
            _name = IsDirectory
                ? System.IO.Path.GetFileName(fullPath.TrimEnd('/'))
                : System.IO.Path.GetFileName(fullPath);
        }
        if (string.IsNullOrEmpty(_name))
        {
            _name = "";
        }

        _isChecked = isChecked;
        _isExpanded = false;

        // 设置文件属性
        if (!IsDirectory)
        {
            Size = entry.Size;
            HasChildren = false;
        }
        else
        {
            HasChildren = true;
            // 目录不立即加载子项
        }
    }

    /// <summary>
    /// 构造函数（用于构建树时创建中间节点）
    /// </summary>
    private ZipTreeNodeModel(ZipTreeNodeModel? parent, string path, bool isDirectory, bool isChecked = true)
    {
        _parent = parent ?? this;
        _path = path.StartsWith('/') ? path : "/" + path;
        IsDirectory = isDirectory;
        _name = System.IO.Path.GetFileName(_path.TrimEnd('/'));

        if (string.IsNullOrEmpty(_name))
        {
            _name = "";
        }

        _isChecked = isChecked;
        _isExpanded = false;
        HasChildren = isDirectory;
    }

    /// <summary>
    /// 从ZIP存档创建树结构
    /// </summary>
    public ZipTreeNodeModel(IWritableArchive<ZipWriterOptions> archive, bool defaultChecked = true)
    {
        _parent = this;
        _name = "";
        _path = "/";
        _isExpanded = true;
        IsDirectory = true;
        _isChecked = true;
        HasChildren = archive.Entries.Any();

        var pathDictionary = new Dictionary<string, ZipTreeNodeModel>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in archive.Entries)
        {
            ProcessEntry(entry, this, pathDictionary, defaultChecked);
        }

        SortChildren();
    }

    partial void OnIsCheckedChanged(bool value)
    {
        if (IsChecked)
        {
            foreach (var item in Children)
            {
                item.IsChecked = true;
            }
            _parent.IsAllCheck();
        }
        else
        {
            foreach (var item in Children)
            {
                item.IsChecked = false;
            }
        }
    }

    /// <summary>
    /// 排序子节点（目录优先，然后按名称）
    /// </summary>
    private void SortChildren()
    {
        var sorted = Children.OrderBy(c => !c.IsDirectory)  // 目录在前
                             .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                             .ToList();

        Children.Clear();
        foreach (var item in sorted)
        {
            Children.Add(item);
        }
    }

    /// <summary>
    /// 处理单个ZIP条目
    /// </summary>
    private static void ProcessEntry(
        IArchiveEntry entry,
        ZipTreeNodeModel rootNode,
        Dictionary<string, ZipTreeNodeModel> pathDictionary,
        bool defaultChecked)
    {
        if (entry.Key != null)
        {
            string fullPath = entry.Key.Replace('\\', '/').Trim('/');
            string[] parts = fullPath.Split('/');

            string currentPath = "";
            ZipTreeNodeModel parent = rootNode;

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i];
                bool isLastPart = i == parts.Length - 1;

                currentPath = string.IsNullOrEmpty(currentPath) ? part : currentPath + "/" + part;

                if (!pathDictionary.TryGetValue(currentPath, out var node))
                {
                    if (isLastPart && !(entry.IsDirectory || entry.Key.EndsWith('/')))
                    {
                        // 文件节点
                        node = new ZipTreeNodeModel(parent, entry, defaultChecked);
                    }
                    else
                    {
                        // 目录节点
                        node = new ZipTreeNodeModel(parent, currentPath, true, defaultChecked);
                    }

                    pathDictionary[currentPath] = node;
                    parent.Children.Add(node);
                    parent.SortChildren();
                }

                parent = node;
            }
        }
    }

    /// <summary>
    /// 获取未选中的文件列表
    /// </summary>
    public List<IArchiveEntry> GetUnSelectItems()
    {
        var list = new List<IArchiveEntry>();

        foreach (var item in Children)
        {
            list.AddRange(item.GetUnSelectItems());
        }

        if (!IsChecked && !IsDirectory && Entry !=null)
        {
            list.Add(Entry);
        }

        return list;
    }

    /// <summary>
    /// 获取选中的文件列表
    /// </summary>
    /// <param name="includeDirectories">是否包含目录</param>
    public List<string> GetSelectItems(bool includeDirectories = false)
    {
        var list = new List<string>();

        foreach (var item in Children)
        {
            list.AddRange(item.GetSelectItems(includeDirectories));
        }

        if (IsDirectory && !includeDirectories)
        {
            return list;
        }

        if (IsChecked)
        {
            list.Add(Path);
        }

        return list;
    }

    /// <summary>
    /// 获取选中的文件路径列表（只包含文件，不包含目录）
    /// </summary>
    public List<string> GetSelectedFiles()
    {
        return GetSelectItems(false);
    }

    /// <summary>
    /// 获取选中的所有项目路径列表（包含文件和目录）
    /// </summary>
    public List<string> GetSelectedItems()
    {
        return GetSelectItems(true);
    }

    /// <summary>
    /// 获取选中的文件节点列表
    /// </summary>
    public List<ZipTreeNodeModel> GetSelectedFileNodes()
    {
        var list = new List<ZipTreeNodeModel>();

        foreach (var item in Children)
        {
            list.AddRange(item.GetSelectedFileNodes());
        }

        if (IsChecked && !IsDirectory)
        {
            list.Add(this);
        }

        return list;
    }

    /// <summary>
    /// 获取选中的目录节点列表
    /// </summary>
    public List<ZipTreeNodeModel> GetSelectedDirectoryNodes()
    {
        var list = new List<ZipTreeNodeModel>();

        foreach (var item in Children)
        {
            list.AddRange(item.GetSelectedDirectoryNodes());
        }

        if (IsChecked && IsDirectory)
        {
            list.Add(this);
        }

        return list;
    }

    /// <summary>
    /// 获取所有节点（递归）
    /// </summary>
    public List<ZipTreeNodeModel> GetAllNodes()
    {
        var list = new List<ZipTreeNodeModel> { this };

        foreach (var child in Children)
        {
            list.AddRange(child.GetAllNodes());
        }

        return list;
    }

    /// <summary>
    /// 获取所有文件节点（递归）
    /// </summary>
    public List<ZipTreeNodeModel> GetAllFileNodes()
    {
        var list = new List<ZipTreeNodeModel>();

        if (!IsDirectory)
        {
            list.Add(this);
        }

        foreach (var child in Children)
        {
            list.AddRange(child.GetAllFileNodes());
        }

        return list;
    }

    /// <summary>
    /// 获取所有目录节点（递归）
    /// </summary>
    public List<ZipTreeNodeModel> GetAllDirectoryNodes()
    {
        var list = new List<ZipTreeNodeModel>();

        if (IsDirectory && this != _parent) // 排除根节点
        {
            list.Add(this);
        }

        foreach (var child in Children)
        {
            list.AddRange(child.GetAllDirectoryNodes());
        }

        return list;
    }

    /// <summary>
    /// 选中指定路径的文件
    /// </summary>
    public bool Select(string itemPath)
    {
        // 标准化路径比较
        string normalizedPath = itemPath.Replace('\\', '/');
        if (!normalizedPath.StartsWith('/'))
        {
            normalizedPath = "/" + normalizedPath;
        }

        if (Path.Equals(normalizedPath, StringComparison.OrdinalIgnoreCase))
        {
            IsChecked = true;
            return true;
        }

        foreach (var child in Children)
        {
            if (child.Select(itemPath))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 选中所有文件
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
    /// 取消选中指定路径的文件
    /// </summary>
    public bool UnSelect(string itemPath)
    {
        // 标准化路径比较
        string normalizedPath = itemPath.Replace('\\', '/');
        if (!normalizedPath.StartsWith('/'))
        {
            normalizedPath = "/" + normalizedPath;
        }

        if (Path.Equals(normalizedPath, StringComparison.OrdinalIgnoreCase))
        {
            IsChecked = false;
            return true;
        }

        foreach (var child in Children)
        {
            if (child.UnSelect(itemPath))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 取消选中所有文件
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
    /// 根据条件选中文件
    /// </summary>
    public int SelectWhere(Func<ZipTreeNodeModel, bool> predicate)
    {
        int count = 0;

        if (predicate(this))
        {
            IsChecked = true;
            count++;
        }

        foreach (var child in Children)
        {
            count += child.SelectWhere(predicate);
        }

        return count;
    }

    /// <summary>
    /// 排序比较器（升序）
    /// </summary>
    public static Comparison<ZipTreeNodeModel?> SortAscending<T>(Func<ZipTreeNodeModel, T> selector)
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
    /// 排序比较器（降序）
    /// </summary>
    public static Comparison<ZipTreeNodeModel?> SortDescending<T>(Func<ZipTreeNodeModel, T> selector)
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
    /// 是否全选
    /// </summary>
    private void IsAllCheck()
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