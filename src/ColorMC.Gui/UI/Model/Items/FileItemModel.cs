using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Items;

/// <summary>
/// 下载文件项目
/// </summary>
public partial class FileItemModel : SelectItemModel
{
    /// <summary>
    /// 下载
    /// </summary>
    public IAddControl Add { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public Task<Bitmap?> Image => GetImage();
    /// <summary>
    /// 图标
    /// </summary>
    private Bitmap? _img;

    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; init; }
    /// <summary>
    /// 简介
    /// </summary>
    public string Summary { get; init; }
    /// <summary>
    /// 简介
    /// </summary>
    public string SummaryOld { get; init; }
    /// <summary>
    /// 作者
    /// </summary>
    public ObservableCollection<AuthorModel> Authors { get; init; } = [];
    /// <summary>
    /// 标签
    /// </summary>
    public ObservableCollection<TagModel> Tags { get; init; } = [];
    /// <summary>
    /// 截图
    /// </summary>
    public ObservableCollection<WebPicModel> Screenshots { get; init; } = [];
    /// <summary>
    /// 下载次数
    /// </summary>
    public string DownloadCount { get; init; }
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime ModifiedDate { get; init; }

    /// <summary>
    /// 是否为整合包
    /// </summary>
    public bool IsModPack { get; init; }
    /// <summary>
    /// 是否已经下载
    /// </summary>
    public bool HaveDownload { get; init; }
    /// <summary>
    /// 是否显示标星
    /// </summary>
    public bool ShowStar { get; init; }
    /// <summary>
    /// 是否有百科翻译
    /// </summary>
    public bool HaveMcmod { get; init; }

    public string SummaryHtml { get; private set; }
    public string SummaryMarkdown { get; private set; }

    /// <summary>
    /// 星的图标
    /// </summary>
    [ObservableProperty]
    private string _star = ImageManager.Stars[1];
    /// <summary>
    /// 是否标星
    /// </summary>
    [ObservableProperty]
    private bool _isStar;
    /// <summary>
    /// 是否显示星
    /// </summary>
    [ObservableProperty]
    private bool _starVis;

    /// <summary>
    /// 是否已下载
    /// </summary>
    [ObservableProperty]
    private bool _isDownload = false;
    /// <summary>
    /// 是否正在下载
    /// </summary>
    [ObservableProperty]
    private bool _nowDownload = false;
    /// <summary>
    /// 是否鼠标在上面
    /// </summary>
    [ObservableProperty]
    private bool _top;
    /// <summary>
    /// 是否启用按钮
    /// </summary>
    [ObservableProperty]
    private bool _enableButton;
    /// <summary>
    /// 是否启用按钮
    /// </summary>
    [ObservableProperty]
    private bool _isMarkdown;

    /// <summary>
    /// 文件类型
    /// </summary>
    public FileType FileType;
    /// <summary>
    /// 下载源
    /// </summary>
    public SourceType SourceType;
    /// <summary>
    /// 网址
    /// </summary>
    public string Url;
    /// <summary>
    /// Mcmod信息
    /// </summary>
    public McModSearchItemObj? McMod;
    /// <summary>
    /// 图标地址
    /// </summary>
    public string? Logo;
    /// <summary>
    /// 文件ID
    /// </summary>
    public string ID;

    /// <summary>
    /// 项目ID
    /// </summary>
    public string Pid;

    /// <summary>
    /// 是否已经关闭
    /// </summary>
    private bool _close;

    public FileItemModel(CurseForgeListObj.CurseForgeListDataObj data, FileType type, McModSearchItemObj? mcmod)
    {
        HaveMcmod = mcmod != null;

        McMod = mcmod;
        Pid = data.Id.ToString();

        ID = data.Id.ToString();
        Name = mcmod?.McmodName ?? data.Name;
        Summary = mcmod?.McmodText ?? data.Summary;
        SummaryOld = data.Summary;
        foreach (var item in data.Authors)
        {
            if (Authors.Count > 10)
            {
                Authors.Add(new AuthorModel(string.Format(LanguageUtils.Get("App.Text114"), data.Authors.Count - 10), null));
                break;
            }
            Authors.Add(new AuthorModel(item.Name, item.AvatarUrl));
        }
        foreach (var item in data.Categories)
        {
            Tags.Add(new TagModel(item.Name, item.IconUrl));
        }
        foreach (var item in data.Screenshots)
        {
            Screenshots.Add(new WebPicModel(item.Title, item.Description, item.Url));
        }
        DownloadCount = UIUtils.MakeDownload(data.DownloadCount);
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
            IsStar = CollectUtils.IsCollect(SourceType, data.Id.ToString());
        }

