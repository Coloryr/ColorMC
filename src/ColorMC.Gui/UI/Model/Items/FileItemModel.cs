using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.ColorMC;
using ColorMC.Core.Objs.CurseForge;
using ColorMC.Core.Objs.Modrinth;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// �����ļ���Ŀ
/// </summary>
public partial class FileItemModel : SelectItemModel
{
    /// <summary>
    /// ����
    /// </summary>
    public IAddControl Add { get; set; }

    /// <summary>
    /// ͼ��
    /// </summary>
    public Task<Bitmap?> Image => GetImage();
    /// <summary>
    /// ͼ��
    /// </summary>
    private Bitmap? _img;

    /// <summary>
    /// ����
    /// </summary>
    public string Name { get; init; }
    /// <summary>
    /// ���
    /// </summary>
    public string Summary { get; init; }
    /// <summary>
    /// ����
    /// </summary>
    public string Author { get; init; }
    /// <summary>
    /// ���ش���
    /// </summary>
    public long DownloadCount { get; init; }
    /// <summary>
    /// ����ʱ��
    /// </summary>
    public DateTime ModifiedDate { get; init; }

    /// <summary>
    /// �Ƿ�Ϊ���ϰ�
    /// </summary>
    public bool IsModPack { get; init; }
    /// <summary>
    /// �Ƿ��Ѿ�����
    /// </summary>
    public bool HaveDownload { get; init; }
    /// <summary>
    /// �Ƿ���ʾ����
    /// </summary>
    public bool ShowStar { get; init; }

    /// <summary>
    /// �ǵ�ͼ��
    /// </summary>
    [ObservableProperty]
    private string _star = ImageManager.Stars[1];
    /// <summary>
    /// �Ƿ����
    /// </summary>
    [ObservableProperty]
    private bool _isStar;
    /// <summary>
    /// �Ƿ���ʾ��
    /// </summary>
    [ObservableProperty]
    private bool _starVis;

    /// <summary>
    /// �Ƿ�������
    /// </summary>
    [ObservableProperty]
    private bool _isDownload = false;
    /// <summary>
    /// �Ƿ���������
    /// </summary>
    [ObservableProperty]
    private bool _nowDownload = false;
    /// <summary>
    /// �Ƿ����������
    /// </summary>
    [ObservableProperty]
    private bool _top;
    /// <summary>
    /// �Ƿ����ð�ť
    /// </summary>
    [ObservableProperty]
    private bool _enableButton;

    /// <summary>
    /// �ļ�����
    /// </summary>
    public FileType FileType;
    /// <summary>
    /// ����Դ
    /// </summary>
    public SourceType SourceType;
    /// <summary>
    /// ��ַ
    /// </summary>
    public string Url;
    /// <summary>
    /// Mcmod��Ϣ
    /// </summary>
    public McModSearchItemObj? McMod;
    /// <summary>
    /// ͼ���ַ
    /// </summary>
    public string? Logo;
    /// <summary>
    /// �ļ�ID
    /// </summary>
    public string ID;

    /// <summary>
    /// ����
    /// </summary>
    public object Data;

    /// <summary>
    /// �Ƿ��Ѿ��ر�
    /// </summary>
    private bool _close;

    public FileItemModel(CurseForgeListObj.DataObj data, FileType type, McModSearchItemObj? mcmod)
    {
        McMod = mcmod;
        Data = data;

        ID = data.Id.ToString();
        Name = mcmod?.McmodName ?? data.Name;
        Summary = mcmod?.McmodText ?? data.Summary;
        Author = data.Authors.GetString();
        DownloadCount = data.DownloadCount;
        ModifiedDate = DateTime.Parse(data.DateModified);
        Logo = data.Logo?.Url;
        FileType = type;
        SourceType = SourceType.CurseForge;
        Url = data.GetUrl();

        HaveDownload = true;
        IsModPack = type == FileType.ModPack;

        ShowStar = type is FileType.Mod or FileType.Shaderpack or FileType.Resourcepack;
        if (ShowStar)
        {
            IsStar = BaseBinding.IsStar(SourceType, data.Id.ToString());
        }
    }

    public FileItemModel(ModrinthSearchObj.HitObj data, FileType type, McModSearchItemObj? mcmod)
    {
        McMod = mcmod;
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

        ShowStar = type is FileType.Mod or FileType.Shaderpack or FileType.Resourcepack;
        if (ShowStar)
        {
            IsStar = BaseBinding.IsStar(SourceType, data.ProjectId);
        }
    }

    public FileItemModel(McModSearchItemObj data, FileType type)
    {
        Data = data;
        McMod = data;

        Logo = data.McmodIcon.StartsWith("//") ? "https:" + data.McmodIcon : data.McmodIcon;
        Name = data.McmodName;
        Summary = data.McmodText;
        Author = data.McmodAuthor;
        FileType = FileType.Mod;
        SourceType = SourceType.McMod;
        ModifiedDate = data.McmodUpdateTime;

        HaveDownload = data.CurseforgeId != null || data.ModrinthId != null;
        IsModPack = type == FileType.ModPack;
    }

    partial void OnIsStarChanged(bool value)
    {
        Star = ImageManager.Stars[value ? 0 : 1];
        StarVis = (value || Top);
    }

    protected override void IsSelectChanged(bool value)
    {
        EnableButton = Top || IsSelect;
    }

    partial void OnTopChanged(bool value)
    {
        if (ShowStar)
        {
            StarVis = (value || IsStar);
        }

        EnableButton = Top || IsSelect;
    }

    /// <summary>
    /// ����
    /// </summary>
    [RelayCommand]
    public void DoStar()
    {
        if (!ShowStar)
        {
            return;
        }
        IsStar = BaseBinding.SetStart(this);
    }

    /// <summary>
    /// ����ҳ
    /// </summary>
    [RelayCommand]
    public void OpenWeb()
    {
        if (Url != null)
        {
            BaseBinding.OpenUrl(Url);
        }
    }

    /// <summary>
    /// ��ȡͼ��
    /// </summary>
    /// <returns></returns>
    private async Task<Bitmap?> GetImage()
    {
        if (_close || Logo == null)
        {
            return null;
        }
        if (_img != null)
        {
            return _img;
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

    /// <summary>
    /// ��װ
    /// </summary>
    public void Install()
    {
        if (!HaveDownload)
        {
            return;
        }
        Add?.Install(this);
    }

    /// <summary>
    /// ѡ��
    /// </summary>
    public void SetSelect()
    {
        Add?.SetSelect(this);
    }

    /// <summary>
    /// ��һҳ
    /// </summary>
    public void Back()
    {
        Add?.Back();
    }

    /// <summary>
    /// ��һҳ
    /// </summary>
    public void Next()
    {
        Add?.Next();
    }

    /// <summary>
    /// ����ͼ��
    /// </summary>
    public void Close()
    {
        _close = true;
        if (_img != ImageManager.GameIcon)
        {
            _img?.Dispose();
            _img = null;
            OnPropertyChanged(nameof(Image));
        }
    }
}
