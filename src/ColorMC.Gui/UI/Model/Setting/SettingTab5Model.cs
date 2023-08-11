using Avalonia.Controls;
using AvaloniaEdit.Utils;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingTab5Model : BaseModel
{
    [ObservableProperty]
    private string? _name;
    [ObservableProperty]
    private string? _local;

    public ObservableCollection<JavaDisplayObj> JavaList { get; init; } = new();

    public SettingTab5Model(IUserControl con) : base(con)
    {

    }

    [RelayCommand]
    public void ShowAddJava()
    {
        App.ShowAddJava();
    }

    [RelayCommand]
    public void OpenPath()
    {
        PathBinding.OpPath(PathType.JavaPath);
    }

    [RelayCommand]
    public void AddJava()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Local))
        {
            Show(App.GetLanguage("Gui.Error8"));
            return;
        }

        try
        {
            Progress(App.GetLanguage("SettingWindow.Tab5.Info1"));

            var res = JavaBinding.AddJava(Name, Local);
            ProgressClose();
            if (res.Item1 == null)
            {
                Show(res.Item2!);
                return;
            }

            Name = "";
            Local = "";

            Load();
        }
        finally
        {

        }
    }

    [RelayCommand]
    public async Task Select()
    {
        var file = await PathBinding.SelectFile(Window, FileType.Java);

        if (file != null)
        {
            Local = file;
            var info = JavaBinding.GetJavaInfo(file);
            if (info != null)
            {
                Name = info.Type + "_" + info.Version;
            }
        }
    }

    [RelayCommand]
    public void OpenFile()
    {
        var list = JavaBinding.FindJava();
        if (list == null)
        {
            Show(App.GetLanguage("SettingWindow.Tab5.Error1"));
            return;
        }

        list.ForEach(item => JvmPath.AddItem(item.Type + "_" + item.Version, item.Path));
        Load();
        Notify(App.GetLanguage("SettingWindow.Tab5.Info4"));
    }

    [RelayCommand]
    public void Load()
    {
        JavaList.Clear();
        JavaList.AddRange(JavaBinding.GetJavas());
    }

    [RelayCommand]
    public async Task Delete()
    {
        var res = await ShowWait(App.GetLanguage("SettingWindow.Tab5.Info3"));
        if (!res)
            return;

        JavaBinding.RemoveAllJava();
        Load();
    }

    public void Flyout(Control con, IList list)
    {
        _ = new SettingFlyout1(con, this, list);
    }

    public override void Close()
    {
        JavaList.Clear();
    }
}
