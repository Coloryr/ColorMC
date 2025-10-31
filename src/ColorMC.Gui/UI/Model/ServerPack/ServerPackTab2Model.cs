using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.ServerPack;

public partial class ServerPackModel
{
    /// <summary>
    /// 模组列表
    /// </summary>
    public ObservableCollection<ServerPackItemModel> ModList { get; init; } = [];

    /// <summary>
    /// 选中的模组项目
    /// </summary>
    [ObservableProperty]
    private ServerPackItemModel _modItem;

    /// <summary>
    /// 选中所有模组
    /// </summary>
    [RelayCommand]
    public void SelectAllMod()
    {
        foreach (var item in ModList)
        {
            item.Check = true;
            ModItemEdit(item);
        }
    }

    /// <summary>
    /// 取消选中所有模组
    /// </summary>
    [RelayCommand]
    public void UnSelectAllMod()
    {
        foreach (var item in ModList)
        {
            item.Check = false;
            ModItemEdit(item);
        }
    }

    /// <summary>
    /// 模组编辑
    /// </summary>
    public void ModItemEdit()
    {
        ModItemEdit(ModItem);
    }

    /// <summary>
    /// 模组编辑
    /// </summary>
    /// <param name="model">服务器包模组项目</param>
    private void ModItemEdit(ServerPackItemModel model)
    {
        var item = Obj.Mod?.FirstOrDefault(a => a.Sha256 == model.Sha256
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
                item.Sha256 = model.Sha256;
                item.File = model.FileName;
            }
            else
            {
                Obj.Mod ??= [];
                item = new()
                {
                    Projcet = model.PID,
                    FileId = model.FID,
                    Source = source,
                    Sha256 = model.Sha256,
                    File = model.FileName
                };
                Obj.Mod.Add(item);
            }

            model.Url = item.Url = UrlHelper.MakeUrl(item, FileType.Mod, Obj.Game.ServerUrl);
        }
        else
        {
            if (item != null)
            {
                model.Url = "";
                Obj.Mod?.Remove(item);
            }
        }

        Obj.Save();
    }

    /// <summary>
    /// 加载模组列表
    /// </summary>
    public async void LoadMod()
    {
        ModList.Clear();
        var mods = await GameBinding.GetGameModsAsync(Obj.Game, true);

        Obj.Mod?.RemoveAll(a => mods.Find(b => a.Sha256 == b.Sha256) == null);

        mods.ForEach(item =>
        {
            if (item.ReadFail)
            {
                return;
            }

            string file = Path.GetFileName(item.Local);

            var item1 = Obj.Mod?.FirstOrDefault(a => a.Sha256 == item.Sha256
                        && a.File == file);

            var obj1 = Obj.Game.Mods.Values.FirstOrDefault(a => a.Sha1 == item.Sha1);

            var item2 = new ServerPackItemModel()
            {
                FileName = file,
                Sha256 = item.Sha256,
                Mod = new ModDisplayModel(item, obj1, null)
            };

            if (item1 != null)
            {
                item2.PID = item1.Projcet;
                item2.FID = item1.FileId;
                item2.Check = true;
                if (!string.IsNullOrWhiteSpace(item1.Url))
                {
                    item2.Url = item1.Url;
                }
                else
                {
                    item2.Url = UrlHelper.MakeUrl(item1, FileType.Mod, Obj.Game.ServerUrl);
                }
            }

            ModList.Add(item2);
        });

        Obj.Save();
    }
}
