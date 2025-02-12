using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using AvaloniaEdit.Utils;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.GameExport;

/// <summary>
/// 实例导出
/// </summary>
public partial class GameExportModel : MenuModel
{
    /// <summary>
    /// 导出的文件列表
    /// </summary>
    public FilesPageModel Files { get; private set; }

    /// <summary>
    /// 文件列表
    /// </summary>
    [ObservableProperty]
    private HierarchicalTreeDataGridSource<FileTreeNodeModel> _source;

    /// <summary>
    /// 导出类型列表
    /// </summary>
    public string[] ExportTypes { get; init; } = LanguageBinding.GetExportName();

    /// <summary>
    /// 在线下载的Mod列表
    /// </summary>
    public ObservableCollection<ModExportModel> Mods { get; init; } = [];
    /// <summary>
    /// 在线下载的文件列表
    /// </summary>
    public ObservableCollection<ModExport1Model> OtherFiles { get; init; } = [];

    /// <summary>
    /// 待选择的文件列表
    /// </summary>
    public ObservableCollection<string> FileList { get; init; } = [];

    /// <summary>
    /// 导出类型
    /// </summary>
    [ObservableProperty]
    private PackType _type;

    /// <summary>
    /// 选中的模组
    /// </summary>
    [ObservableProperty]
    private ModExportModel? _selectMod;
    /// <summary>
    /// 选中的文件
    /// </summary>
    [ObservableProperty]
    private ModExport1Model? _selectFile;

    /// <summary>
    /// 文本
    /// </summary>
    [ObservableProperty]
    private string _text;
    /// <summary>
    /// 名字
    /// </summary>
    [ObservableProperty]
    private string _name;
    /// <summary>
    /// 版本
    /// </summary>
    [ObservableProperty]
    private string _version;
    /// <summary>
    /// 作者
    /// </summary>
    [ObservableProperty]
    private string _author;
    /// <summary>
    /// 描述
    /// </summary>
    [ObservableProperty]
    private string _summary;
    /// <summary>
    /// 自定义导出文件
    /// </summary>
    [ObservableProperty]
    private string _fileName;

    /// <summary>
    /// 是否有额外文件
    /// </summary>
    [ObservableProperty]
    private bool _cfEx;
    /// <summary>
    /// 是否有额外文件
    /// </summary>
    [ObservableProperty]
    private bool _moEx;

    /// <summary>
    /// 导出选择
    /// </summary>
    [ObservableProperty]
    private bool _enableInputText;

    /// <summary>
    /// 导出模组列表
    /// </summary>
    public readonly List<ModExportModel> Items = [];

    /// <summary>
    /// 游戏实例
    /// </summary>
    public GameSettingObj Obj { get; init; }

    private readonly string _useName;

    public GameExportModel(BaseModel model, GameSettingObj obj) : base(model)
    {
        Obj = obj;

        _useName = ToString() ?? "GameExportModel";

        SetMenu(
        [
            new()
            {
                Icon = "/Resource/Icon/GameExport/item1.svg",
                Text = App.Lang("GameExportWindow.Tabs.Text1")
            },
            new()
            {
                Icon = "/Resource/Icon/GameExport/item2.svg",
                Text = App.Lang("GameExportWindow.Tabs.Text2")
            },
            new()
            {
                Icon = "/Resource/Icon/GameExport/item3.svg",
                Text = App.Lang("GameExportWindow.Tabs.Text3")
            },
            new()
            {
                Icon = "/Resource/Icon/GameExport/item4.svg",
                Text = App.Lang("GameExportWindow.Tabs.Text4")
            },
        ]);
    }

    /// <summary>
    /// 导出格式切换
    /// </summary>
    /// <param name="value"></param>
    async partial void OnTypeChanged(PackType value)
    {
        Model.Progress(App.Lang("GameExportWindow.Info6"));

        CfEx = value == PackType.CurseForge;
        MoEx = value == PackType.Modrinth;

        EnableInputText = value switch
        {
            PackType.CurseForge => true,
            PackType.Modrinth => true,
            _ => false
        };

        LoadFile();
        if (MoEx)
        {
            await LoadFiles();
        }

        Model.ProgressClose();
    }

