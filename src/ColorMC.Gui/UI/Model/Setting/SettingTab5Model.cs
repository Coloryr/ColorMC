using Avalonia.Threading;
using AvaloniaEdit.Utils;
using ColorMC.Core;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingModel
{
    [ObservableProperty]
    private string? _javaName;
    [ObservableProperty]
    private string? _javaLocal;

    [ObservableProperty]
    private JavaDisplayObj _javaItem;

    public ObservableCollection<JavaDisplayObj> JavaList { get; init; } = [];

    [RelayCommand]
    public async Task AddJavaZip()
    {
        var file = await PathBinding.SelectFile(FileType.JavaZip);
        if (file == null)
        {
            return;
        }

        Model.Progress(App.Lang("SettingWindow.Tab5.Info7"));
        string temp = App.Lang("Gui.Info27");
        ColorMCCore.UnZipItem = (a, b, c) =>
        {
            Dispatcher.UIThread.Post(() => Model.ProgressUpdate($"{temp} {a} {b}/{c}"));
        };
        var res = await JavaBinding.AddJavaZip(file);
        ColorMCCore.UnZipItem = null;
        Model.ProgressClose();
        if (!res.Item1)
        {
            Model.Show(res.Item2!);
        }
        else
        {
            Model.Notify(App.Lang("SettingWindow.Tab5.Info6"));
        }
        LoadJava();
    }

    [RelayCommand]
    public void ShowAddJava()
    {
        App.ShowAddJava();
    }

    [RelayCommand]
    public void OpenJavaPath()
    {
        PathBinding.OpPath(PathType.JavaPath);
    }

    [RelayCommand]
    public void AddJava()
    {
        if (string.IsNullOrWhiteSpace(JavaName) || string.IsNullOrWhiteSpace(JavaLocal))
        {
            Model.Show(App.Lang("Gui.Error8"));
            return;
        }

        Model.Progress(App.Lang("SettingWindow.Tab5.Info1"));

        var res = JavaBinding.AddJava(JavaName, JavaLocal);
        Model.ProgressClose();
        if (res.Item1 == null)
        {
            Model.Show(res.Item2!);
            return;
        }

        JavaName = "";
        JavaLocal = "";

        LoadJava();
    }

    [RelayCommand]
    public async Task SelectJava()
    {
        var file = await PathBinding.SelectFile(FileType.Java);

        if (file != null)
        {
            JavaLocal = file;
            var info = JavaBinding.GetJavaInfo(file);
            if (info != null)
            {
                JavaName = info.Type + "_" + info.Version;
            }
        }
    }

    [RelayCommand]
    public void OpenJavaFile()
    {
        var list = JavaBinding.FindJava();
        if (list == null)
        {
            Model.Show(App.Lang("SettingWindow.Tab5.Error1"));
            return;
        }

        list.ForEach(item => JvmPath.AddItem(item.Type + "_" + item.Version, item.Path));
        LoadJava();
        Model.Notify(App.Lang("SettingWindow.Tab5.Info4"));
    }

    [RelayCommand]
    public void LoadJava()
    {
        JavaList.Clear();
        JavaList.AddRange(JavaBinding.GetJavas());
    }

    [RelayCommand]
    public async Task DeleteJava()
    {
        var res = await Model.ShowWait(App.Lang("SettingWindow.Tab5.Info3"));
        if (!res)
            return;

        JavaBinding.RemoveAllJava();
        LoadJava();
    }
}
