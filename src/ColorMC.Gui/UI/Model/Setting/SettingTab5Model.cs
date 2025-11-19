using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AvaloniaEdit.Utils;
using ColorMC.Core.Helpers;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Setting;

/// <summary>
/// 设置窗口
/// </summary>
public partial class SettingModel
{
    /// <summary>
    /// Java列表
    /// </summary>
    public ObservableCollection<JavaDisplayModel> JavaList { get; init; } = [];

    /// <summary>
    /// Java名字
    /// </summary>
    [ObservableProperty]
    private string? _javaName;
    /// <summary>
    /// Java路径
    /// </summary>
    [ObservableProperty]
    private string? _javaLocal;

    /// <summary>
    /// 选中的Java
    /// </summary>
    [ObservableProperty]
    private JavaDisplayModel _javaItem;

    /// <summary>
    /// 是否在搜索Java
    /// </summary>
    [ObservableProperty]
    private bool _javaFinding;

    /// <summary>
    /// Java是否在加载
    /// </summary>
    private bool _javaLoaded;
    /// <summary>
    /// 需要的Java版本
    /// </summary>
    private int _needJava;

    /// <summary>
    /// 添加Java
    /// </summary>
    [RelayCommand]
    public void AddJava()
    {
        if (string.IsNullOrWhiteSpace(JavaName) || string.IsNullOrWhiteSpace(JavaLocal))
        {
            Model.Show(LanguageUtils.Get("SettingWindow.Tab5.Text21"));
            return;
        }

        Model.Progress(LanguageUtils.Get("SettingWindow.Tab5.Text13"));

        var res = JavaBinding.AddJava(JavaName, JavaLocal);
        Model.ProgressClose();
        if (!res.State)
        {
            Model.Show(res.Data!);
            return;
        }

        JavaName = "";
        JavaLocal = "";

        LoadJava();
    }

    /// <summary>
    /// 选中Java
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task SelectJava()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectFileAsync(top, FileType.Java);
        if (file.Path != null)
        {
            JavaLocal = file.Path;
            var info = JavaHelper.GetJavaInfo(file.Path);
            if (info != null)
            {
                JavaName = info.Type + "_" + info.Version;
            }
        }
    }

    /// <summary>
    /// 下载Java
    /// </summary>
    private void ShowAddJava()
    {
        WindowManager.ShowAddJava(_needJava);
    }

    /// <summary>
    /// 导入Java压缩包
    /// </summary>
    private async void AddJavaZip()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectFileAsync(top, FileType.JavaZip);
        if (file.Path == null || file.FileName == null)
        {
            return;
        }

        Model.Progress(LanguageUtils.Get("SettingWindow.Tab5.Text17"));
        var zip = new ZipGui(Model);
        var res = await JavaBinding.AddJavaZipAsync(file.Path, file.FileName, new ZipGui(Model));
        zip.Stop();
        Model.ProgressClose();
        if (!res.State)
        {
            Model.Show(res.Data!);
        }
        else
        {
            Model.Notify(LanguageUtils.Get("SettingWindow.Tab5.Text16"));
        }
        LoadJava();
    }

    /// <summary>
    /// 搜索Java
    /// </summary>
    public async void FindJavaDir()
    {
#if Phone
        return;
#endif
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectPathAsync(top, PathType.JavaPath);
        if (file == null)
        {
            return;
        }

        var res = await Model.ShowAsync(string.Format(LanguageUtils.Get("AddGameWindow.Tab3.Text6"), file));
        if (!res)
        {
            return;
        }

        JavaFinding = true;
        Model.SubTitle = LanguageUtils.Get("SettingWindow.Tab5.Text18");
        var list = await Task.Run(() => JavaHelper.FindJava(file));
        Model.SubTitle = null;
        JavaFinding = false;
        if (list == null)
        {
            Model.Show(LanguageUtils.Get("SettingWindow.Tab5.Text20"));
            return;
        }

        list.ForEach(item => JvmPath.AddItem(item.Type + "_" + item.Version, item.Path));
        LoadJava();
        Model.Notify(LanguageUtils.Get("SettingWindow.Tab5.Text15"));
    }

    /// <summary>
    /// 搜索Java
    /// </summary>
    public async void FindJava()
    {
        JavaFinding = true;
        Model.SubTitle = LanguageUtils.Get("SettingWindow.Tab5.Text18");
        var list = await Task.Run(JavaHelper.FindJava);
        Model.SubTitle = null;
        JavaFinding = false;
        if (list == null)
        {
            Model.Show(LanguageUtils.Get("SettingWindow.Tab5.Text20"));
            return;
        }

        list.ForEach(item => JvmPath.AddItem(item.Type + "_" + item.Version, item.Path));
        LoadJava();
        Model.Notify(LanguageUtils.Get("SettingWindow.Tab5.Text15"));
    }

    /// <summary>
    /// 加载Java列表
    /// </summary>
    public void LoadJava()
    {
        JavaList.Clear();
        JavaList.AddRange(JavaBinding.GetJavas());
    }

    /// <summary>
    /// 删除所有Java
    /// </summary>
    private async void DeleteJava()
    {
        var res = await Model.ShowAsync(LanguageUtils.Get("SettingWindow.Tab5.Text14"));
        if (!res)
            return;

        JvmPath.RemoveAll();
        LoadJava();
    }

    /// <summary>
    /// 打开Java文件夹
    /// </summary>
    private void OpenJavaPath()
    {
        PathBinding.OpenPath(PathType.JavaPath);
    }

    /// <summary>
    /// 加载Java版本
    /// </summary>
    /// <param name="mainversion"></param>
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
