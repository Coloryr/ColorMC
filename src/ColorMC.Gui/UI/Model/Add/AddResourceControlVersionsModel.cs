using System;
using System.Collections.Generic;
using System.Linq;
using ColorMC.Core.Game;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Dialog;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Model.Add;

/// <summary>
/// 添加游戏资源
/// 文件列表
/// </summary>
public partial class AddResourceControlModel
{
    /// <summary>
    /// 安装所选项目
    /// </summary>
    /// <param name="item">项目</param>
    public override async void Install(FileItemModel item)
    {
        SetSelect(item);

        string? loadid = null;
        SourceType loadtype = SourceType.McMod;

        var type = SourceTypeList[Source];

        //如果是mcmod需要选择下载源
        if (type == SourceType.McMod)
        {
            var obj1 = item.McMod!;
            if (obj1.CurseforgeId != null && obj1.ModrinthId != null)
            {
                var dialog = new SelectModel(Window.WindowId)
                {
                    Text = LanguageUtils.Get("AddResourceWindow.Text22"),
                    Items = [.. _sourceTypeNameList]
                };
                if (await Window.ShowDialogWait(dialog) is not true)
                {
                    return;
                }
                loadtype = dialog.Index == 0 ? SourceType.CurseForge : SourceType.Modrinth;
                loadid = type == SourceType.CurseForge ? obj1.CurseforgeId : obj1.ModrinthId;
            }
            else if (obj1.CurseforgeId != null)
            {
                loadid = obj1.CurseforgeId;
                loadtype = SourceType.CurseForge;
            }
            else if (obj1.ModrinthId != null)
            {
                loadid = obj1.ModrinthId;
                loadtype = SourceType.Modrinth;
            }
        }
        else if (type == SourceType.CurseForge)
        {
            loadid = item.Pid;
            loadtype = SourceType.CurseForge;

        }
        else if (type == SourceType.Modrinth)
        {
            loadid = item.Pid;
            loadtype = SourceType.Modrinth;
        }

        if (loadtype == SourceType.McMod || loadid == null)
        {
            Window.Show(LanguageUtils.Get("AddResourceWindow.Text34"));
            return;
        }

        var dialog1 = Window.ShowProgress(LanguageUtils.Get("AddResourceWindow.Text18"));
        var res = await WebBinding.GetFileListAsync(loadtype, loadid, 0,
                GameVersionDownload, _now == FileType.Mod ? _obj.Loader : Loaders.Normal, _now);
        Window.CloseDialog(dialog1);
        if (res == null || res.List == null || res.List.Count == 0)
        {
            Window.Show(LanguageUtils.Get("AddResourceWindow.Text27"));
            return;
        }

        var item1 = res.List.First();
        if (item1.IsDownload)
        {
            var res1 = await Window.ShowChoice(LanguageUtils.Get("AddModPackWindow.Text40"));
            if (!res1)
            {
                return;
            }
        }

        Install(item1);
    }

