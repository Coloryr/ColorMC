using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingModel
{
    [ObservableProperty]
    private string? _javaName;
    [ObservableProperty]
    private string? _javaLocal;

    [ObservableProperty]
    private JavaDisplayObj _javaItem;

    [ObservableProperty]
    private bool _javaFinding;

    public ObservableCollection<JavaDisplayObj> JavaList { get; init; } = [];

    private bool _javaLoaded;
    private int _needJava;

    [RelayCommand]
    public void AddJava()
    {
        if (string.IsNullOrWhiteSpace(JavaName) || string.IsNullOrWhiteSpace(JavaLocal))
        {
            Model.Show(App.Lang("SettingWindow.Tab5.Error2"));
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
        if (file.Item1 != null)
        {
            JavaLocal = file.Item1;
            var info = JavaBinding.GetJavaInfo(file.Item1);
            if (info != null)
            {
                JavaName = info.Type + "_" + info.Version;
            }
        }
    }

    private void ShowAddJava()
    {
        WindowManager.ShowAddJava(_needJava);
    }

    private async void AddJavaZip()
    {
        var file = await PathBinding.SelectFile(FileType.JavaZip);
        if (file.Item1 == null || file.Item2 == null)
        {
            return;
        }

        Model.Progress(App.Lang("SettingWindow.Tab5.Info7"));
        string temp = App.Lang("Gui.Info27");
        var res = await JavaBinding.AddJavaZip(file.Item1, file.Item2, (a, b, c) =>
        {
            Dispatcher.UIThread.Post(() => Model.ProgressUpdate($"{temp} {a} {b}/{c}"));
        });
        Model.ProgressClose();
        if (!res.State)
        {
            Model.Show(res.Message!);
        }
        else
        {
            Model.Notify(App.Lang("SettingWindow.Tab5.Info6"));
        }
        LoadJava();
    }

    public async void FindJava()
    {
        if (SystemInfo.Os == OsType.Android)
        {
            return;
        }
        JavaFinding = true;
        Model.Title1 = App.Lang("SettingWindow.Tab5.Info8");
        var list = await JavaBinding.FindJava();
        Model.Title1 = null;
        JavaFinding = false;
        if (list == null)
        {
            Model.Show(App.Lang("SettingWindow.Tab5.Error1"));
            return;
        }

        list.ForEach(item => JvmPath.AddItem(item.Type + "_" + item.Version, item.Path));
        LoadJava();
        Model.Notify(App.Lang("SettingWindow.Tab5.Info4"));
    }

    public void LoadJava()
    {
        JavaList.Clear();
        JavaList.AddRange(JavaBinding.GetJavas());
    }

    private async void DeleteJava()
    {
        var res = await Model.ShowWait(App.Lang("SettingWindow.Tab5.Info3"));
        if (!res)
            return;

        JavaBinding.RemoveAllJava();
        LoadJava();
    }

    private void OpenJavaPath()
    {
        PathBinding.OpPath(PathType.JavaPath);
    }

    public void Load(int mainversion)
    {
        _needJava = mainversion;
        LoadJava();
        if (!_javaLoaded && JavaList.Count == 0)
        {
            _javaLoaded = true;
            FindJava();
        }
    }
}