    partial void OnTextChanged(string value)
    {
        LoadMods();
    }

    /// <summary>
    /// 打开模组目录
    /// </summary>
    [RelayCommand]
    public void OpenMod()
    {
        if (SelectMod == null)
        {
            return;
        }

        PathBinding.OpenFileWithExplorer(SelectMod.Obj.Local);
    }

    /// <summary>
    /// 加载模组列表
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task LoadMod()
    {
        Items.Clear();

        var list = await Obj.GetModsAsync(false);
        if (list == null)
        {
            return;
        }

        await Parallel.ForEachAsync(list, async (item, cancel) =>
        {
            ModExportModel obj1;
            if (item.ReadFail)
            {
                return;
            }
            var info = new FileInfo(item.Local);
            using var stream = PathHelper.OpenRead(item.Local)!;
            var sha512 = await HashHelper.GenSha512Async(stream);
            var item1 = Obj.Mods.Values.FirstOrDefault(a => a.Sha1 == item.Sha1);
            if (item1 != null)
            {
                obj1 = new ModExportModel(Model, item1.ModId, item1.FileId)
                {
                    Type = Type,
                    Obj = item,
                    Obj1 = item1,
                    Sha1 = item1.Sha1!,
                    Sha512 = sha512,
                    Url = item1.Url,
                    FileSize = info.Length
                };
                obj1.Reload();
                Items.Add(obj1);
            }
            else
            {
                obj1 = new ModExportModel(Model, null, null)
                {
                    Type = Type,
                    Obj = item
                };
                Items.Add(obj1);
            }
        });

        Model.Notify(App.Lang("GameExportWindow.Info8"));

        LoadMods();
    }

