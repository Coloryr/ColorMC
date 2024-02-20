using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;

namespace ColorMC.Gui.UI.Model.ServerPack;

public partial class ServerPackModel
{
    public ObservableCollection<ServerPackConfigModel> FileList { get; init; } = [];
    public ObservableCollection<string> NameList { get; init; } = [];
    public string[] FuntionList { get; init; } = LanguageBinding.GetFuntionList();

    [ObservableProperty]
    private ServerPackConfigModel _fileItem;

    [ObservableProperty]
    private int _funtion;

    [ObservableProperty]
    private string? _group;

    [RelayCommand]
    public void AddFile()
    {
        if (string.IsNullOrEmpty(Group))
        {
            return;
        }
        string local = Obj.Game.GetGamePath() + "/" + Group;
        local = local.Replace('\\', '/');
        Obj.Config ??= [];
        if (local.EndsWith('/'))
        {
            if (Funtion == 0)
            {
                var item = new ConfigPackObj()
                {
                    Group = Group,
                    IsZip = true,
                    IsDir = true
                };

                Obj.Config.Add(item);
            }
            else
            {
                var item = new ConfigPackObj()
                {
                    Group = Group,
                    IsZip = false,
                    IsDir = true
                };

                Obj.Config.Add(item);
            }
        }
        else
        {
            var item = new ConfigPackObj()
            {
                Group = Group,
                IsZip = false,
                IsDir = false
            };

            Obj.Config.Add(item);
        }
        LoadFile();
    }

    public void SetTab4Click()
    {
        Model.SetChoiseCall(_name, Gen);
        Model.SetChoiseContent(_name, App.Lang("ServerPackWindow.Tab1.Text10"));
    }

    public void LoadFile()
    {
        FileList.Clear();
        NameList.Clear();
        var mods = GameBinding.GetAllTopConfig(Obj.Game);

        Obj.Config?.RemoveAll(a => mods.Find(b => a.Group == b) == null);

        mods.ForEach(item =>
        {
            var item1 = Obj.Config?.FirstOrDefault(a => a.Group == item);
            if (item1 != null)
            {
                FileList.Add(new(item1));
            }
            else
            {
                NameList.Add(item);
            }
        });

        GameBinding.SaveServerPack(Obj);
    }

    public void DeleteFile(ServerPackConfigModel obj)
    {
        Obj.Config?.Remove(obj.Obj);
        LoadFile();
    }
}
