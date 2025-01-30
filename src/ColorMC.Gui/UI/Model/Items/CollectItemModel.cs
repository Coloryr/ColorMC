using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ColorMC.Core.Helpers;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Items;

public partial class CollectItemModel(CollectItemObj obj) : SelectItemModel
{
    public const int DefaultWidth = 350;

    public ICollectWindow Add { get; set; }

    public string Name => Obj.Name;
    public string Type => Obj.FileType.GetName();
    public string Source => Obj.Source.GetName();

    public Task<Bitmap?> Image => GetImage();

    private Bitmap? _img;

    [ObservableProperty]
    private bool _isCheck;
    [ObservableProperty]
    private double _width = DefaultWidth;

    public readonly CollectItemObj Obj = obj;

    [RelayCommand]
    public void OpenWeb()
    {
        BaseBinding.OpenUrl(Obj.Url);
    }

    [RelayCommand]
    public void Install()
    {
        Add?.Install(this);
    }

    private async Task<Bitmap?> GetImage()
    {
        if (_img != null || Obj.Icon == null)
        {
            return _img;
        }
        try
        {
            await Task.Run(() =>
            {
                _img = ImageManager.Load(Obj.Icon, true).Result;
            });
            return _img;
        }
        catch (Exception e)
        {
            Logs.Error(App.Lang("AddModPackWindow.Error5"), e);
        }

        return null;
    }

    public void SetMin(bool min)
    {
        if (min)
        {
            Width = double.NaN;
        }
        else
        {
            Width = DefaultWidth;
        }
    }

    public void Close()
    {
        _img?.Dispose();
    }

    public void SetSelect()
    {
        Add?.SetSelect(this);
    }
}
