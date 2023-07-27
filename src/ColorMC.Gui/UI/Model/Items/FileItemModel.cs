using Avalonia.Media.Imaging;
using ColorMC.Core.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Add;

public partial class FileItemModel : ObservableObject
{
    private readonly IAddWindow _add;

    public Task<Bitmap?> Image => GetImage();

    public FileItemDisplayObj Data { get; init; }
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

    public FileItemModel(FileItemDisplayObj data, IAddWindow add)
    {
        Data = data;
        _add = add;

        if (data == null)
            return;
        isDownload = data.IsDownload;
    }


    private async Task<Bitmap?> GetImage()
    {
        if (Data?.Logo == null)
            return null;
        try
        {
            return await ImageUtils.Load(Data.Logo);
        }
        catch (Exception e)
        {
            Logs.Error(App.GetLanguage("AddModPackWindow.Error5"), e);
        }

        return null;
    }

    public void Install()
    {
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
}
