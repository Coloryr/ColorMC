using Avalonia.Controls;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameCloud;

public partial class GameCloudModel : GameEditModel
{
    /// <summary>
    /// 导出的文件列表
    /// </summary>
    public FilesPageViewModel Files;

    [ObservableProperty]
    private HierarchicalTreeDataGridSource<FileTreeNodeModel> _source;

    [ObservableProperty]
    private bool _enable;
    [ObservableProperty]
    private string _configTime;
    [ObservableProperty]
    private string _localConfigTime;

    public ObservableCollection<WorldCloudModel> WorldCloudList { get; } = new();

    public string UUID => Obj.UUID;

    public GameCloudModel(IUserControl con, GameSettingObj obj) : base(con, obj)
    {

    }

    [RelayCommand]
    public async Task MakeEnable()
    {
        if (Enable)
        {
            return;
        }

        Progress("启用云同步中");
        var res = await WebBinding.StartCloud(Obj);
        ProgressClose();
        if (res == null)
        {
            ShowOk("云服务器错误", Window.Close);
            return;
        }
        if (res.Value == AddSaveState.Exist)
        {
            Show("游戏实例已经启用同步了");
            return;
        }
        else if (res.Value == AddSaveState.Error)
        {
            Show("游戏实例启用同步错误");
            return;
        }

        Notify("同步已启用");
        Enable = true;
    }

    [RelayCommand]
    public async Task MakeDisable()
    {
        if (!Enable)
        {
            return;
        }

        var ok = await ShowWait("关闭云同步会删除服务器上的所有东西，是否继续");
        if (!ok)
        {
            return;
        }

        Progress("关闭云同步中");
        var res = await WebBinding.StopCloud(Obj);
        ProgressClose();
        if (res == null)
        {
            ShowOk("云服务器错误", Window.Close);
            return;
        }
        if (!res.Value)
        {
            Show("云同步关闭失败");
            return;
        }

        Notify("同步已关闭");
        Enable = false;
    }

    [RelayCommand]
    public async Task UploadConfig()
    {
        Progress("正在打包");
        var files = Files.GetSelectItems();
        var data = GameCloudUtils.GetCloudData(Obj);
        string dir = Obj.GetBasePath();
        data.Config.Clear();
        foreach (var item in files)
        {
            data.Config.Add(item[(dir.Length + 1)..]);
        }
        string name = Path.GetFullPath(dir + "/config.zip");
        files.Remove(name);
        await ZipUtils.ZipFile(name, files, dir);
        ProgressUpdate("上传中");
        await GameCloudUtils.UploadConfig(Obj.UUID, name);
        File.Delete(name);
        await LoadCloud();
        data.ConfigTime = DateTime.Parse(ConfigTime);
        LocalConfigTime = ConfigTime;
        GameCloudUtils.Save();
        ProgressClose();
    }

    [RelayCommand]
    public async Task DownloadConfig()
    {
        Progress("正在下载");
        var data = GameCloudUtils.GetCloudData(Obj);
        string dir = Obj.GetBasePath();
        string name = Path.GetFullPath(dir + "/config.zip");
        var res = await GameCloudUtils.DownloadConfig(Obj.UUID, name);
        if (res != true)
        {
            ProgressClose();
            Show("同步失败");
            return;
        }
        ProgressUpdate("解压中");
        data.Config.Clear();
        await Task.Run(() =>
        {
            using ZipInputStream s = new(File.OpenRead(name));
            ZipEntry theEntry;
            while ((theEntry = s.GetNextEntry()) != null)
            {
                string filename = $"{dir}/{theEntry.Name}";
                data.Config.Add(theEntry.Name);

                var directoryName = Path.GetDirectoryName(filename);
                string fileName = Path.GetFileName(theEntry.Name);

                if (directoryName?.Length > 0)
                {
                    Directory.CreateDirectory(directoryName);
                }

                if (fileName != string.Empty)
                {
                    using var streamWriter = File.Create(filename);

                    s.CopyTo(streamWriter);
                }
            }
        });
        await LoadCloud();
        data.ConfigTime = DateTime.Parse(ConfigTime);
        LocalConfigTime = ConfigTime;
        GameCloudUtils.Save();
        ProgressClose();
    }

    public async Task LoadCloud()
    {
        Progress("检查云同步中");
        var res = await WebBinding.CheckCloud(Obj);
        ProgressClose();
        if (res.Item1 == null)
        {
            ShowOk("云服务器错误", Window.Close);
            return;
        }
        Enable = (bool)res.Item1;
        ConfigTime = res.Item2 ?? "没有同步";
    }

    public async void Load()
    {
        if (!GameCloudUtils.Connect)
        {
            ShowOk("云服务器未链接", Window.Close);
            return;
        }
        await LoadCloud();

        string dir = Obj.GetBasePath();
        Files = new FilesPageViewModel(dir, false);

        var data = GameCloudUtils.GetCloudData(Obj);
        LocalConfigTime = data.ConfigTime.ToString();

        var list = new List<string>();
        foreach (var item in data.Config)
        {
            list.Add(Path.GetFullPath(dir + "/" + item));
        }
        Files.SetSelectItems(list);

        Source = Files.Source;
    }
}
