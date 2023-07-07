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
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddJavaModel : ObservableObject
{
    private readonly IUserControl Con;
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
    private int archIndex = -1;
    [ObservableProperty]
    private bool display = true;

    private bool load = true;


    public AddJavaModel(IUserControl con)
    {
        Con = con;

        ColorMCCore.JavaUnzip = JavaUnzip;
    }

    async partial void OnJavaTypeChanged(string value)
    {
        load = true;
        Switch();
        await Load();
    }

    partial void OnArchChanged(string value)
    {
        if (load)
            return;

        Select();
    }

    async partial void OnSystemChanged(string value)
    {
        if (load)
            return;

        if (TypeIndex == 0)
        {
            await Load();
        }
        else
        {
            Select();
        }
    }

    async partial void OnVersionChanged(string value)
    {
        if (load)
            return;

        if (TypeIndex == 0)
        {
            await Load();
        }
        else
        {
            Select();
        }
    }

    [RelayCommand]
    public async Task Load()
    {
        var window = Con.Window;
        window.ProgressInfo.Show(App.GetLanguage("AddJavaWindow.Info4"));

        load = true;

        List1.Clear();
        JavaList.Clear();

        var res = await JavaBinding.GetJavaList(TypeIndex, SystemList.IndexOf(System), VersionList.IndexOf(Version));

        if (res.Item1)
        {
            if (res.Os != null && SystemList.Count == 0)
            {
                SystemList.AddRange(res.Os);
            }
            if (res.MainVersion != null && VersionList.Count == 0)
            {
                VersionList.AddRange(res.MainVersion);
                Version = res.MainVersion[0];
            }
            if (res.Arch != null && ArchList.Count == 0)
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
            window.OkInfo.Show(App.GetLanguage("AddJavaWindow.Error1"));
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
        SystemList.Clear();
        VersionList.Clear();
        ArchList.Clear();
        System = "";
        Version = "";
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
