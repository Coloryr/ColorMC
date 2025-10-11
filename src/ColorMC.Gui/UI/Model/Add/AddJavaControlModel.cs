using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using ColorMC.Core.Objs;
using ColorMC.Core.Utils;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Model.Setting;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.Add;

/// <summary>
/// 添加Java
/// </summary>
public partial class AddJavaControlModel : TopModel
{
    /// <summary>
    /// JAVA列表
    /// </summary>
    private readonly List<JavaDownloadModel> _javaList = [];
    /// <summary>
    /// 显示的JAVA列表
    /// </summary>
    public ObservableCollection<JavaDownloadModel> JavaList { get; init; } = [];
    /// <summary>
    /// 系统支持列表
    /// </summary>
    public ObservableCollection<string> SystemList { get; init; } = [];
    /// <summary>
    /// 主版本列表
    /// </summary>
    public ObservableCollection<string> VersionList { get; init; } = [];
    /// <summary>
    /// 进制列表
    /// </summary>
    public ObservableCollection<string> ArchList { get; init; } = [];
    /// <summary>
    /// Java类型
    /// </summary>
    public List<string> JavaTypeList { get; init; } = WebBinding.PCJavaType;

    /// <summary>
    /// 选中的Java类型
    /// </summary>
    [ObservableProperty]
    private string _javaType;
    /// <summary>
    /// 选中的操作系统
    /// </summary>
    [ObservableProperty]
    private string _system;
    /// <summary>
    /// 选中的主版本
    /// </summary>
    [ObservableProperty]
    private string _version;
    /// <summary>
    /// 选中的进制
    /// </summary>
    [ObservableProperty]
    private string _arch;
    /// <summary>
    /// Java类型
    /// </summary>
    [ObservableProperty]
    private int _typeIndex = -1;
    /// <summary>
    /// 进制类型
    /// </summary>
    [ObservableProperty]
    private int _archIndex = -1;
    /// <summary>
    /// 是否有文件可以显示
    /// </summary>
    [ObservableProperty]
    private bool _display = false;
    /// <summary>
    /// 是否正在加载
    /// </summary>
    private bool _load = true;

    private readonly string _useName;
    /// <summary>
    /// 需要下载的Java版本
    /// </summary>
    private readonly int _needJava;

    public AddJavaControlModel(BaseModel model, int version) : base(model)
    {
        _useName = ToString() ?? "AddJavaControlModel";
        _needJava = version;
        Model.SetChoiseContent(_useName, App.Lang("Button.Refash"));
        Model.SetChoiseCall(_useName, Load);
    }

    /// <summary>
    /// Java类型切换
    /// </summary>
    /// <param name="value"></param>
    partial void OnJavaTypeChanged(string value)
    {
        _load = true;
        Switch();
        Load();
    }

    /// <summary>
    /// 进制切换
    /// </summary>
    /// <param name="value"></param>
    partial void OnArchChanged(string value)
    {
        if (_load)
        {
            return;
        }

        Select();
    }

    /// <summary>
    /// 系统切换
    /// </summary>
    /// <param name="value"></param>
    partial void OnSystemChanged(string value)
    {
        if (_load)
        {
            return;
        }

        if (TypeIndex == 0)
        {
            Load();
        }
        else
        {
            Select();
        }
    }

    /// <summary>
    /// 主版本切换
    /// </summary>
    /// <param name="value"></param>
    partial void OnVersionChanged(string value)
    {
        if (_load)
        {
            return;
        }

        if (TypeIndex == 0)
        {
            Load();
        }
        else
        {
            Select();
        }
    }

