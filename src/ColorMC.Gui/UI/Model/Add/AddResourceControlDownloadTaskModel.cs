using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ColorMC.Core.Game;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddResourceControlModel
{
    /// <summary>
    /// 下载的模组
    /// </summary>
    [ObservableProperty]
    private FileModVersionModel? _mod;

    /// <summary>
    /// 文件列表是否显示下一页
    /// </summary>
    [ObservableProperty]
    private bool _nextPageDisplay;

    /// <summary>
    /// 下载一个列表的模组
    /// </summary>
    /// <param name="task"></param>
    private async Task<bool> DownloadListItem(ModDependModel task)
    {
        if (task.DownloadModList.Any(item => item is ModUpgradeModel))
        {
            var list = task.DownloadModList.Where(item => item is ModUpgradeModel && item.Download)
                .Select(item => new DownloadModObj()
                {
                    Info = item.Items[item.SelectVersion].Info,
                    Item = item.Items[item.SelectVersion].Item,
                    Old = (item as ModUpgradeModel)!.Obj
                });

            var list1 = new List<FileItemDownloadModel>();
            foreach (var item in list)
            {
                var info = new FileItemDownloadModel(Window)
                {
                    Name = item.Item.Name,
                    Type = FileType.Mod,
                    Source = task.Source,
                    Pid = item.Info.ModId
                };
                StartDownload(info);
                list1.Add(info);
            }
            var info1 = new FileItemDownloadModel(Window)
            {
                Name = LanguageUtils.Get("AddResourceWindow.Text37"),
                Type = FileType.Mod,
                Source = SourceType.ColorMC,
                Pid = ""
            };
            var pack = new ResourceGui(info1);
            var res = await WebBinding.DownloadModAsync(_obj, list, pack, info1.Token);
            pack.Stop();
            foreach (var item in list1)
            {
                StopDownload(item, res);
            }

            return res;
        }
        else
        {
            var list = task.DownloadModList.Where(item => item.Download && !item.IsDisable)
                                .Select(item => item.Items[item.SelectVersion]);

            var mod = task.DownloadModList.First();
            var info = new FileItemDownloadModel(Window)
            {
                Name = mod.Name,
                Type = FileType.Mod,
                Source = task.Source,
                Pid = task.Modsave.Info.ModId,
                SubPid = [.. list.Select(item1 => item1.Info.ModId)]
            };

            StartDownload(info);
            var pack = new ResourceGui(info);
            var res = await WebBinding.DownloadModAsync(_obj, [task.Modsave, .. list], pack, info.Token);
            StopDownload(info, res);

            return res;
        }
    }

    private async Task<bool?> StartListTask(ModDownloadListRes list, ModInfoObj? mod, 
        SourceType source, string name, string version)
    {
        var self = new FileModVersionModel(name, [version], [new DownloadModObj()
        {
            Item = list.Item!,
            Info = list.Info!,
            Old = await _obj.ReadModAsync(mod)
        }], false)
        {
            IsDisable = true
        };

        //添加模组信息
        var dialog = new ModDependModel(Window.WindowId, [self, .. list.List], source);

        var res = await Window.ShowDialogWait(dialog);
        if (res is true)
        {
            return await DownloadListItem(dialog);
        }

        return null;
    }

    /// <summary>
    /// 升级所选的模组
    /// </summary>
    /// <param name="list"></param>
    public async void Upgrade(ICollection<ModUpgradeModel> list)
    {
        var dialog = new ModDependModel(Window.WindowId, list);
        var res = await Window.ShowDialogWait(dialog);
        if (res is true)
        {
            await DownloadListItem(dialog);
        }
    }
}
