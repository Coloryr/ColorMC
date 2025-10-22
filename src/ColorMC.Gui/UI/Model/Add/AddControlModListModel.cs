using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ColorMC.Core.Game;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddControlModel
{
    /// <summary>
    /// 多个模组下载任务
    /// </summary>
    private record ModListDownloadTask
    {
        /// <summary>
        /// 下载的模组项目
        /// </summary>
        public required DownloadModArg? Modsave;
        /// <summary>
        /// 下载源
        /// </summary>
        public required SourceType Source;

        /// <summary>
        /// 下载结果
        /// </summary>
        public bool TaskRes;
        /// <summary>
        /// 是否结束
        /// </summary>
        public bool IsEnd;
    }

    /// <summary>
    /// 多个模组下载任务
    /// </summary>
    private ModListDownloadTask? _nowTask;

    /// <summary>
    /// Mod下载项目显示列表
    /// </summary>
    private readonly List<FileModVersionModel> _modList = [];

    /// <summary>
    /// 下载所选模组
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public void DownloadMod()
    {
        if (_nowTask != null)
        {
            DownloadListItem(_nowTask);
        }
    }

    /// <summary>
    /// 选择下载所有模组
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public void DownloadAllMod()
    {
        foreach (var item in DownloadModList)
        {
            item.Download = true;
        }

        if (_nowTask != null)
        {
            DownloadListItem(_nowTask);
        }
    }

    /// <summary>
    /// 下载一个列表的模组
    /// </summary>
    /// <param name="task"></param>
    private async void DownloadListItem(ModListDownloadTask task)
    {
        CloseModDownloadDisplay();

        if (DownloadModList.Any(item => item is ModUpgradeModel))
        {
            var list = DownloadModList.Where(item => item is ModUpgradeModel && item.Download)
                .Select(item => new DownloadModArg()
                {
                    Info = item.Items[item.SelectVersion].Info,
                    Item = item.Items[item.SelectVersion].Item,
                    Old = (item as ModUpgradeModel)!.Obj
                }).ToList();

            var list1 = new List<DownloadItemInfo>();
            foreach (var item in list)
            {
                var info = new DownloadItemInfo
                {
                    Type = FileType.Mod,
                    Source = task.Source,
                    PID = item.Info.ModId
                };
                StartDownload(info);
                list1.Add(info);
            }

            task.TaskRes = await WebBinding.DownloadModAsync(Obj, list);

            foreach (var item in list1)
            {
                StopDownload(item, task.TaskRes);
            }
        }
        else
        {
            var list = DownloadModList.Where(item => item.Download)
                                .Select(item => item.Items[item.SelectVersion]).ToList();

            if (task.Modsave != null)
            {
                list.Add(task.Modsave);
            }

            var list1 = new List<DownloadItemInfo>();
            foreach (var item in list)
            {
                var info = new DownloadItemInfo
                {
                    Type = FileType.Mod,
                    Source = task.Source,
                    PID = item.Info.ModId
                };
                StartDownload(info);
                list1.Add(info);
            }

            task.TaskRes = await WebBinding.DownloadModAsync(Obj, list);

            foreach (var item in list1)
            {
                StopDownload(item, task.TaskRes);
            }
        }

        task.IsEnd = true;
    }

    private async Task<bool> StartListTask(ModDownloadListRes list, ModInfoObj? mod, SourceType source)
    {
        if (ModDownloadDisplay)
        {
            await Task.Run(async () =>
            {
                while (ModDownloadDisplay)
                {
                    await Task.Delay(500);
                }
            });
        }

        //添加模组信息
        _modList.Clear();
        _modList.AddRange(list.List);
        _modList.ForEach(item =>
        {
            if (item.Optional == false)
            {
                item.Download = true;
            }
        });
        _nowTask = new ModListDownloadTask()
        {
            Modsave = new DownloadModArg()
            {
                Item = list.Item!,
                Info = list.Info!,
                Old = await Obj.ReadModAsync(mod)
            },
            Source = source
        };

        OpenModDownloadDisplay();

        ModsLoad();

        await Task.Run(async () =>
        {
            while (!_nowTask.IsEnd)
            {
                await Task.Delay(500);
            }
        });

        return _nowTask.TaskRes;
    }
}
