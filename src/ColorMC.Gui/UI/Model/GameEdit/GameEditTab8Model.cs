using Avalonia.Input;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditModel : MenuModel
{
    public ObservableCollection<ResourcePackModel> ResourcePackList { get; init; } = new();

    private ResourcePackModel? _lastResource;

    [RelayCommand]
    public void AddResource()
    {
        App.ShowAdd(_obj, FileType.Resourcepack);
    }

    [RelayCommand]
    public async Task ImportResource()
    {
        var file = await PathBinding.AddFile(_obj, FileType.Resourcepack);
        if (file == null)
            return;

        if (file == false)
        {
            Model.Notify(App.GetLanguage("GameEditWindow.Tab8.Error1"));
            return;
        }

        Model.Notify(App.GetLanguage("GameEditWindow.Tab4.Info2"));
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
        Model.Progress(App.GetLanguage("GameEditWindow.Tab8.Info3"));
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
            string.Format(App.GetLanguage("GameEditWindow.Tab8.Info1"), obj.Local));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteResourcepack(obj);
        Model.Notify(App.GetLanguage("GameEditWindow.Tab4.Info3"));
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