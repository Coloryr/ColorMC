using Avalonia.Threading;
using AvaloniaEdit.Utils;
using ColorMC.Core;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddJavaModel : ObservableObject
{
    private IUserControl Con;
    private readonly List<JavaDownloadDisplayObj> List1 = new();
    public ObservableCollection<JavaDownloadDisplayObj> JavaList { get; init; } = new();
    public ObservableCollection<string> SystemList { get; init; } = new();
    public ObservableCollection<string> VersionList { get; init; } = new();
    public ObservableCollection<string> ArchList { get; init; } = new();
    public List<string> JavaTypeList => JavaBinding.GetJavaType();

    [ObservableProperty]
    private string javaType;
    [ObservableProperty]
    private string system;
    [ObservableProperty]
    private string version;
    [ObservableProperty]
    private string arch;
    [ObservableProperty]
    private int typeIndex = -1;
    [ObservableProperty]
    private int systemIndex = -1;
    [ObservableProperty]
    private int versionIndex = -1;
    [ObservableProperty]
    private int archIndex = -1;
    [ObservableProperty]
    private bool display = true;

    private bool load = true;


    public AddJavaModel(IUserControl con)
    {
        Con = con;

        ColorMCCore.JavaUnzip = JavaUnzip;
    }

    partial void OnJavaTypeChanged(string value)
    {
        Switch();
        Load();
    }

    partial void OnArchChanged(string value)
    {
        if (load)
            return;

        Select();
    }

    partial void OnSystemChanged(string value)
    {
        if (load)
            return;

        if (TypeIndex == 0)
        {
            Load();
        }
        else
        {
            Select();
        }
    }

    partial void OnVersionChanged(string value)
    {
        if (load)
            return;

        if (TypeIndex == 0)
        {
            Load();
        }
        else
        {
            Select();
        }
    }

    [RelayCommand]
    public async void Load()
    {
        var window = Con.Window;
        window.ProgressInfo.Show(App.GetLanguage("AddJavaWindow.Info4"));

        load = true;

        List1.Clear();
        JavaList.Clear();

        SystemList.Clear();
        VersionList.Clear();
        ArchList.Clear();

        var res = await JavaBinding.GetJavaList(TypeIndex, SystemIndex, VersionIndex);

        if (res.Item1)
        {
            if (res.Os != null)
            {
                SystemList.AddRange(res.Os);
            }
            if (res.MainVersion != null)
            {
                VersionList.AddRange(res.MainVersion);
            }
            if (res.Arch != null)
            {
                ArchList.AddRange(res.Arch);
            }

            List1.AddRange(res.Item5!);

            Select();

            window.ProgressInfo.Close();
        }
        else
        {
            window.ProgressInfo.Close();
#if !DEBUG
            window.OkInfo.Show(App.GetLanguage("AddJavaWindow.Error1"));
#endif
        }

        load = false;
    }

    public async void Install(JavaDownloadDisplayObj obj)
    {
        var window = Con.Window;
        var res = await window.OkInfo.ShowWait(string.Format(
            App.GetLanguage("AddJavaWindow.Info1"), obj.Name));
        if (!res)
            return;

        if (ConfigBinding.GetAllConfig().Item2?.WindowMode != true)
        {
            window.ProgressInfo.Show(App.GetLanguage("AddJavaWindow.Info2"));
        }
        var res1 = await JavaBinding.DownloadJava(obj);
        window.ProgressInfo.Close();
        if (!res1.Item1)
        {
            window.OkInfo.Show(res1.Item2!);
            return;
        }

        window.NotifyInfo.Show(App.GetLanguage("AddJavaWindow.Info3"));
    }

    private void Switch()
    {
        SystemIndex = 0;
        VersionIndex = 0;
        ArchIndex = 0;
    }

    private void Select()
    {
        JavaList.Clear();

        bool arch1 = !string.IsNullOrWhiteSpace(Arch);
        bool version1 = !string.IsNullOrWhiteSpace(Version);
        bool os1 = !string.IsNullOrWhiteSpace(System);

        var list1 = from item in List1
                    where (!arch1 || (item.Arch == Arch))
                    && (!version1 || (item.MainVersion == Version))
                    && (TypeIndex == 0 || !os1 || (item.Os == System))
                    select item;

        if (list1.Count() > 100 && !(arch1 && version1 && os1))
        {
            Display = true;
        }
        else
        {
            Display = false;
            JavaList.AddRange(list1);
        }
    }

    private void JavaUnzip()
    {
        Dispatcher.UIThread.Post(() =>
        {
            var window = Con.Window;
            window.ProgressInfo.NextText(App.GetLanguage("AddJavaWindow.Info5"));
        });
    }
}
