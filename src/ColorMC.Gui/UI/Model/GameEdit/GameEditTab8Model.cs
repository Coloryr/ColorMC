using Avalonia.Input;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditTab8Model : GameEditModel, ILoadFuntion<ResourcePackModel>
{
    public ObservableCollection<ResourcePackModel> ResourcePackList { get; init; } = new();

    private ResourcePackModel? _last;

    public GameEditTab8Model(IUserControl con, GameSettingObj obj) : base(con, obj)
    {

    }

    [RelayCommand]
    public void Add()
    {
        App.ShowAdd(Obj, FileType.Resourcepack);
    }

    [RelayCommand]
    public async Task Import()
    {
        var file = await PathBinding.AddFile(Window, Obj, FileType.Resourcepack);
        if (file == null)
            return;

        if (file == false)
        {
            Notify(App.GetLanguage("GameEditWindow.Tab8.Error1"));
            return;
        }

        Notify(App.GetLanguage("GameEditWindow.Tab4.Info2"));
        await Load();
    }

    [RelayCommand]
    public void Open()
    {
        PathBinding.OpPath(Obj, PathType.ResourcepackPath);
    }

    [RelayCommand]
    public async Task Load()
    {
        Progress(App.GetLanguage("GameEditWindow.Tab8.Info3"));
        ResourcePackList.Clear();

        var res = await GameBinding.GetResourcepacks(Obj);
        ProgressClose();
        foreach (var item in res)
        {
            ResourcePackList.Add(new(Control, this, item));
        }
    }

    public async void Drop(IDataObject data)
    {
        var res = await GameBinding.AddFile(Obj, data, FileType.Resourcepack);
        if (res)
        {
            await Load();
        }
    }

    public void SetSelect(ResourcePackModel item)
    {
        if (_last != null)
        {
            _last.IsSelect = false;
        }
        _last = item;
        _last.IsSelect = true;
    }
}