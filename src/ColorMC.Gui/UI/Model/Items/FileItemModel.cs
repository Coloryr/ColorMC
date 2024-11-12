using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.McMod;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Items;

public partial class FileItemModel : SelectItemModel
{
    public IAddWindow Add { get; set; }

    public Task<Bitmap?> Image => GetImage();

    private Bitmap? _img;

    public string? Name { get; init; }
    public string? Summary { get; init; }
    public string? Author { get; init; }
    public long? DownloadCount { get; init; }
    public DateTime? ModifiedDate { get; init; }
    public bool IsModPack { get; init; }
    public bool HaveDownload { get; init; }

    [ObservableProperty]
    private bool _isDownload = false;
    [ObservableProperty]
    private bool _nowDownload = false;
    [ObservableProperty]
    private bool _top;
    [ObservableProperty]
    private bool _enableButton;

    public FileType FileType;
    public SourceType SourceType;
    public string Url;
    public McModSearchItemObj? McMod;
    public string? Logo;
    public string ID;

    /// <summary>
    /// Êý¾Ý
    /// </summary>
    public object Data;

    private bool close;

    public FileItemModel(CurseForgeObjList.DataObj data, FileType type)
    {
        Data = data;

        ID = data.Id.ToString();
        Name = data.Name;
        Summary = data.Summary;
        Author = data.Authors.GetString();
        DownloadCount = data.DownloadCount;
        ModifiedDate = DateTime.Parse(data.DateModified);
        Logo = data.Logo?.Url;
        FileType = type;
        SourceType = SourceType.CurseForge;
        Url = data.GetUrl();

        HaveDownload = true;
        IsModPack = type == FileType.ModPack;
    }

    public FileItemModel(ModrinthSearchObj.HitObj data, FileType type)
    {
        Data = data;

        ID = data.ProjectId;
        Name = data.Title;
        Summary = data.Description;
        Author = data.Author;
        DownloadCount = data.Downloads;
        ModifiedDate = DateTime.Parse(data.DateModified);
        Logo = data.IconUrl;
        FileType = type;
        SourceType = SourceType.Modrinth;
        Url = data.GetUrl(type);

        HaveDownload = true;
        IsModPack = type == FileType.ModPack;
    }

    public FileItemModel(McModSearchItemObj data, FileType type)
    {
        Data = data;
        McMod = data;

        Logo = data.McmodIcon.StartsWith("//")
                    ? "https:" + data.McmodIcon : data.McmodIcon;
        Name = data.McmodName;
        Summary = data.McmodText;
        Author = data.McmodAuthor;
        FileType = FileType.Mod;
        SourceType = SourceType.McMod;
        ModifiedDate = data.McmodUpdateTime;

        HaveDownload = data.CurseforgeId != null || data.ModrinthId != null;
        IsModPack = type == FileType.ModPack;
    }

    protected override void IsSelectChanged(bool value)
    {
        EnableButton = Top || IsSelect;
    }

    partial void OnTopChanged(bool value)
    {
        EnableButton = Top || IsSelect;
    }

    [RelayCommand]
    public void OpenWeb()
    {
        if (Url != null)
        {
            BaseBinding.OpUrl(Url);
        }
    }

    private async Task<Bitmap?> GetImage()
    {
        if (close)
        {
            return null;
        }
        if (_img != null)
        {
            return _img;
        }
        if (Logo == null)
        {
            return null;
        }
        try
        {
            await Task.Run(() =>
            {
                _img = ImageManager.Load(Logo, true).Result;
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
        Add?.Install(this);
    }

    public void SetSelect()
    {
        Add?.SetSelect(this);
    }

    public void Back()
    {
        Add?.Back();
    }

    public void Next()
    {
        Add?.Next();
    }

    public void Close()
    {
        close = true;
        if (_img != ImageManager.GameIcon)
        {
            _img?.Dispose();
            _img = null;
            OnPropertyChanged(nameof(Image));
        }
    }
}
