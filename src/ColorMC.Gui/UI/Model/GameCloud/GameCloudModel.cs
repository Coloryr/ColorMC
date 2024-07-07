﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Newtonsoft.Json;

namespace ColorMC.Gui.UI.Model.GameCloud;

public partial class GameCloudModel : MenuModel
{
    [ObservableProperty]
    private bool _displayFilter = true;

    /// <summary>
    /// 导出的文件列表
    /// </summary>
    private FilesPageModel _files;

    [ObservableProperty]
    private HierarchicalTreeDataGridSource<FileTreeNodeModel> _source;

    [ObservableProperty]
    private bool _enable;
    [ObservableProperty]
    private string _configTime;
    [ObservableProperty]
    private string _localConfigTime;

    private bool _configHave;

    private WorldCloudModel? _selectWorld;

    public GameSettingObj Obj { get; init; }

    public ObservableCollection<WorldCloudModel> WorldCloudList { get; } = [];

    public string UUID => Obj.UUID;

    private readonly string _useName;

    public GameCloudModel(BaseModel model, GameSettingObj obj) : base(model)
    {
        _useName = ToString() + ":" + obj.UUID;

        Obj = obj;

        SetHeadBack();

        LoadWorld();

        SetMenu(
        [
            new()
            {
                Icon = "/Resource/Icon/GameExport/item1.svg",
                Text = App.Lang("GameCloudWindow.Tabs.Text1")
            },
            new()
            {
                Icon = "/Resource/Icon/GameExport/item2.svg",
                Text = App.Lang("GameCloudWindow.Tabs.Text2")
            },
            new()
            {
                Icon = "/Resource/Icon/GameExport/item4.svg",
                Text = App.Lang("GameCloudWindow.Tabs.Text3")
            }
        ]);
    }

    [RelayCommand]
    public async Task MakeEnable()
    {
        if (Enable)
        {
            return;
        }

        Model.Progress(App.Lang("GameCloudWindow.Info3"));
        var res = await GameCloudUtils.StartCloud(Obj);
        Model.ProgressClose();
        if (res == 101)
        {
            Model.Show(App.Lang("GameCloudWindow.Error2"));
            return;
        }
        else if (res != 100)
        {
            Model.Show(App.Lang("GameCloudWindow.Error3"));
            return;
        }

        Model.Notify(App.Lang("GameCloudWindow.Info4"));
        Enable = true;
    }

    [RelayCommand]
    public async Task MakeDisable()
    {
        if (!Enable)
        {
            return;
        }

        var ok = await Model.ShowWait(App.Lang("GameCloudWindow.Info7"));
        if (!ok)
        {
            return;
        }

        Model.Progress(App.Lang("GameCloudWindow.Info5"));
        var res = await GameCloudUtils.StopCloud(Obj);
        Model.ProgressClose();
        if (res == 101)
        {
            Model.Show(App.Lang("GameCloudWindow.Error2"));
            return;
        }
        else if (res == 102)
        {
            Model.Show(App.Lang("GameCloudWindow.Error4"));
            return;
        }
        else if (res != 100)
        {
            Model.Show(App.Lang("GameCloudWindow.Error3"));
            return;
        }


        Model.Notify(App.Lang("GameCloudWindow.Info6"));
        Enable = false;
    }

    [RelayCommand]
    public async Task UploadConfig()
    {
        Model.Progress(App.Lang("GameCloudWindow.Info8"));
        var files = _files.GetSelectItems(true);
        var data = GameCloudUtils.GetCloudData(Obj);
        string dir = Obj.GetBasePath();
        data.Config ??= [];
        data.Config.Clear();
        foreach (var item in files)
        {
            data.Config.Add(item[(dir.Length + 1)..]);
        }
        string name = Path.GetFullPath(dir + "/config.zip");
        files.Remove(name);
        await new ZipUtils().ZipFileAsync(name, files, dir);
        Model.ProgressUpdate(App.Lang("GameCloudWindow.Info9"));
        var res = await GameCloudUtils.UploadConfig(Obj, name);
        PathHelper.Delete(name);
        Model.ProgressClose();
        if (res == 104)
        {
            Model.Show(App.Lang("GameCloudWindow.Error5"));
            return;
        }
        else if (res == 101)
        {
            Model.Show(App.Lang("GameCloudWindow.Error2"));
            return;
        }
        else if (res != 100)
        {
            Model.Show(App.Lang("GameCloudWindow.Error3"));
            return;
        }
        Model.Notify(App.Lang("GameCloudWindow.Info14"));
        await LoadCloud();
        if (_configHave)
        {
            data.ConfigTime = DateTime.Parse(ConfigTime);
        }
        LocalConfigTime = ConfigTime;
        GameCloudUtils.Save();
    }

