using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Input;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditModel
{
    public ObservableCollection<ResourcePackModel> ResourcePackList { get; init; } = [];

    private ResourcePackModel? _lastResource;

    [RelayCommand]
    public void AddResource()
    {
        WindowManager.ShowAdd(_obj, FileType.Resourcepack);
    }

    [RelayCommand]
    public async Task ImportResource()
    {
        var file = await PathBinding.AddFile(_obj, FileType.Resourcepack);
        if (file == null)
            return;

        if (file == false)
        {
            Model.Notify(App.Lang("GameEditWindow.Tab8.Error1"));
            return;
        }

        Model.Notify(App.Lang("GameEditWindow.Tab4.Info2"));
        await LoadResource();
    }

    [RelayCommand]
    public void OpenResource()
    {
        PathBinding.OpPath(_obj, PathType.ResourcepackPath);
    }

    [RelayCommand]
    public async Task LoadResource()
    {
        Model.Progress(App.Lang("GameEditWindow.Tab8.Info3"));
        ResourcePackList.Clear();

        var res = await GameBinding.GetResourcepacks(_obj);
        Model.ProgressClose();
        foreach (var item in res)
        {
            ResourcePackList.Add(new(this, item));
        }
    }

    public async void DeleteResource(ResourcepackObj obj)
    {
        var res = await Model.ShowWait(
            string.Format(App.Lang("GameEditWindow.Tab8.Info1"), obj.Local));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteResourcepack(obj);
        Model.Notify(App.Lang("GameEditWindow.Tab4.Info3"));
        await LoadResource();
    }

    public async void DropResource(IDataObject data)
    {
        var res = await GameBinding.AddFile(_obj, data, FileType.Resourcepack);
        if (res)
        {
            await LoadResource();
        }
    }

    public void SetSelectResource(ResourcePackModel item)
    {
        if (_lastResource != null)
        {
            _lastResource.IsSelect = false;
        }
        _lastResource = item;
        _lastResource.IsSelect = true;
    }
}