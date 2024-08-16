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
using ColorMC.Gui.Utils;

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

    public FileItemModel(CurseForgeObjList.Data data, FileType type)
    {
        Data = data;

        ID = data.id.ToString();
        Name = data.name;
        Summary = data.summary;
        Author = data.authors.GetString();
        DownloadCount = data.downloadCount;
        ModifiedDate = DateTime.Parse(data.dateModified);
        Logo = data.logo?.url;
        FileType = type;
        SourceType = SourceType.CurseForge;
        Url = data.GetUrl();

        HaveDownload = true;
        IsModPack = type == FileType.ModPack;
    }

    public FileItemModel(ModrinthSearchObj.Hit data, FileType type)
    {
        Data = data;

        ID = data.project_id;
        Name = data.title;
        Summary = data.description;
        Author = data.author;
        DownloadCount = data.downloads;
        ModifiedDate = DateTime.Parse(data.date_modified);
        Logo = data.icon_url;
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

        Logo = data.mcmod_icon.StartsWith("//")
                    ? "https:" + data.mcmod_icon : data.mcmod_icon;
        Name = data.mcmod_name;
        Summary = data.mcmod_text;
        Author = data.mcmod_author;
        FileType = FileType.Mod;
        SourceType = SourceType.McMod;
        ModifiedDate = data.mcmod_update_time;

        HaveDownload = data.curseforge_id != null || data.modrinth_id != null;
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
                _img = ImageUtils.Load(Logo, true).Result;
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

    public async void Close()
    {
        if (await GetImage() != ImageManager.GameIcon)
        {
            _img?.Dispose();
        }
    }
}
