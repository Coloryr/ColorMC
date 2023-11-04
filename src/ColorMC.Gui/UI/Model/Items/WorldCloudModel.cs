using Avalonia.Media.Imaging;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.GameCloud;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.IO;

namespace ColorMC.Gui.UI.Model.Items;

public partial class WorldCloudModel : ObservableObject
{
    public string Name => HaveLocal ? World.LevelName : Cloud.Name;
    public string Time => HaveLocal ? FuntionUtils.MillisecondsToDataTime(World.LastPlayed).ToString()
        : App.Lang("GameCloudWindow.Tab3.Info3");
    public string Time1 => HaveCloud ? Cloud.Time : App.Lang("GameCloudWindow.Tab3.Info1");
    public string Local => HaveLocal ? World.Local : App.Lang("GameCloudWindow.Tab3.Info2");

    public WorldObj World { get; init; }
    public CloudWorldObj Cloud { get; init; }

    private readonly GameCloudModel _model;

    [ObservableProperty]
    private bool _isSelect;

    [ObservableProperty]
    private Bitmap _pic;

    public readonly bool HaveCloud;
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

    public void Select()
    {
        _model.SetSelectWorld(this);
    }

    public void Close()
    {
        Pic?.Dispose();
    }

    public void Reload()
    {
        OnPropertyChanged(nameof(Time));
        OnPropertyChanged(nameof(Time1));
        OnPropertyChanged(nameof(Local));
    }

    public void Upload()
    {
        _model.UploadWorld(this);
    }

    public void Download()
    {
        _model.DownloadWorld(this);
    }

    public void DeleteCloud()
    {
        _model.DeleteCloud(this);
    }
}
