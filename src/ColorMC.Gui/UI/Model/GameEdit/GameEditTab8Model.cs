using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditModel
{
    public ObservableCollection<ResourcePackModel> ResourcePackList { get; init; } = [];

    private readonly List<ResourcePackModel> _resourceItems = [];

    private ResourcePackModel? _lastResource;

    [ObservableProperty]
    private string _resourceText;
    [ObservableProperty]
    private bool _resourceEmptyDisplay;

    partial void OnResourceTextChanged(string value)
    {
        LoadResource1();
    }

    [RelayCommand]
    public async Task LoadResource()
    {
        Model.Progress(App.Lang("GameEditWindow.Tab8.Info3"));
        _resourceItems.Clear();

        var res = await GameBinding.GetResourcepacks(_obj);
        Model.ProgressClose();
        foreach (var item in res)
        {
            _resourceItems.Add(new(this, item));
        }

        LoadResource1();
    }

    private void LoadResource1()
    {
        ResourcePackList.Clear();
        if (string.IsNullOrWhiteSpace(ResourceText))
        {
            ResourcePackList.AddRange(_resourceItems);
        }
        else
        {
            ResourcePackList.AddRange(_resourceItems.Where(item => item.Local.Contains(ResourceText)));
        }

        ResourceEmptyDisplay = ResourcePackList.Count == 0;
    }

    private void AddResource()
    {
        WindowManager.ShowAdd(_obj, FileType.Resourcepack);
    }

    private async void ImportResource()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.AddFile(top, _obj, FileType.Resourcepack);
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

    private void OpenResource()
    {
        PathBinding.OpenPath(_obj, PathType.ResourcepackPath);
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