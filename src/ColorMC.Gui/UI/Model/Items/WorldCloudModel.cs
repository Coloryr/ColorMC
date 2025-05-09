using System;
using System.IO;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs.ColorMC;
using ColorMC.Gui.UI.Model.GameCloud;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 云存档存档项目
/// </summary>
public partial class WorldCloudModel : SelectItemModel
{
    /// <summary>
    /// 名字
    /// </summary>
    public string Name => HaveLocal ? World.LevelName : Cloud.Name;
    /// <summary>
    /// 本地时间
    /// </summary>
    public string Time => HaveLocal ? FuntionUtils.MillisecondsToDataTime(World.LastPlayed).ToString()
        : App.Lang("GameCloudWindow.Tab3.Info3");
    /// <summary>
    /// 云存档时间
    /// </summary>
    public string Time1 => HaveCloud ? Cloud.Time : App.Lang("GameCloudWindow.Tab3.Info1");
    /// <summary>
    /// 本地位置
    /// </summary>
    public string Local => HaveLocal ? World.Local : App.Lang("GameCloudWindow.Tab3.Info2");

    /// <summary>
    /// 本地游戏存档
    /// </summary>
    public WorldObj World { get; init; }
    /// <summary>
    /// 云存档
    /// </summary>
    public CloudWorldObj Cloud { get; init; }

    /// <summary>
    /// 云同步
    /// </summary>
    private readonly GameCloudModel _model;

    /// <summary>
    /// 存档图片
    /// </summary>
    [ObservableProperty]
    private Bitmap _pic;

    /// <summary>
    /// 是否有云端存档
    /// </summary>
    public readonly bool HaveCloud;
    /// <summary>
    /// 是否有本地存档
    /// </summary>
    public readonly bool HaveLocal;

    public WorldCloudModel(GameCloudModel model, CloudWorldObj cloud, WorldObj world)
    {
        _model = model;
        World = world;
        Cloud = cloud;

        if (world.Icon != null && File.Exists(world.Icon))
        {
            _pic = new(world.Icon);
        }

        HaveCloud = true;
        HaveLocal = true;
    }

    public WorldCloudModel(GameCloudModel model, WorldObj world)
    {
        _model = model;
        World = world;

        if (world.Icon != null && File.Exists(world.Icon))
        {
            _pic = new(world.Icon);
        }

        HaveCloud = false;
        HaveLocal = true;
    }

    public WorldCloudModel(GameCloudModel model, CloudWorldObj cloud)
    {
        _model = model;
        Cloud = cloud;

        var data = Convert.FromBase64String(cloud.Icon);
        using var stream = new MemoryStream(data);
        stream.Seek(0, SeekOrigin.Begin);
        _pic = new(stream);

        HaveCloud = true;
        HaveLocal = false;
    }

    /// <summary>
    /// 选中
    /// </summary>
    public void Select()
    {
        _model.SetSelectWorld(this);
    }

    /// <summary>
    /// 清理图片
    /// </summary>
    public void Close()
    {
        Pic?.Dispose();
    }

    /// <summary>
    /// 重载数据
    /// </summary>
    public void Reload()
    {
        OnPropertyChanged(nameof(Time));
        OnPropertyChanged(nameof(Time1));
        OnPropertyChanged(nameof(Local));
    }

    /// <summary>
    /// 更新存档
    /// </summary>
    public void Upload()
    {
        _model.UploadWorld(this);
    }

    /// <summary>
    /// 下载存档
    /// </summary>
    public void Download()
    {
        _model.DownloadWorld(this);
    }

    /// <summary>
    /// 删除云端存档
    /// </summary>
    public void DeleteCloud()
    {
        _model.DeleteCloud(this);
    }
}
