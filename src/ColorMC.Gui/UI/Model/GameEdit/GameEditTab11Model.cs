using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System;
using ColorMC.Core.Game;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Input;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class GameEditTab11Model : GameEditTabModel
{
    public ObservableCollection<ShaderpackDisplayObj> ShaderpackList { get; init; } = new();

    [ObservableProperty]
    private ShaderpackDisplayObj? item;

    public GameEditTab11Model(IUserControl con, GameSettingObj obj) : base(con, obj)
    {

    }

    [RelayCommand]
    public void Open()
    {
        BaseBinding.OpPath(Obj, PathType.ShaderpacksPath);
    }
    [RelayCommand]
    public void Load()
    {
        var window = Con.Window;
        window.ProgressInfo.Show(App.GetLanguage("GameEditWindow.Tab10.Info4"));
        ShaderpackList.Clear();
        ShaderpackList.AddRange(GameBinding.GetShaderpacks(Obj));
        window.ProgressInfo.Close();
    }
    [RelayCommand]
    public async void Add()
    {
        var window = Con.Window;
        var res = await GameBinding.AddFile(window as Window, Obj, FileType.Shaderpack);
        if (res == null)
            return;

        if (res == false)
        {
            window.NotifyInfo.Show(App.GetLanguage("Gui.Error12"));
            return;
        }

        window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab11.Info3"));
        Load();
    }

    public async void Drop(IDataObject data)
    {
        var res = await GameBinding.AddFile(Obj, data, FileType.Shaderpack);
        if (res)
        {
            Load();
        }
    }

    public void Delete(ShaderpackDisplayObj obj)
    {
        var window = Con.Window;
        obj.Shaderpack.Delete();
        window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab10.Info5"));
        Load();
    }
}
