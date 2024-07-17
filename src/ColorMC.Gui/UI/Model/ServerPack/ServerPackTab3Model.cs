using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.ServerPack;

public partial class ServerPackModel
{
    public ObservableCollection<ServerPackItemModel> ResourceList { get; init; } = [];

    [ObservableProperty]
    private ServerPackItemModel _resourceItem;

    [RelayCommand]
    public void SelectAllResource()
    {
        foreach (var item in ResourceList)
        {
            item.Check = true;
            ResourceItemEdit(item);
        }
    }

    [RelayCommand]
    public void UnSelectAllResource()
    {
        foreach (var item in ResourceList)
        {
            item.Check = false;
            ResourceItemEdit(item);
        }
    }

    public void ResourceItemEdit()
    {
        ResourceItemEdit(ResourceItem);
    }

    public async void LoadResourceList()
    {
        ResourceList.Clear();
        var mods = await GameBinding.GetResourcepacks(Obj.Game, true);

        Obj.Resourcepack?.RemoveAll(a => mods.Find(b => a.Sha256 == b.Sha256) == null);

        mods.ForEach(item =>
        {
            if (item.Broken)
                return;

            string file = Path.GetFileName(item.Local);

            var item1 = Obj.Resourcepack?.FirstOrDefault(a => a.Sha256 == item.Sha256
                        && a.File == file);

            var item2 = new ServerPackItemModel()
            {
                FileName = file,
                Check = item1 != null,
                Sha256 = item.Sha256,
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
                    item2.Url = BaseBinding.MakeUrl(item1, FileType.Resourcepack, Obj.Game.ServerUrl);
                }
            }

            ResourceList.Add(item2);
        });

        GameBinding.SaveServerPack(Obj);
    }

    private void ResourceItemEdit(ServerPackItemModel obj)
    {
        var item = Obj.Resourcepack?.FirstOrDefault(a => a.Sha256 == obj.Sha256
                        && a.File == obj.FileName);
        if (obj.Check)
        {
            if (item != null)
            {
                item.Sha256 = obj.Sha256;
                item.File = obj.FileName;
            }
            else
            {
                Obj.Resourcepack ??= [];
                item = new()
                {
                    Sha256 = obj.Sha256,
                    File = obj.FileName
                };
                Obj.Resourcepack.Add(item);
            }

            obj.Url = item.Url = BaseBinding.MakeUrl(item, FileType.Resourcepack, Obj.Game.ServerUrl);
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
