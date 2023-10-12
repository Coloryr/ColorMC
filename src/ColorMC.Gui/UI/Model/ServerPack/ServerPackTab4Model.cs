using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ColorMC.Gui.UI.Model.ServerPack;

public partial class ServerPackModel : MenuModel
{
    public ObservableCollection<ServerPackConfigObj> FileList { get; init; } = new();
    public ObservableCollection<string> NameList { get; init; } = new();
    public List<string> FuntionList { get; init; } = LanguageBinding.GetFontName();

    [ObservableProperty]
    private ServerPackConfigObj _fileItem;

    [ObservableProperty]
    private int _funtion;

    [ObservableProperty]
    private string? _group;

    [RelayCommand]
    public void AddFile()
    {
        if (string.IsNullOrEmpty(Group))
            return;
        string local = Obj.Game.GetGamePath() + "/" + Group;
        Obj.Config ??= new();
        if (local.EndsWith("/"))
        {
            if (Funtion == 0)
            {
                var item = new ConfigPackObj()
                {
                    Group = Group,
                    Zip = true,
                    Dir = true
                };

                item.Url = GetUrl(item)[..^1] + ".zip";
                Obj.Config.Add(item);
            }
            else
            {
                var item = new ConfigPackObj()
                {
                    Group = Group,
                    Zip = false,
                    Dir = true
                };

                item.Url = GetUrl(item);
                Obj.Config.Add(item);
            }
        }
        else
        {
            var item = new ConfigPackObj()
            {
                Group = Group,
                Zip = Funtion == 0,
                Dir = false
            };

            item.Url = GetUrl(item);
            Obj.Config.Add(item);
        }
        LoadFile();
    }

    public void LoadFile()
    {
        FuntionList.Clear();
        FileList.Clear();
        var mods = GameBinding.GetAllTopConfig(Obj.Game);

        Obj.Config?.RemoveAll(a => mods.Find(b => a.Group == b) == null);

        mods.ForEach(item =>
        {
            var item1 = Obj.Config?.FirstOrDefault(a => a.Group == item);
            if (item1 != null)
            {
                var item2 = new ServerPackConfigObj()
                {
                    Group = item,
                    Type = GetType(item1),
                    Url = item1.Url
                };

                FileList.Add(item2);
            }
            else
            {
                FuntionList.Add(item);
            }
        });

        GameBinding.SaveServerPack(Obj);
    }

    private string GetUrl(ConfigPackObj item)
    {
        if (!string.IsNullOrWhiteSpace(Obj.Url))
        {
            return Obj.Url + "config/" + item.Group;
        }
        else
        {
            return "";
        }
    }

    private string GetType(ConfigPackObj obj)
    {
        return App.GetLanguage(
            obj.Zip ? "ServerPackWindow.Tab4.Item1"
            : "ServerPackWindow.Tab4.Item2");
    }

    public void DeleteFile(ServerPackConfigObj obj)
    {
        var item1 = Obj.Config?.FirstOrDefault(a => a.Group == obj.Group);
        if (item1 != null)
        {
            Obj.Config?.Remove(item1);
            LoadFile();
        }
    }
}
