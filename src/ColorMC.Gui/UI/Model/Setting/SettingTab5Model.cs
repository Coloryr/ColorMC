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

namespace ColorMC.Gui.UI.Model.Setting;

public partial class SettingTab5Model : ObservableObject
{
    private readonly IUserControl Con;

    [ObservableProperty]
    private string? name;
    [ObservableProperty]
    private string? local;

    public ObservableCollection<JavaDisplayObj> JavaList { get; init; } = new();

    public SettingTab5Model(IUserControl con)
    {
        Con = con;
    }

    [RelayCommand]
    public void ShowAddJava()
    {
        App.ShowAddJava();
    }

    [RelayCommand]
    public void OpenPath()
    {
        BaseBinding.OpenDownloadJavaPath();
    }

    [RelayCommand]
    public void AddJava()
    {
        var window = Con.Window;

        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Local))
        {
            window.OkInfo.Show(App.GetLanguage("Gui.Error8"));
            return;
        }

        try
        {
            window.ProgressInfo.Show(App.GetLanguage("SettingWindow.Tab5.Info1"));

            var res = JavaBinding.AddJava(Name, Local);
            if (res.Item1 == null)
            {
                window.ProgressInfo.Close();
                window.OkInfo.Show(res.Item2!);
                return;
            }

            Name = "";
            Local = "";
            window.ProgressInfo.Close();

            Load();
        }
        finally
        {

        }
    }

    [RelayCommand]
    public async void Select()
    {
        var window = Con.Window;
        var file = await BaseBinding.OpFile(window, FileType.Java);

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
        var window = Con.Window;
        var list = JavaBinding.FindJava();
        if (list == null)
        {
            window.OkInfo.Show(App.GetLanguage("SettingWindow.Tab5.Error1"));
            return;
        }

        list.ForEach(item => JvmPath.AddItem(item.Type + "_" + item.Version, item.Path));
        Load();
        window.NotifyInfo.Show(App.GetLanguage("SettingWindow.Tab5.Info4"));
    }

    [RelayCommand]
    public void Load()
    {
        JavaList.Clear();
        JavaList.AddRange(JavaBinding.GetJavas());
    }

    [RelayCommand]
    public async void Delete()
    {
        var window = Con.Window;
        var res = await window.OkInfo.ShowWait(App.GetLanguage("SettingWindow.Tab5.Info3"));
        if (!res)
            return;

        JavaBinding.RemoveAllJava();
        Load();
    }

    public void Flyout(Control con, IList list)
    {
        _ = new SettingFlyout1(con, this, list);
    }
}
