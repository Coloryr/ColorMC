using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditTab8Model : GameEditTabModel, ILoadFuntion<ResourcePackModel>
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
        var window = _con.Window;
        var file = await GameBinding.AddFile(window as TopLevel, Obj, FileType.Resourcepack);
        if (file == null)
            return;

        if (file == false)
        {
            window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab8.Error1"));
            return;
        }

        window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab4.Info2"));
        await Load();
    }

    [RelayCommand]
    public void Open()
    {
        BaseBinding.OpPath(Obj, PathType.ResourcepackPath);
    }

    [RelayCommand]
    public async Task Load()
    {
        var window = _con.Window;
        window.ProgressInfo.Show(App.GetLanguage("GameEditWindow.Tab8.Info3"));
        ResourcePackList.Clear();

        var res = await GameBinding.GetResourcepacks(Obj);
        window.ProgressInfo.Close();
        foreach (var item in res)
        {
            ResourcePackList.Add(new(_con, this, item));
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