    [RelayCommand]
    public async Task DownloadConfig()
    {
        Model.Progress(App.Lang("GameCloudWindow.Info10"));
        var data = GameCloudUtils.GetCloudData(Obj);
        string local = Path.GetFullPath(Obj.GetBasePath() + "/config.zip");
        var res = await GameCloudUtils.DownloadConfig(Obj, local);
        if (res == 101)
        {
            Model.Show(App.Lang("GameCloudWindow.Error2"));
            return;
        }
        else if (res != 100)
        {
            Model.Show(App.Lang("GameCloudWindow.Error3"));
            return;
        }

        Model.ProgressUpdate(App.Lang("GameCloudWindow.Info11"));
        var res1 = await GameBinding.UnZipCloudConfig(Obj, data, local);
        Model.ProgressClose();
        if (!res1)
        {
            Model.Show(App.Lang("GameCloudWindow.Error6"));
            return;
        }
        await LoadCloud();
        if (_configHave)
        {
            data.ConfigTime = DateTime.Parse(ConfigTime);
        }
        LocalConfigTime = ConfigTime;
        GameCloudUtils.Save();
    }

    /// <summary>
    /// 获取游戏实例是否开启了云同步
    /// </summary>
    public async Task LoadCloud()
    {
        Model.Progress(App.Lang("GameCloudWindow.Info1"));
        var res = await GameCloudUtils.HaveCloud(Obj);
        Model.ProgressClose();
        Enable = res.Item2;
        _configHave = res.Item3 != null;
        ConfigTime = res.Item3 ?? App.Lang("GameCloudWindow.Info2");
    }

    public async Task<bool> Init()
    {
        if (!GameCloudUtils.Connect)
        {
            Model.ShowOk(App.Lang("GameCloudWindow.Erro1"), WindowClose);
            return false;
        }
        await LoadCloud();
        await LoadConfig();

        return true;
    }

    /// <summary>
    /// 加载数据
    /// </summary>
    public async Task LoadConfig()
    {
        string dir = Obj.GetBasePath();
        _files = new FilesPageModel(dir, false);
        var data = GameCloudUtils.GetCloudData(Obj);
        LocalConfigTime = data.ConfigTime.ToString();

        var list = new List<string>()
        {
            Obj.GetGameJsonFile(),
            Obj.GetModInfoJsonFile(),
            Obj.GetIconFile(),
            Obj.GetLaunchFile(),
            Obj.GetModPackJsonFile()
        };
        _files.SetSelectItems(list);

        list.Clear();
        if (data.Config != null)
        {
            foreach (var item in data.Config)
            {
                list.Add(Path.GetFullPath(dir + "/" + item));
            }
        }
        else
        {
            list.Add(Obj.GetConfigPath());
            list.Add(Obj.GetOptionsFile());
            foreach (var mod in await GameBinding.GetGameMods(Obj))
            {
                if (mod.Obj1 == null)
                {
                    list.Add(mod.Local);
                }
            }
        }
        _files.SetSelectItems(list);

        Source = _files.Source;
    }

    public void SetSelectWorld(WorldCloudModel item)
    {
        if (_selectWorld != null)
        {
            _selectWorld.IsSelect = false;
        }
        _selectWorld = item;
        _selectWorld.IsSelect = true;
    }

    public async void LoadWorld()
    {
        WorldCloudList.Clear();
        var res = await GameCloudUtils.GetWorldList(Obj);
        var worlds = await GameBinding.GetWorlds(Obj);
        if (res != null)
        {
            foreach (var item in res)
            {
                var obj = worlds.Find(a => a.LevelName == item.Name);
                if (obj != null)
                {
                    worlds.Remove(obj);
                    WorldCloudList.Add(new(this, item, obj));
                }
                else
                {
                    WorldCloudList.Add(new(this, item));
                }
            }
        }
        foreach (var item in worlds)
        {
            WorldCloudList.Add(new(this, item));
        }
    }

    public override void Close()
    {
        _files = null!;
        WorldCloudList.Clear();
        RemoveHeadBack();
        Source = null!;
    }

