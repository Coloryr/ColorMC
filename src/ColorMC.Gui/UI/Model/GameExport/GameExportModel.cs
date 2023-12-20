using Avalonia.Controls;
using AvaloniaEdit.Utils;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameExport;

public partial class GameExportModel(BaseModel model, GameSettingObj obj) : MenuModel(model)
{
    /// <summary>
    /// 导出的文件列表
    /// </summary>
    public FilesPageModel Files { get; private set; }

    [ObservableProperty]
    private HierarchicalTreeDataGridSource<FileTreeNodeModel> _source;

    public string[] ExportTypes { get; init; } = LanguageBinding.GetExportName();

    public override List<MenuObj> TabItems { get; init; } =
    [
        new() { Icon = "/Resource/Icon/GameExport/item1.svg",
            Text = App.Lang("GameExportWindow.Tabs.Text1") },
        new() { Icon = "/Resource/Icon/GameExport/item2.svg",
            Text = App.Lang("GameExportWindow.Tabs.Text2") },
        new() { Icon = "/Resource/Icon/GameExport/item3.svg",
            Text = App.Lang("GameExportWindow.Tabs.Text3") },
        new() { Icon = "/Resource/Icon/GameExport/item4.svg",
            Text = App.Lang("GameExportWindow.Tabs.Text4") },
    ];

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

    [ObservableProperty]
    private PackType _type;

    [ObservableProperty]
    private ModExportModel? _selectMod;
    [ObservableProperty]
    private ModExport1Model? _selectFile;

    [ObservableProperty]
    private string _text;
    [ObservableProperty]
    private string _name;
    [ObservableProperty]
    private string _version;
    [ObservableProperty]
    private string _author;
    [ObservableProperty]
    private string _summary;
    [ObservableProperty]
    private string _fileName;

    [ObservableProperty]
    private bool _cfEx;
    [ObservableProperty]
    private bool _moEx;

    [ObservableProperty]
    private bool _enableInput1;
    [ObservableProperty]
    private bool _enableInput2;
    [ObservableProperty]
    private bool _enableInput3;

    public readonly List<ModExportModel> Items = [];

    public GameSettingObj Obj { get; init; } = obj;

    async partial void OnTypeChanged(PackType value)
    {
        Model.Progress(App.Lang("GameExportWindow.Info6"));

        CfEx = value == PackType.CurseForge;
        MoEx = value == PackType.Modrinth;

        EnableInput1 = value switch
        {
            PackType.CurseForge => true,
            PackType.Modrinth => true,
            _ => false
        };

        LoadFile();
        await LoadMod();
        if (MoEx)
        {
            await Load2();
        }

        Model.ProgressClose();
    }

    partial void OnTextChanged(string value)
    {
        Load1();
    }

    [RelayCommand]
    public void OpenMod()
    {
        if (SelectMod == null)
        {
            return;
        }

        PathBinding.OpFile(SelectMod.Obj.Local);
    }

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
            var item1 = Obj.Mods.Values.FirstOrDefault(a => a.SHA1 == item.Sha1);
            if (item1 != null)
            {
                obj1 = new ModExportModel(item1.ModId, item1.FileId)
                {
                    Type = Type,
                    Obj = item,
                    Obj1 = item1,
                    Sha1 = item1.SHA1!,
                    Sha512 = sha512,
                    Url = item1.Url,
                    FileSize = info.Length
                };
                obj1.Reload();
                Items.Add(obj1);
            }
            else
            {
                obj1 = new ModExportModel(null, null)
                {
                    Type = Type,
                    Obj = item
                };
                Items.Add(obj1);
            }
        });

        Load1();
    }

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
        var file = await PathBinding.Export(this);
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

    [RelayCommand]
    public async Task AddFile()
    {
        if (string.IsNullOrWhiteSpace(FileName))
        {
            return;
        }

        var info = new FileInfo(Obj.GetGamePath() + "/" + FileName);
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

    public void Load1()
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

    public async Task Load2()
    {
        FileList.Clear();
        OtherFiles.Clear();

        var path = Obj.GetGamePath();

        var list = PathHelper.GetAllFile(path);
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
            var item1 = Obj.Mods.Values.FirstOrDefault(a => a.SHA1 == sha1);
            if (item1 != null)
            {
                var obj1 = new ModExport1Model()
                {
                    Path = path1,
                    Type = Type,
                    Sha1 = item1.SHA1!,
                    Sha512 = await HashHelper.GenSha512Async(stream),
                    Url = item1.Url,
                    FileSize = item.Length
                };
                OtherFiles.Add(obj1);
            }
            else
            {
                FileList.Add(path1);
            }
        }
    }

    public void LoadFile()
    {
        if (Type == PackType.CurseForge || Type == PackType.Modrinth)
        {
            Files = new FilesPageModel(Obj.GetGamePath(), false);
        }
        else
        {
            Files = new FilesPageModel(Obj.GetBasePath(), true);
        }

        Source = Files.Source;
    }

    protected override void Close()
    {
        Mods.Clear();
        OtherFiles.Clear();
        Files = null!;
        Source = null!;
    }
}
