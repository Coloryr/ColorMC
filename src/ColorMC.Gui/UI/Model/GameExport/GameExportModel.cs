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
    //private FilesPageViewModel _model;

    //[ObservableProperty]
    //private bool _isGameRun;

    //[ObservableProperty]
    //private HierarchicalTreeDataGridSource<FileTreeNodeModel> _source;

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
    private int _filter;

    private readonly List<ModExportModel> _items = new();

    public GameExportModel(IUserControl con, GameSettingObj obj) : base(con, obj)
    {
        
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
        _items.Clear();

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

            obj1 = new ModExportModel()
            {
                Obj = item
            };

            var item1 = Obj.Mods.Values.FirstOrDefault(a => a.SHA1 == item.Sha1);
            if (item1 != null)
            {
                obj1.Obj1 = item1;
                obj1.FID = item1.FileId;
                Obj.PID = item1.ModId;
            }

            obj1.Export = !item.Disable;

            _items.Add(obj1);
        }
    }

    [RelayCommand]
    public async Task Export()
    {
        //Progress(App.GetLanguage("GameExportWindow.Info1"));
        //var file = await BaseBinding.SaveFile(Window, FileType.Game, new object[]
        //    { Obj, _model.GetUnSelectItems(), PackType.ColorMC });
        //ProgressClose();
        //if (file == null)
        //    return;

        //if (file == false)
        //{
        //    Show(App.GetLanguage("GameExportWindow.Error1"));
        //}
        //else
        //{
        //    Notify(App.GetLanguage("GameExportWindow.Info2"));
        //}
    }

    private void Load1()
    {
        if (string.IsNullOrWhiteSpace(Text))
        {
            Mods.Clear();
            Mods.AddRange(_items);
        }
        else
        {
            string fil = Text.ToLower();
            switch (Filter)
            {
                case 0:
                    var list = from item in _items
                               where item.Name.ToLower().Contains(fil)
                               select item;
                    Mods.Clear();
                    Mods.AddRange(list);
                    break;
                case 1:
                    list = from item in _items
                           where item.Local.ToLower().Contains(fil)
                           select item;
                    Mods.Clear();
                    Mods.AddRange(list);
                    break;
            }
        }
    }

    //public void Load()
    //{
    //    _model = new FilesPageViewModel(Obj.GetBasePath());
    //    Source = _model.Source;
    //    IsGameRun = BaseBinding.IsGameRun(Obj);
    //}
}
