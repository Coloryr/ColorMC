using ColorMC.Core.Objs;
using ColorMC.Core.Objs.ServerPack;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace ColorMC.Gui.UI.Model.ServerPack;

public partial class ServerPackTab3Model : ServerPackBaseModel
{
    public ObservableCollection<ServerPackItemModel> ModList { get; init; } = new();

    [ObservableProperty]
    private ServerPackItemModel _item;

    public ServerPackTab3Model(IUserControl con, ServerPackObj obj) : base(con, obj)
    {

    }

    [RelayCommand]
    public void SelectAll()
    {
        foreach (var item in ModList)
        {
            item.Check = true;
            ItemEdit(item);
        }
    }

    [RelayCommand]
    public void UnSelectAll()
    {
        foreach (var item in ModList)
        {
            item.Check = false;
            ItemEdit(item);
        }
    }

    public void ItemEdit()
    {
        ItemEdit(Item);
    }

    public async void Load()
    {
        ModList.Clear();
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

    private void ItemEdit(ServerPackItemModel obj)
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

    public override void Close()
    {
        throw new System.NotImplementedException();
    }
}
