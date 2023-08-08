using Avalonia.Controls;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ColorMC.Gui.UI.Model.ServerPack;

public partial class ServerPackTab4Model : ServerPackBaseModel
{
    public ObservableCollection<ServerPackConfigDisplayObj> ConfigList { get; init; } = new();
    public ObservableCollection<string> NameList { get; init; } = new();
    public List<string> FuntionList { get; init; } = LanguageBinding.GetFontName();

    [ObservableProperty]
    private ServerPackConfigDisplayObj _item;

    [ObservableProperty]
    private int _funtion;

    [ObservableProperty]
    private string? _group;

    public ServerPackTab4Model(IUserControl con, ServerPackObj obj) : base(con, obj)
    {

    }

    [RelayCommand]
    public void Add()
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
        Load();
    }

    public void Flyout(Control con)
    {
        _ = new ServerPackFlyout1(con, this, Item);
    }

    public void Load()
    {
        FuntionList.Clear();
        ConfigList.Clear();
        var mods = GameBinding.GetAllTopConfig(Obj.Game);

        Obj.Config?.RemoveAll(a => mods.Find(b => a.Group == b) == null);

        mods.ForEach(item =>
        {
            var item1 = Obj.Config?.FirstOrDefault(a => a.Group == item);
            if (item1 != null)
            {
                var item2 = new ServerPackConfigDisplayObj()
                {
                    Group = item,
                    Type = GetType(item1),
                    Url = item1.Url
                };

                ConfigList.Add(item2);
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
        if (obj.Zip)
            return App.GetLanguage("ServerPackWindow.Tab4.Item1");
        else
            return App.GetLanguage("ServerPackWindow.Tab4.Item2");
    }

    public void Delete(ServerPackConfigDisplayObj obj)
    {
        var item1 = Obj.Config?.FirstOrDefault(a => a.Group == obj.Group);
        if (item1 != null)
        {
            Obj.Config?.Remove(item1);
            Load();
        }
    }
}
