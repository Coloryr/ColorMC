using Avalonia.Controls;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Items;
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

public partial class GameCloudModel : GameModel
{
    /// <summary>
    /// 导出的文件列表
    /// </summary>
    private FilesPage _files;

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

    public GameCloudModel(BaseModel model, GameSettingObj obj) : base(model, obj)
    {

    }

    [RelayCommand]
    public async Task MakeEnable()
    {
        if (Enable)
        {
            return;
        }

        Model.Progress("启用云同步中");
        var res = await WebBinding.StartCloud(Obj);
        Model.ProgressClose();
        if (res == null)
        {
            Model.ShowOk("云服务器错误", WindowClose);
            return;
        }
        if (res.Value == AddSaveState.Exist)
        {
            Model.Show("游戏实例已经启用同步了");
            return;
        }
        else if (res.Value == AddSaveState.Error)
        {
            Model.Show("游戏实例启用同步错误");
            return;
        }

        Model.Notify("同步已启用");
        Enable = true;
    }

    [RelayCommand]
    public async Task MakeDisable()
    {
        if (!Enable)
        {
            return;
        }

        var ok = await Model.ShowWait("关闭云同步会删除服务器上的所有东西，是否继续");
        if (!ok)
        {
            return;
        }

        Model.Progress("关闭云同步中");
        var res = await WebBinding.StopCloud(Obj);
        Model.ProgressClose();
        if (res == null)
        {
            Model.ShowOk("云服务器错误", WindowClose);
            return;
        }
        if (!res.Value)
        {
            Model.Show("云同步关闭失败");
            return;
        }

        Model.Notify("同步已关闭");
        Enable = false;
    }

    [RelayCommand]
    public async Task UploadConfig()
    {
        Model.Progress("正在打包");
        var files = _files.GetSelectItems();
        var data = GameCloudUtils.GetCloudData(Obj);
        string dir = Obj.GetBasePath();
        data.Config.Clear();
        foreach (var item in files)
        {
            data.Config.Add(item[(dir.Length + 1)..]);
        }
        string name = Path.GetFullPath(dir + "/config.zip");
        files.Remove(name);
        await new ZipUtils().ZipFile(name, files, dir);
        Model.ProgressUpdate("上传中");
        await GameCloudUtils.UploadConfig(Obj.UUID, name);
        File.Delete(name);
        await LoadCloud();
        data.ConfigTime = DateTime.Parse(ConfigTime);
        LocalConfigTime = ConfigTime;
        GameCloudUtils.Save();
        Model.ProgressClose();
    }

    [RelayCommand]
    public async Task DownloadConfig()
    {
        Model.Progress("正在下载");
        var data = GameCloudUtils.GetCloudData(Obj);
        string dir = Obj.GetBasePath();
        string name = Path.GetFullPath(dir + "/config.zip");
        var res = await GameCloudUtils.DownloadConfig(Obj.UUID, name);
        if (res != true)
        {
            Model.ProgressClose();
            Model.Show("同步失败");
            return;
        }
        Model.ProgressUpdate("解压中");
        data.Config.Clear();
        await Task.Run(() =>
        {
            using var s = new ZipInputStream(PathHelper.OpenRead(name));
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
        Model.ProgressClose();
    }

    public async Task LoadCloud()
    {
        Model.Progress("检查云同步中");
        var res = await WebBinding.CheckCloud(Obj);
        Model.ProgressClose();
        if (res.Item1 == null)
        {
            Model.ShowOk("云服务器错误", WindowClose);
            return;
        }
        Enable = (bool)res.Item1;
        ConfigTime = res.Item2 ?? "没有同步";
    }

    public async void Load()
    {
        if (!GameCloudUtils.Connect)
        {
            Model.ShowOk("云服务器未链接", WindowClose);
            return;
        }
        await LoadCloud();

        string dir = Obj.GetBasePath();
        _files = new FilesPage(dir, false);

        var data = GameCloudUtils.GetCloudData(Obj);
        LocalConfigTime = data.ConfigTime.ToString();

        var list = new List<string>();
        foreach (var item in data.Config)
        {
            list.Add(Path.GetFullPath(dir + "/" + item));
        }
        _files.SetSelectItems(list);

        Source = _files.Source;
    }

    public void WindowClose()
    {
        OnPropertyChanged("WindowClose");
    }

    protected override void Close()
    {
        _files = null!;
        WorldCloudList.Clear();
        Source = null!;
    }
}