    /// <summary>
    /// 开始导出
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task Export()
    {
        if (Type == PackType.CurseForge || Type == PackType.Modrinth)
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Version))
            {
                Model.Show(App.Lang("GameExportWindow.Error2"));
                return;
            }
        }

        Model.Progress(App.Lang("GameExportWindow.Info1"));
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.Export(top, this);
        Model.ProgressClose();
        if (file == null)
        {
            return;
        }

        if (file == false)
        {
            Model.Show(App.Lang("GameExportWindow.Error1"));
        }
        else
        {
            Model.Notify(App.Lang("GameExportWindow.Info2"));
        }
    }

    /// <summary>
    /// 添加自定义文件
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task AddFile()
    {
        if (string.IsNullOrWhiteSpace(FileName))
        {
            return;
        }

        var info = new FileInfo(Path.GetFullPath(Obj.GetGamePath() + "/" + FileName));
        using var stream = PathHelper.OpenRead(info.FullName)!;

        var sha1 = await HashHelper.GenSha1Async(stream)!;
        stream.Seek(0, SeekOrigin.Begin);
        var obj1 = new ModExport1Model()
        {
            Path = FileName,
            Type = Type,
            Sha1 = sha1,
            Sha512 = await HashHelper.GenSha512Async(stream),
            Url = "",
            FileSize = info.Length
        };
        OtherFiles.Add(obj1);
        FileList.Remove(FileName);
    }

    /// <summary>
    /// 加载模组列表
    /// </summary>
    public void LoadMods()
    {
        if (string.IsNullOrWhiteSpace(Text))
        {
            Mods.Clear();
            Mods.AddRange(Items);
        }
        else
        {
            string fil = Text.ToLower();
            var list = from item in Items
                       where item.Name.Contains(fil, StringComparison.CurrentCultureIgnoreCase)
                       select item;
            Mods.Clear();
            Mods.AddRange(list);
        }
    }

    /// <summary>
    /// 加载文件列表
    /// </summary>
    /// <returns></returns>
    public async Task LoadFiles()
    {
        FileList.Clear();
        OtherFiles.Clear();

        var path = Obj.GetGamePath();

        var list = PathHelper.GetAllFile(path);
        var list1 = new List<string>();
        foreach (var item in list)
        {
            var path1 = item.FullName[(path.Length + 1)..].Replace("\\", "/");
            if (path1.StartsWith("mods"))
            {
                continue;
            }

            using var stream = PathHelper.OpenRead(item.FullName)!;

            var sha1 = await HashHelper.GenSha1Async(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var item1 = Obj.Mods.Values.FirstOrDefault(a => a.Sha1 == sha1);
            if (item1 != null)
            {
                var obj1 = new ModExport1Model()
                {
                    Path = path1,
                    Type = Type,
                    Sha1 = item1.Sha1!,
                    Sha512 = await HashHelper.GenSha512Async(stream),
                    Url = item1.Url,
                    FileSize = item.Length
                };
                OtherFiles.Add(obj1);
            }
            else
            {
                list1.Add(item.FullName);
                FileList.Add(path1);
            }
        }

        //除去可以自己下载的文件
        Files.SetSelectItems(list1);
    }

    /// <summary>
    /// 设置标题选择项目
    /// </summary>
    public void SetTab3Choise()
    {
        Model.SetChoiseCall(_useName, SelectAllFile, UnSelectAllFile);
        Model.SetChoiseContent(_useName, App.Lang("Button.SelectAll"), App.Lang("Button.UnSelectAll"));
    }

    /// <summary>
    /// 设置标题选择项目
    /// </summary>
    public void SetTab2Choise()
    {
        Model.SetChoiseCall(_useName, SelectAllMod, UnSelectAllMod);
        Model.SetChoiseContent(_useName, App.Lang("Button.SelectAll"), App.Lang("Button.UnSelectAll"));
    }

    /// <summary>
    /// 删除标题选择项目
    /// </summary>
    public void RemoveChoise()
    {
        Model.RemoveChoiseData(_useName);
    }

    /// <summary>
    /// 选择所有文件
    /// </summary>
    private void SelectAllFile()
    {
        Files.SetSelectItems();
    }

    /// <summary>
    /// 取消选择所有文件
    /// </summary>
    private void UnSelectAllFile()
    {
        Files.SetUnSelectItems();
    }

    /// <summary>
    /// 选择所有模组
    /// </summary>
    private void SelectAllMod()
    {
        foreach (var item in Mods)
        {
            if (item.Export == false && item.Obj1 != null)
            {
                item.Export = true;
            }
        }
    }

    /// <summary>
    /// 取消选择所有模组
    /// </summary>
    private void UnSelectAllMod()
    {
        foreach (var item in Mods)
        {
            if (item.Export == true)
            {
                item.Export = false;
            }
        }
    }

    /// <summary>
    /// 加载文件列表
    /// </summary>
    public void LoadFile()
    {
        Files = Type switch
        {
            PackType.Modrinth => new FilesPageModel(Obj.GetGamePath(), false),
            PackType.CurseForge => new FilesPageModel(Obj.GetGamePath(), true),
            _ => new FilesPageModel(Obj.GetBasePath(), true)
        };
        //除去可以自己下载的模组
        var list = new List<string>();
        foreach (var item in Items)
        {
            if (item.Obj1 == null)
            {
                continue;
            }

            var type = DownloadItemHelper.TestSourceType(item.Obj1.ModId, item.Obj1.FileId);
            if ((type == SourceType.CurseForge && Type == PackType.CurseForge) 
                || (type == SourceType.Modrinth && Type == PackType.Modrinth))
            {
                item.Export = true;
                list.Add(item.Obj.Local);
            }
        }
        Files.SetUnSelectItems(list);

        Source = Files.Source;
    }

    public override void Close()
    {
        Mods.Clear();
        OtherFiles.Clear();
        Files = null!;
        Source = null!;
    }
}
