using Avalonia.Controls;
using Avalonia.Input;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditTab8Model : GameEditTabModel, IResourcePackFuntion
{
    public ObservableCollection<ResourcePackModel> ResourcePackList { get; init; } = new();

    private ResourcePackModel? Last;

    public GameEditTab8Model(IUserControl con, GameSettingObj obj) : base(con, obj)
    {

    }

    [RelayCommand]
    public void Add()
    {
        App.ShowAdd(Obj, FileType.Resourcepack);
    }

    [RelayCommand]
    public async void Import()
    {
        var window = Con.Window;
        var file = await GameBinding.AddFile(window as TopLevel, Obj, FileType.Resourcepack);
        if (file == null)
            return;

        if (file == false)
        {
            window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab8.Error1"));
            return;
        }

        window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab4.Info2"));
        Load();
    }

    [RelayCommand]
    public void Open()
    {
        BaseBinding.OpPath(Obj, PathType.ResourcepackPath);
    }

    [RelayCommand]
    public async void Load()
    {
        var window = Con.Window;
        window.ProgressInfo.Show(App.GetLanguage("GameEditWindow.Tab8.Info3"));
        ResourcePackList.Clear();

        var res = await GameBinding.GetResourcepacks(Obj);
        window.ProgressInfo.Close();
        foreach (var item in res)
        {
            ResourcePackList.Add(new(Con, this, item));
        }
    }

    public async void Drop(IDataObject data)
    {
        var res = await GameBinding.AddFile(Obj, data, FileType.Resourcepack);
        if (res)
        {
            Load();
        }
    }

    public void SetSelect(ResourcePackModel item)
    {
        if (Last != null)
        {
            Last.IsSelect = false;
        }
        Last = item;
        Last.IsSelect = true;
    }
}