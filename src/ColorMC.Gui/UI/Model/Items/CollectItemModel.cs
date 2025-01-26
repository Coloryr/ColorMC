using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using ColorMC.Core.Helpers;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Items;

public partial class CollectItemModel(CollectItemObj obj) : SelectItemModel
{
    public string Name => Obj.Name;
    public string Type => Obj.FileType.GetName();
    public string Source => Obj.Source.GetName();

    public Task<Bitmap?> Image => GetImage();

    private Bitmap? _img;

    public readonly CollectItemObj Obj = obj;

    public void Close()
    {
        _img?.Dispose();
    }

    [RelayCommand]
    public void OpenWeb()
    { 
    
    }

    [RelayCommand]
    public void Install()
    { 
        
    }

    private async Task<Bitmap?> GetImage()
    {
        if (_img != null)
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
}