    /// <summary>
    /// 开始下载文件
    /// </summary>
    /// <param name="data"></param>
    public override async void Install(FileVersionItemModel data)
    {
        if (data.IsDownload)
        {
            return;
        }

        ModInfoObj? mod = null;
        if (_now == FileType.Mod && _obj.Mods.TryGetValue(data.ID, out mod))
        {
            var res1 = await Window.ShowChoice(LanguageUtils.Get("AddResourceWindow.Text23"));
            if (!res1)
            {
                return;
            }
        }

        bool res = false;

        GameManager.StartAdd(_obj.UUID);
        try
        {
            //数据包
            if (_now == FileType.DataPacks)
            {
                //选择存档
                var list = await _obj.GetSavesAsync();
                if (list.Count == 0)
                {
                    Window.Show(LanguageUtils.Get("AddResourceWindow.Text29"));
                    return;
                }

                var world = new List<string>();
                list.ForEach(item => world.Add(item.LevelName));
                var dialog1 = new SelectModel(Window.WindowId)
                {
                    Text = LanguageUtils.Get("AddResourceWindow.Text19"),
                    Items = [.. world]
                };
                var res1 = await Window.ShowDialogWait(dialog1);
                if (res1 is not true)
                {
                    return;
                }
                var item = list[dialog1.Index];

                FileItemDownloadModel? info = null;
                if (data.SourceType == SourceType.CurseForge && data.Data is CurseForgeModObj.CurseForgeDataObj data1)
                {
                    info = new FileItemDownloadModel(Window)
                    {
                        Name = data1.DisplayName,
                        Type = FileType.DataPacks,
                        Source = data.SourceType,
                        Pid = data1.ModId.ToString()
                    };
                    StartDownload(info);
                    var pack = new ResourceGui(info);
                    res = await WebBinding.DownloadResourceAsync(item, data1, pack, info.Token);
                    pack.Stop();
                }
                else if (data.SourceType == SourceType.Modrinth && data.Data is ModrinthVersionObj data2)
                {
                    info = new FileItemDownloadModel(Window)
                    {
                        Name = data2.Name,
                        Type = FileType.DataPacks,
                        Source = data.SourceType,
                        Pid = data2.ProjectId
                    };
                    StartDownload(info);
                    var pack = new ResourceGui(info);
                    res = await WebBinding.DownloadResourceAsync(item, data2, pack, info.Token);
                    pack.Stop();
                }
                if (info != null)
                {
                    StopDownload(info, res);
                }
            }
            //模组
            else if (_now == FileType.Mod)
            {
                var dialog = Window.ShowProgress(LanguageUtils.Get("AddResourceWindow.Text40"));
                var list = data.SourceType switch
                {
                    SourceType.CurseForge => await WebBinding.GetDownloadModListAsync(_obj,
                    data.Data as CurseForgeModObj.CurseForgeDataObj),
                    SourceType.Modrinth => await WebBinding.GetDownloadModListAsync(_obj,
                    data.Data as ModrinthVersionObj),
                    _ => null
                };
                Window.CloseDialog(dialog);
                if (list == null)
                {
                    Window.Show(LanguageUtils.Get("AddResourceWindow.Text32"));
                    return;
                }

                if (list.List!.Count == 0)
                {
                    var info = new FileItemDownloadModel(Window)
                    {
                        Name = list.Info.Name,
                        Type = FileType.Mod,
                        Source = data.SourceType,
                        Pid = list.Info.ModId
                    };
                    StartDownload(info);
                    var pack = new ResourceGui(info);
                    res = await WebBinding.DownloadModAsync(_obj,
                    [
                        new DownloadModObj()
                            {
                                Item = list.Item!,
                                Info = list.Info!,
                                Old = await _obj.ReadModAsync(mod)
                            }
                    ], pack, info.Token);

                    StopDownload(info, res);
                }
                else
                {
                    if (await StartListTask(list, mod, data.SourceType,
                        data.ProjectName, data.Name) is { } value)
                    {
                        res = value;
                    }
                    else
                    {
                        return;
                    }
                }

            }
            else
            {
                res = data.SourceType switch
                {
                    SourceType.CurseForge => await WebBinding.DownloadAsync(_now, _obj,
                        data.Data as CurseForgeModObj.CurseForgeDataObj),
                    SourceType.Modrinth => await WebBinding.DownloadAsync(_now, _obj,
                        data.Data as ModrinthVersionObj),
                    _ => false
                };
            }

            //下载结束
            if (res)
            {
                Window.Notify(LanguageUtils.Get("Text.Downloaded"));
            }
            else
            {
                Window.Show(LanguageUtils.Get("AddResourceWindow.Text28"));
            }
        }
        catch (Exception e)
        {
            Logs.Error(LanguageUtils.Get("AddResourceWindow.Text31"), e);
            res = false;
        }
        finally
        {
            GameManager.StopAdd(_obj.UUID);
        }
    }
}
