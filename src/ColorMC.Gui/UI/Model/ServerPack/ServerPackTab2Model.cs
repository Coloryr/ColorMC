using ColorMC.Core.Helpers;
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

public partial class ServerPackTab2Model : ServerPackBaseModel
{
    public ObservableCollection<ServerPackItemModel> ModList { get; init; } = new();

    [ObservableProperty]
    private ServerPackItemModel _item;

    public ServerPackTab2Model(IUserControl con, ServerPackObj obj) : base(con, obj)
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

    private void ItemEdit(ServerPackItemModel obj)
    {
        var item = Obj.Mod?.FirstOrDefault(a => a.Sha1 == obj.Sha1
                        && a.File == obj.FileName);
        if (obj.Check)
        {
            SourceType? source = null;
            if (obj.Source == SourceType.CurseForge.GetName())
            {
                source = SourceType.CurseForge;
            }
            else if (obj.Source == SourceType.Modrinth.GetName())
            {
                source = SourceType.Modrinth;
            }

            if (item != null)
            {
                item.Projcet = obj.PID;
                item.FileId = obj.FID;
                item.Source = source;
                item.Sha1 = obj.Sha1;
                item.File = obj.FileName;
            }
            else
            {
                Obj.Mod ??= new();
                item = new()
                {
                    Projcet = obj.PID,
                    FileId = obj.FID,
                    Source = source,
                    Sha1 = obj.Sha1,
                    File = obj.FileName
                };
                Obj.Mod.Add(item);
            }

            obj.Url = item.Url = BaseBinding.MakeUrl(item, FileType.Mod, Obj.Url);
        }
        else
        {
            if (item != null)
            {
                obj.Url = "";
                Obj.Mod?.Remove(item);
            }
        }

        GameBinding.SaveServerPack(Obj);
    }

    public async void Load()
    {
        ModList.Clear();
        var mods = await GameBinding.GetGameMods(Obj.Game);

        Obj.Mod?.RemoveAll(a => mods.Find(b => a.Sha1 == b.Obj.Sha1) == null);

        mods.ForEach(item =>
        {
            if (item.Obj.ReadFail)
                return;

            string file = Path.GetFileName(item.Obj.Local);

            var item1 = Obj.Mod?.FirstOrDefault(a => a.Sha1 == item.Obj.Sha1
                        && a.File == file);

            var item2 = new ServerPackItemModel()
            {
                FileName = file,
                Check = item1 != null,
                PID = item.PID,
                FID = item.FID,
                Sha1 = item.Obj.Sha1,
                Mod = item
            };

            if (item2.Check)
            {
                if (item1 != null)
                {
                    item2.PID = item1.Projcet;
                    item2.FID = item1.FileId;
                    if (!string.IsNullOrWhiteSpace(item1.Url))
                    {
                        item2.Url = item1.Url;
                    }
                    else
                    {
                        item2.Url = BaseBinding.MakeUrl(item1, FileType.Mod, Obj.Url);
                    }
                }
            }

            ModList.Add(item2);
        });

        GameBinding.SaveServerPack(Obj);
    }

    public override void Close()
    {
        ModList.Clear();
        _item = null;
    }
}
