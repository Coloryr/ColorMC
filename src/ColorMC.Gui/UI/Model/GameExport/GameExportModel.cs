using Avalonia.Controls;
using AvaloniaEdit.Utils;
using ColorMC.Core.Game;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameExport;

public partial class GameExportModel : GameModel
{
    /// <summary>
    /// 导出的文件列表
    /// </summary>
    public FilesPageViewModel Files;

    [ObservableProperty]
    private HierarchicalTreeDataGridSource<FileTreeNodeModel> _source;

    public List<string> ExportTypes { get; init; } = LanguageBinding.GetExportName();

    /// <summary>
    /// 在线下载的Mod列表
    /// </summary>
    public ObservableCollection<ModExportModel> Mods { get; init; } = new();
    /// <summary>
    /// 在线下载的文件列表
    /// </summary>
    public ObservableCollection<ModExport1Model> OtherFiles { get; init; } = new();

    /// <summary>
    /// 待选择的文件列表
    /// </summary>
    public ObservableCollection<string> FileList { get; init; } = new();

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

    public readonly List<ModExportModel> Items = new();

    public GameExportModel(IUserControl con, GameSettingObj obj) : base(con, obj)
    {

    }

    async partial void OnTypeChanged(PackType value)
    {
        Progress(App.GetLanguage("GameExportWindow.Info6"));

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

        ProgressClose();
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
    }

    [RelayCommand]
    public async Task LoadMod()
    {
        Items.Clear();

        var list = await Obj.GetMods();
        if (list == null)
        {
            return;
        }
        foreach (var item in list)
        {
            ModExportModel obj1;
            if (item.ReadFail)
            {
                continue;
            }

            var info = new FileInfo(item.Local);
            using var stream = File.OpenRead(item.Local);

            var sha512 = await FuntionUtils.GenSha512Async(stream);

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
        }

        Load1();
    }

    [RelayCommand]
    public async Task Export()
    {
        if (Type == PackType.CurseForge || Type == PackType.Modrinth)
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Version))
            {
                Show(App.GetLanguage("GameExportWindow.Error2"));
                return;
            }
        }

        Progress(App.GetLanguage("GameExportWindow.Info1"));
        var file = await PathBinding.Export(Window, this);
        ProgressClose();
        if (file == null)
        {
            return;
        }

        if (file == false)
        {
            Show(App.GetLanguage("GameExportWindow.Error1"));
        }
        else
        {
            Notify(App.GetLanguage("GameExportWindow.Info2"));
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
        using var stream = File.OpenRead(info.FullName);

        var sha1 = await FuntionUtils.GenSha1Async(stream);
        stream.Seek(0, SeekOrigin.Begin);
        var obj1 = new ModExport1Model()
        {
            Path = FileName,
            Type = Type,
            Sha1 = sha1,
            Sha512 = await FuntionUtils.GenSha512Async(stream),
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
                       where item.Name.ToLower().Contains(fil)
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

        var list = PathCUtils.GetAllFile(path);
        foreach (var item in list)
        {
            var path1 = item.FullName[(path.Length + 1)..].Replace("\\", "/");
            if (path1.StartsWith("mods"))
            {
                continue;
            }

            using var stream = File.OpenRead(item.FullName);

            var sha1 = await FuntionUtils.GenSha1Async(stream);
            stream.Seek(0, SeekOrigin.Begin);
            var item1 = Obj.Mods.Values.FirstOrDefault(a => a.SHA1 == sha1);
            if (item1 != null)
            {
                var obj1 = new ModExport1Model()
                {
                    Path = path1,
                    Type = Type,
                    Sha1 = item1.SHA1!,
                    Sha512 = await FuntionUtils.GenSha512Async(stream),
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
            Files = new FilesPageViewModel(Obj.GetGamePath(), false);
        }
        else
        {
            Files = new FilesPageViewModel(Obj.GetBasePath(), true);
        }

        Source = Files.Source;
    }

    public void CellPressd()
    {

    }
}
