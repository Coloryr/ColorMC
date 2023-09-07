using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ColorMC.Gui.UI.Model.ServerPack;

public partial class ServerPackModel : TopModel
{
    public ObservableCollection<ServerPackItemModel> ConfigList { get; init; } = new();

    [ObservableProperty]
    private ServerPackItemModel _configItem;

    [RelayCommand]
    public void SelectAllConfig()
    {
        foreach (var item in ConfigList)
        {
            item.Check = true;
            ConfigItemEdit(item);
        }
    }

    [RelayCommand]
    public void UnSelectAllConfig()
    {
        foreach (var item in ConfigList)
        {
            item.Check = false;
            ConfigItemEdit(item);
        }
    }

    public void ConfigItemEdit()
    {
        ConfigItemEdit(ConfigItem);
    }

    public async void LoadConfigList()
    {
        ConfigList.Clear();
        var mods = await GameBinding.GetResourcepacks(Obj.Game);

        Obj.Resourcepack?.RemoveAll(a => mods.Find(b => a.Sha1 == b.Sha1) == null);

        mods.ForEach(item =>
        {
            if (item.Broken)
                return;

            string file = Path.GetFileName(item.Local);

            var item1 = Obj.Resourcepack?.FirstOrDefault(a => a.Sha1 == item.Sha1
                        && a.File == file);

            var item2 = new ServerPackItemModel()
            {
                FileName = file,
                Check = item1 != null,
                Sha1 = item.Sha1,
                Resourcepack = item
            };
            if (item1 != null)
            {
                if (!string.IsNullOrWhiteSpace(item1.Url))
                {
                    item2.Url = item1.Url;
                }
                else
                {
                    item2.Url = BaseBinding.MakeUrl(item1, FileType.Resourcepack, Obj.Url);
                }
            }

            ModList.Add(item2);
        });

        GameBinding.SaveServerPack(Obj);
    }

    private void ConfigItemEdit(ServerPackItemModel obj)
    {
        var item = Obj.Resourcepack?.FirstOrDefault(a => a.Sha1 == obj.Sha1
                        && a.File == obj.FileName);
        if (obj.Check)
        {
            if (item != null)
            {
                item.Sha1 = obj.Sha1;
                item.File = obj.FileName;
            }
            else
            {
                Obj.Resourcepack ??= new();
                item = new()
                {
                    Sha1 = obj.Sha1,
                    File = obj.FileName
                };
                Obj.Resourcepack.Add(item);
            }

            obj.Url = item.Url = BaseBinding.MakeUrl(item, FileType.Resourcepack, Obj.Url);
        }
        else
        {
            if (item != null)
            {
                obj.Url = "";
                Obj.Resourcepack?.Remove(item);
            }
        }

        GameBinding.SaveServerPack(Obj);
    }
}
