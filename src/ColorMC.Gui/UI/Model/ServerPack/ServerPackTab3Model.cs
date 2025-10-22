using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using ColorMC.Core.Game;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model.Items;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.ServerPack;

public partial class ServerPackModel
{
    /// <summary>
    /// 资源包列表
    /// </summary>
    public ObservableCollection<ServerPackItemModel> ResourceList { get; init; } = [];

    /// <summary>
    /// 选中的资源包
    /// </summary>
    [ObservableProperty]
    private ServerPackItemModel _resourceItem;

    /// <summary>
    /// 选中所有资源
    /// </summary>
    [RelayCommand]
    public void SelectAllResource()
    {
        foreach (var item in ResourceList)
        {
            item.Check = true;
            ResourceItemEdit(item);
        }
    }

    /// <summary>
    /// 取消选中所有资源
    /// </summary>
    [RelayCommand]
    public void UnSelectAllResource()
    {
        foreach (var item in ResourceList)
        {
            item.Check = false;
            ResourceItemEdit(item);
        }
    }

    /// <summary>
    /// 编辑资源项目
    /// </summary>
    public void ResourceItemEdit()
    {
        ResourceItemEdit(ResourceItem);
    }

    /// <summary>
    /// 加载资源项目列表
    /// </summary>
    public async void LoadResourceList()
    {
        ResourceList.Clear();
        var mods = await Obj.Game.GetResourcepacksAsync(true);

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
                    item2.Url = UrlHelper.MakeUrl(item1, FileType.Resourcepack, Obj.Game.ServerUrl);
                }
            }

            ResourceList.Add(item2);
        });

        Obj.Save();
    }

    /// <summary>
    /// 编辑资源项目
    /// </summary>
    /// <param name="obj">服务器包资源项目</param>
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

            obj.Url = item.Url = UrlHelper.MakeUrl(item, FileType.Resourcepack, Obj.Game.ServerUrl);
        }
        else
        {
            if (item != null)
            {
                obj.Url = "";
                Obj.Resourcepack?.Remove(item);
            }
        }

        Obj.Save();
    }
}
