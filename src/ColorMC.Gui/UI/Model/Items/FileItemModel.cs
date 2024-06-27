using System;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.McMod;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using ColorMC.Gui.Utils.LaunchSetting;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Items;

public partial class FileItemModel : ObservableObject
{
    private readonly IAddWindow _add;

    private static readonly BoxShadows shadows = new(BoxShadow.Parse("0 0 3 1 #1A000000"), [BoxShadow.Parse("0 0 5 -1 #1A000000")]);

    public Task<Bitmap?> Image => GetImage();

    private Bitmap? _img;

    public FileItemObj Data { get; init; }
    public string? Name => Data.Name;
    public string? Summary => Data.Summary;
    public string? Author => Data.Author;
    public long? DownloadCount => Data.DownloadCount;
    public DateTime? ModifiedDate => DateTime.Parse(Data.ModifiedDate);

    [ObservableProperty]
    private bool isDownload = false;
    [ObservableProperty]
    private bool _nowDownload = false;
    [ObservableProperty]
    private bool _isSelect;
    [ObservableProperty]
    private bool _top;
    [ObservableProperty]
    private bool _enableButton;
    [ObservableProperty]
    private bool _haveDownload;
    [ObservableProperty]
    private BoxShadows _border = shadows;

    public FileItemModel(FileItemObj data, IAddWindow add)
    {
        Data = data;
        _add = add;

        if (data == null)
        {
            return;
        }
        isDownload = data.IsDownload;
        if (data.SourceType == SourceType.McMod)
        {
            var obj1 = (data.Data as McModSearchItemObj)!;
            _haveDownload = obj1.curseforge_id != null || obj1.modrinth_id != null;
        }
        else
        {
            _haveDownload = true;
        }
    }

    partial void OnIsSelectChanged(bool value)
    {
        EnableButton = Top || IsSelect;
        if (IsSelect)
        {
            var color = ColorSel.MainColor.ToColor();
            var color1 = new Color(255, color.R, color.G, color.B);
            var box = BoxShadow.Parse("0 0 3 1 #1A000000");
            box.Color = color1;
            Border = new BoxShadows(box);
        }
        else
        {
            Border = shadows;
        }
    }

    partial void OnTopChanged(bool value)
    {
        EnableButton = Top || IsSelect;
    }

    [RelayCommand]
    public void OpenWeb()
    {
        var url = Data.SourceType == SourceType.McMod ? Data.GetMcMod() : Data.GetUrl();
        if (url != null)
        {
            BaseBinding.OpUrl(url);
        }
    }

    private async Task<Bitmap?> GetImage()
    {
        if (_img != null)
        {
            return _img;
        }
        if (Data?.Logo == null)
        {
            return null;
        }
        try
        {
            await Task.Run(() =>
            {
                _img = ImageUtils.Load(Data.Logo).Result;
            });
            return _img;
        }
        catch (Exception e)
        {
            Logs.Error(App.Lang("AddModPackWindow.Error5"), e);
        }

        return null;
    }

    public void Install()
    {
        if (!HaveDownload)
        {
            return;
        }
        _add.Install(this);
    }

    public void SetSelect()
    {
        _add.SetSelect(this);
    }

    public void Back()
    {
        _add.Back();
    }

    public void Next()
    {
        _add.Next();
    }

    public async void Close()
    {
        if (await GetImage() != ImageManager.GameIcon)
        {
            _img?.Dispose();
        }
    }
}