    /// <summary>
    /// 加载Java列表
    /// </summary>
    public async void Load()
    {
        _load = true;

        Model.Progress(App.Lang("AddJavaWindow.Info4"));
        Model.ChoiseEnable = false;

        _javaList.Clear();
        JavaList.Clear();

        var res = await WebBinding.GetJavaListAsync(TypeIndex, SystemList.IndexOf(System), VersionList.IndexOf(Version));

        if (res.Res)
        {
            if (res.Os != null && SystemList.Count == 0)
            {
                SystemList.AddRange(res.Os);
                //根据系统自动选中
                if (SystemInfo.Os == OsType.Windows)
                {
                    var item = res.Os.FirstOrDefault(item => item.Equals(GuiNames.NameWindows, StringComparison.CurrentCultureIgnoreCase));
                    if (item != null)
                    {
                        System = item;
                    }
                }
                else if (SystemInfo.Os == OsType.Linux)
                {
                    var item = res.Os.FirstOrDefault(item => item.Equals(GuiNames.NameLinux, StringComparison.CurrentCultureIgnoreCase));
                    if (item != null)
                    {
                        System = item;
                    }
                }
                else if (SystemInfo.Os == OsType.MacOS)
                {
                    var item = res.Os.FirstOrDefault(item => item.Equals(GuiNames.NameMacos, StringComparison.CurrentCultureIgnoreCase));
                    if (item != null)
                    {
                        System = item;
                    }
                }
            }
            if (res.MainVersion != null && VersionList.Count == 0)
            {
                //根据需要自动选中
                VersionList.AddRange(res.MainVersion);

                if (_needJava != 0
                    && res.MainVersion.Contains(_needJava.ToString()))
                {
                    Version = _needJava.ToString();
                }
                else if (res.MainVersion.Count > 0)
                {
                    Version = res.MainVersion[0];
                }
            }
            if (res.Arch != null && ArchList.Count == 0)
            {
                //根据进制自动选中
                ArchList.AddRange(res.Arch);
                if (SystemInfo.IsArm)
                {
                    if (SystemInfo.Is64Bit)
                    {
                        var item = res.Arch.FirstOrDefault(item => item.Equals(GuiNames.NameAarch64, StringComparison.CurrentCultureIgnoreCase)
                        || item.Equals(GuiNames.NameArm64, StringComparison.CurrentCultureIgnoreCase));
                        if (item != null)
                        {
                            Arch = item;
                        }
                    }
                    else
                    {
                        var item = res.Arch.FirstOrDefault(item => item.Equals(GuiNames.NameArm32, StringComparison.CurrentCultureIgnoreCase)
                        || item.Equals(GuiNames.NameArm, StringComparison.CurrentCultureIgnoreCase));
                        if (item != null)
                        {
                            Arch = item;
                        }
                    }
                }
                else
                {
                    if (SystemInfo.Is64Bit)
                    {
                        var item = res.Arch.FirstOrDefault(item => item.Equals(GuiNames.NameX86_64, StringComparison.CurrentCultureIgnoreCase)
                        || item.Equals(GuiNames.NameX64, StringComparison.CurrentCultureIgnoreCase));
                        if (item != null)
                        {
                            Arch = item;
                        }
                    }
                    else
                    {
                        var item = res.Arch.FirstOrDefault(item => item.Equals(GuiNames.NameX32, StringComparison.CurrentCultureIgnoreCase)
                        || item.Equals(GuiNames.NameX86_32, StringComparison.CurrentCultureIgnoreCase));
                        if (item != null)
                        {
                            Arch = item;
                        }
                    }
                }
            }

            if (Arch != null || Version != null || System != null)
            {
                res = await WebBinding.GetJavaListAsync(TypeIndex, SystemList.IndexOf(System),
                    VersionList.IndexOf(Version!));
            }

            _javaList.AddRange(res.Download!);

            Select();

            Model.ChoiseEnable = true;
            Model.ProgressClose();
            Model.Notify(App.Lang("AddJavaWindow.Info6"));
        }
        else
        {
            Model.ChoiseEnable = true;
            Model.ProgressClose();
            Model.Show(App.Lang("AddJavaWindow.Error1"));
        }

        _load = false;
    }

    /// <summary>
    /// 安装所选Java
    /// </summary>
    /// <param name="obj"></param>
    public async void Install(JavaDownloadModel obj)
    {
        var res = await Model.ShowAsync(string.Format(
            App.Lang("AddJavaWindow.Info1"), obj.Name));
        if (!res)
        {
            return;
        }

        if (GuiConfigUtils.Config.WindowMode != true)
        {
            Model.Progress(App.Lang("AddJavaWindow.Info2"));
        }
        string temp = App.Lang("AddGameWindow.Tab1.Info21");
        //开始下载Java
        var res1 = await JavaBinding.DownloadJavaAsync(obj, (a, b, c) =>
        {
            Dispatcher.UIThread.Post(() => Model.ProgressUpdate($"{temp} {a} {b}/{c}"));
        }, () =>
        {
            Dispatcher.UIThread.Post(() => Model.ProgressUpdate(App.Lang("AddJavaWindow.Info5")));
        });
        Model.ProgressClose();
        if (!res1.State)
        {
            Model.Show(res1.Data!);
            return;
        }

        Model.Notify(App.Lang("AddJavaWindow.Info3"));
        (WindowManager.SettingWindow?.DataContext as SettingModel)?.LoadJava();
    }

    /// <summary>
    /// 切换Java类型
    /// </summary>
    private void Switch()
    {
        SystemList.Clear();
        VersionList.Clear();
        ArchList.Clear();
        System = "";
        Version = "";
        ArchIndex = 0;
    }

    /// <summary>
    /// 筛选Java
    /// </summary>
    private void Select()
    {
        JavaList.Clear();

        bool arch1 = !string.IsNullOrWhiteSpace(Arch);
        bool version1 = !string.IsNullOrWhiteSpace(Version);
        bool os1 = !string.IsNullOrWhiteSpace(System);

        var list1 = from item in _javaList
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

    public override void Close()
    {
        Model.RemoveChoiseData(_useName);
        _load = true;
        _javaList.Clear();
        JavaList.Clear();
    }
}