    public async void UploadWorld(WorldCloudModel world)
    {
        string dir = world.World.Local;
        string local = Path.GetFullPath(world.World.Game.GetSavesPath() + "/" + world.World.LevelName + ".zip");
        if (!world.HaveCloud)
        {
            Model.Progress(App.Lang("GameCloudWindow.Info8"));
            await new ZipUtils().ZipFileAsync(dir, local);
        }
        else
        {
            Model.Progress(App.Lang("GameCloudWindow.Info12"));
            //云端文件
            var list = await GameCloudUtils.GetWorldFiles(Obj, world.World);
            if (list == null)
            {
                Model.ProgressClose();
                Model.Show(App.Lang("GameCloudWindow.Error7"));
                return;
            }
            //本地文件
            var files = PathHelper.GetAllFile(dir);
            var pack = new List<string>();

            string dir1 = Path.GetFullPath(dir + "/");
            foreach (var item in files)
            {
                var name = item.FullName.Replace(dir1, "").Replace("\\", "/");
                using var file = PathHelper.OpenRead(item.FullName)!;
                var sha1 = await HashHelper.GenSha1Async(file);
                if (list.TryGetValue(name, out var sha11))
                {
                    list.Remove(name);
                    if (sha1 != sha11)
                    {
                        pack.Add(item.FullName);
                    }
                }
                else
                {
                    pack.Add(item.FullName);
                }
            }

            bool have = false;
            var delete = Path.GetFullPath(dir + "/delete.json");
            if (list.Count > 0)
            {
                have = true;
                await File.WriteAllTextAsync(delete, JsonConvert.SerializeObject(list.Keys));
                pack.Add(delete);
            }
            if (pack.Count == 0)
            {
                Model.ProgressClose();
                Model.Show(App.Lang("GameCloudWindow.Info13"));
                return;
            }
            Model.ProgressUpdate(App.Lang("GameCloudWindow.Info8"));
            await new ZipUtils().ZipFileAsync(local, pack, dir);
            if (have)
            {
                PathHelper.Delete(delete);
            }
        }

        Model.ProgressUpdate(App.Lang("GameCloudWindow.Info9"));
        var res = await GameCloudUtils.UploadWorld(Obj, world.World, local);
        PathHelper.Delete(local);
        Model.ProgressClose();
        if (res == 101)
        {
            Model.Show(App.Lang("GameCloudWindow.Error2"));
            return;
        }
        else if (res == 104)
        {
            Model.Show(App.Lang("GameCloudWindow.Error5"));
            return;
        }
        else if (res != 100)
        {
            Model.Show(App.Lang("GameCloudWindow.Error3"));
            return;
        }
        else
        {
            Model.Notify(App.Lang("GameCloudWindow.Info14"));
        }
        LoadWorld();
    }

    public async void DownloadWorld(WorldCloudModel world)
    {
        Model.Progress(App.Lang("GameCloudWindow.Info10"));
        string dir = Obj.GetSavesPath() + "/" + world.Cloud.Name;
        string local = Path.GetFullPath(Obj.GetSavesPath() + "/" + world.Cloud.Name + ".zip");
        var list = new Dictionary<string, string>();
        if (world.HaveLocal)
        {
            var files = PathHelper.GetAllFile(dir);
            string dir1 = Path.GetFullPath(dir + "/");
            foreach (var item in files)
            {
                var name = item.FullName.Replace(dir1, "").Replace("\\", "/");
                using var file = PathHelper.OpenRead(item.FullName)!;
                var sha1 = await HashHelper.GenSha1Async(file);
                list.Add(name, sha1);
            }
        }

        var res = await GameCloudUtils.DownloadWorld(Obj, world.Cloud, local, list);
        if (res == 101)
        {
            Model.Show(App.Lang("GameCloudWindow.Error2"));
            return;
        }
        else if (res == 102)
        {
            Model.ProgressClose();
            Model.Show(App.Lang("GameCloudWindow.Error8"));
            return;
        }
        else if (res == 103)
        {
            Model.ProgressClose();
            Model.Show(App.Lang("GameCloudWindow.Info13"));
            return;
        }
        else if (res != 100)
        {
            Model.Show(App.Lang("GameCloudWindow.Error3"));
        }
        else
        {
            Model.ProgressUpdate(App.Lang("GameCloudWindow.Info11"));
            try
            {
                using var file = PathHelper.OpenRead(local)!;
                await new ZipUtils().UnzipAsync(dir, local, file);
                Model.Notify(App.Lang("GameCloudWindow.Info15"));
            }
            catch (Exception e)
            {
                string temp = App.Lang("GameCloudWindow.Error9");
                Logs.Error(temp, e);
                Model.Show(temp);
            }
            Model.ProgressClose();
        }
        PathHelper.Delete(local);
        LoadWorld();
    }

    public async void DeleteCloud(WorldCloudModel world)
    {
        var res = await Model.ShowWait(App.Lang("GameCloudWindow.Info16"));
        if (!res)
        {
            return;
        }

        Model.Progress(App.Lang("GameCloudWindow.Info18"));
        var res1 = await GameCloudUtils.DeleteWorld(Obj, world.Cloud.Name);
        Model.ProgressClose();
        if (res1 == 101)
        {
            Model.Show(App.Lang("GameCloudWindow.Error2"));
            return;
        }
        else if (res1 == 102)
        {

            Model.Show(App.Lang("GameCloudWindow.Error8"));
            return;
        }
        else if (res1 != 100)
        {
            Model.Show(App.Lang("GameCloudWindow.Error3"));
            return;
        }
        else
        {
            Model.Notify(App.Lang("GameCloudWindow.Info17"));
        }
    }

    private async void Reload()
    {
        switch (NowView)
        {
            case 0:
                await LoadCloud();
                break;
            case 1:
                await LoadConfig();
                break;
            case 2:
                LoadWorld();
                break;
        }
    }

    public void SetHeadBack()
    {
        Model.SetChoiseContent(_useName, App.Lang("Button.Refash"));
        Model.SetChoiseCall(_useName, Reload);
    }

    public void RemoveHeadBack()
    {
        Model.RemoveChoiseData(_useName);
    }
}