        LoadBody();
    }

    public FileItemModel(ModrinthSearchObj.HitObj data, List<ModrinthTeamObj>? team, ModrinthProjectObj? project, FileType type, McModSearchItemObj? mcmod)
    {
        HaveMcmod = mcmod != null;

        McMod = mcmod;
        Pid = data.ProjectId;

        ID = data.ProjectId;
        Name = mcmod?.McmodName ?? data.Title;
        Summary = mcmod?.McmodText ?? data.Description;
        if (project != null)
        {
            SummaryOld = project.Body;
        }
        else
        {
            SummaryOld = data.Description;
        }
        if (team != null)
        {
            foreach (var item in team)
            {
                if (Authors.Count > 10)
                {
                    Authors.Add(new AuthorModel(string.Format(LanguageUtils.Get("App.Text114"), team.Count - 10), null));
                    break;
                }
                Authors.Add(new AuthorModel(item.User.Username, item.User.AvatarUrl));
            }
        }
        else
        {
            Authors.Add(new AuthorModel(data.Author, null));
        }
        DownloadCount = UIUtils.MakeDownload(data.Downloads);
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
            IsStar = CollectUtils.IsCollect(SourceType, data.ProjectId);
        }

        LoadBody();
    }

    public FileItemModel(ModrinthProjectObj data, FileType type, McModSearchItemObj? mcmod)
    {
        McMod = mcmod;
        Pid = data.Id;

        ID = data.Id;
        Name = data.Title;
        Summary = data.Description;
        //Author = data.Author;
        DownloadCount = UIUtils.MakeDownload(data.Downloads);
        ModifiedDate = DateTime.Parse(data.Updated);
        Logo = data.IconUrl;
        FileType = type;
        SourceType = SourceType.Modrinth;
        Url = data.GetUrl(type);

        HaveDownload = true;
        IsModPack = type == FileType.ModPack;

        ShowStar = type is FileType.Mod or FileType.Shaderpack or FileType.Resourcepack;
        if (ShowStar)
        {
            IsStar = CollectUtils.IsCollect(SourceType, data.Id);
        }

        LoadBody();
    }

    public FileItemModel(McModSearchItemObj data, FileType type)
    {
        McMod = data;

        Logo = data.McmodIcon.StartsWith("//") ? "https:" + data.McmodIcon : data.McmodIcon;
        Name = data.McmodName;
        Summary = data.McmodText;
        Authors.Add(new AuthorModel(data.McmodAuthor, null));
        FileType = FileType.Mod;
        SourceType = SourceType.McMod;
        ModifiedDate = data.McmodUpdateTime;

        HaveDownload = data.CurseforgeId != null || data.ModrinthId != null;
        IsModPack = type == FileType.ModPack;

        LoadBody();
    }

    private void LoadBody()
    {
        if (SummaryOld.StartsWith('#'))
        {
            IsMarkdown = true;
            SummaryMarkdown = SummaryOld;
        }
        else 
        {
            SummaryHtml = SummaryOld;
        }
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
    /// 标星
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
    /// 打开网页
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
    /// 获取图标
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
            _img = await ImageManager.Load(Logo, true);
            return _img;
        }
        catch (Exception e)
        {
            Logs.Error(LanguageUtils.Get("AddModPackWindow.Text26"), e);
        }

        return null;
    }

    /// <summary>
    /// 安装
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
    /// 选择
    /// </summary>
    public void SetSelect()
    {
        Add?.SetSelect(this);
    }

    /// <summary>
    /// 上一页
    /// </summary>
    public void Back()
    {
        Add?.Back();
    }

    /// <summary>
    /// 下一页
    /// </summary>
    public void Next()
    {
        Add?.Next();
    }

    /// <summary>
    /// 清理图标
    /// </summary>
    public void Close()
    {
        _close = true;
        foreach (var item in Authors)
        {
            item.Close();
        }
        foreach (var item in Tags)
        {
            item.Close();
        }
        foreach (var item in Screenshots)
        {
            item.Close();
        }
        if (_img != ImageManager.GameIcon)
        {
            _img?.Dispose();
            _img = null;
            OnPropertyChanged(nameof(Image));
        }
    }
}
