using ColorMC.Core.Helpers;
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
    public ObservableCollection<ServerPackItemModel> ModList { get; init; } = new();

    [ObservableProperty]
    private ServerPackItemModel _modItem;

    [RelayCommand]
    public void SelectAllMod()
    {
        foreach (var item in ModList)
        {
            item.Check = true;
            ModItemEdit(item);
        }
    }

    [RelayCommand]
    public void UnSelectAllMod()
    {
        foreach (var item in ModList)
        {
            item.Check = false;
            ModItemEdit(item);
        }
    }

    public void ModItemEdit()
    {
        ModItemEdit(ModItem);
    }

    private void ModItemEdit(ServerPackItemModel model)
    {
        var item = Obj.Mod?.FirstOrDefault(a => a.Sha1 == model.Sha1
                        && a.File == model.FileName);
        if (model.Check)
        {
            SourceType? source = null;
            if (model.Source == SourceType.CurseForge.GetName())
            {
                source = SourceType.CurseForge;
            }
            else if (model.Source == SourceType.Modrinth.GetName())
            {
                source = SourceType.Modrinth;
            }

            if (item != null)
            {
                item.Projcet = model.PID;
                item.FileId = model.FID;
                item.Source = source;
                item.Sha1 = model.Sha1;
                item.File = model.FileName;
            }
            else
            {
                Obj.Mod ??= new();
                item = new()
                {
                    Projcet = model.PID,
                    FileId = model.FID,
                    Source = source,
                    Sha1 = model.Sha1,
                    File = model.FileName
                };
                Obj.Mod.Add(item);
            }

            model.Url = item.Url = BaseBinding.MakeUrl(item, FileType.Mod, Obj.Url);
        }
        else
        {
            if (item != null)
            {
                model.Url = "";
                Obj.Mod?.Remove(item);
            }
        }

        GameBinding.SaveServerPack(Obj);
    }

    public async void LoadMod()
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
}
