using Avalonia.Controls;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using ColorMC.Core.LaunchPath;
using System.Collections.Generic;
using ColorMC.Gui.Utils;
using System.Collections.ObjectModel;
using ColorMC.Core.Game;
using System.Collections;
using System.Linq;
using AvaloniaEdit.Utils;

namespace ColorMC.Gui.UI.Model.GameExport;

public partial class GameExportModel : GameEditModel
{
    public FilesPageViewModel Files;

    [ObservableProperty]
    private HierarchicalTreeDataGridSource<FileTreeNodeModel> _source;

    public List<string> FilterList => LanguageUtils.GetFilter1Name();
    public List<string> ExportTypes => LanguageUtils.GetExportName();

    public ObservableCollection<ModExportModel> Mods { get; } = new();

    [ObservableProperty]
    private PackType _type;

    [ObservableProperty]
    private ModExportModel? _selectMod;

    [ObservableProperty]
    private string _text;
    [ObservableProperty]
    private string _name;
    [ObservableProperty]
    private string _version;
    [ObservableProperty]
    private string _author;

    [ObservableProperty]
    private int _filter;

    [ObservableProperty]
    private bool _enableInput1;

    public readonly List<ModExportModel> Items = new();

    public GameExportModel(IUserControl con, GameSettingObj obj) : base(con, obj)
    {
        
    }

    async partial void OnTypeChanged(PackType value)
    {
        EnableInput1 = value switch
        {
            PackType.CurseForge => true,
            PackType.Modrinth => true,
            _ => false
        };

        LoadFile();
        await LoadMod();
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

            var item1 = Obj.Mods.Values.FirstOrDefault(a => a.SHA1 == item.Sha1);
            if (item1 != null)
            {
                obj1 = new ModExportModel(item1.ModId, item1.FileId)
                {
                    Type = Type,
                    Obj = item,
                    Obj1 = item1
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
        var file = await GameBinding.Export(Control, this);
        ProgressClose();
        if (file == null)
            return;

        if (file == false)
        {
            Show(App.GetLanguage("GameExportWindow.Error1"));
        }
        else
        {
            Notify(App.GetLanguage("GameExportWindow.Info2"));
        }
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
            switch (Filter)
            {
                case 0:
                    var list = from item in Items
                               where item.Name.ToLower().Contains(fil)
                               select item;
                    Mods.Clear();
                    Mods.AddRange(list);
                    break;
                case 1:
                    list = from item in Items
                           where item.Local.ToLower().Contains(fil)
                           select item;
                    Mods.Clear();
                    Mods.AddRange(list);
                    break;
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
}
