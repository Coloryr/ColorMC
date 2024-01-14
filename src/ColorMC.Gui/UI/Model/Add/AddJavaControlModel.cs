using Avalonia.Threading;
using AvaloniaEdit.Utils;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Model.Setting;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddJavaControlModel : TopModel
{
    /// <summary>
    /// JAVA列表
    /// </summary>
    private readonly List<JavaDownloadObj> _list1 = [];

    /// <summary>
    /// 显示的JAVA列表
    /// </summary>
    public ObservableCollection<JavaDownloadObj> JavaList { get; init; } = [];
    public ObservableCollection<string> SystemList { get; init; } = [];
    public ObservableCollection<string> VersionList { get; init; } = [];
    public ObservableCollection<string> ArchList { get; init; } = [];
    public List<string> JavaTypeList { get; init; } = WebBinding.GetJavaType();

    [ObservableProperty]
    private string _javaType;
    [ObservableProperty]
    private string _system;
    [ObservableProperty]
    private string _version;
    [ObservableProperty]
    private string _arch;
    [ObservableProperty]
    private int _typeIndex = -1;
    [ObservableProperty]
    private int _archIndex = -1;
    [ObservableProperty]
    private bool _display = false;

    private bool _load = true;

    private readonly string _useName;

    public AddJavaControlModel(BaseModel model) : base(model)
    {
        _useName = ToString() ?? "AddJavaControlModel";

        Model.SetChoiseContent(_useName, App.Lang("Button.Refash"));
        Model.SetChoiseCall(_useName, Load);
    }

    partial void OnJavaTypeChanged(string value)
    {
        _load = true;
        Switch();
        Load();
    }

    partial void OnArchChanged(string value)
    {
        if (_load)
            return;

        Select();
    }

    partial void OnSystemChanged(string value)
    {
        if (_load)
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
        if (_load)
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

    public async void Load()
    {
        Model.Progress(App.Lang("AddJavaWindow.Info4"));

        _load = true;

        _list1.Clear();
        JavaList.Clear();

        var res = await WebBinding.GetJavaList(TypeIndex, SystemList.IndexOf(System), VersionList.IndexOf(Version));

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

            _list1.AddRange(res.Item5!);

            Select();

            Model.ProgressClose();
        }
        else
        {
            Model.ProgressClose();
            Model.Show(App.Lang("AddJavaWindow.Error1"));
        }

        _load = false;
    }

    public async void Install(JavaDownloadObj obj)
    {
        var res = await Model.ShowWait(string.Format(
            App.Lang("AddJavaWindow.Info1"), obj.Name));
        if (!res)
        {
            return;
        }

        if (ConfigBinding.GetAllConfig().Item2?.WindowMode != true)
        {
            Model.Progress(App.Lang("AddJavaWindow.Info2"));
        }
        string temp = App.Lang("Gui.Info27");
        var res1 = await JavaBinding.DownloadJava(obj, (a, b, c) =>
        {
            Dispatcher.UIThread.Post(() => Model.ProgressUpdate($"{temp} {a} {b}/{c}"));
        }, () =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                Model.ProgressUpdate(App.Lang("AddJavaWindow.Info5"));
            });
        }, App.DownloaderUpdate);
        Model.ProgressClose();
        if (!res1.Item1)
        {
            Model.Show(res1.Item2!);
            return;
        }

        Model.Notify(App.Lang("AddJavaWindow.Info3"));
        (App.SettingWindow?.DataContext as SettingModel)?.LoadJava();
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

        var list1 = from item in _list1
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

    protected override void Close()
    {
        _load = true;
        _list1.Clear();
        JavaList.Clear();
    }
}
