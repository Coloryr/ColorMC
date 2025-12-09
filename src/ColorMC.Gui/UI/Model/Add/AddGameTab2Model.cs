using System.Threading.Tasks;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

/// <summary>
/// 添加游戏实例
/// 导入压缩包
/// </summary>
public partial class AddGameModel
{
    /// <summary>
    /// 压缩包类型列表
    /// </summary>
    public string[] PackTypeList { get; init; } = LanguageUtils.GetPackType();

    /// <summary>
    /// 压缩包位置
    /// </summary>
    [ObservableProperty]
    private string? _zipLocal;

    /// <summary>
    /// 压缩包类型
    /// </summary>
    [ObservableProperty]
    private PackType? _type = null;

    /// <summary>
    /// 压缩包路径修改
    /// </summary>
    /// <param name="value"></param>
    async partial void OnZipLocalChanged(string? value)
    {
        if (value != null && Type == null)
        {
            //测试获取压缩包类型
            var dialog = Window.ShowProgress(LanguageUtils.Get("AddGameWindow.Tab2.Text11"));
            var res = await GameBinding.CheckTypeAsync(value);
            Window.CloseDialog(dialog);
            if (res == null)
            {
                Window.Show(LanguageUtils.Get("AddGameWindow.Tab2.Text15"));
            }
            else
            {
                Type = res;
                Window.Notify(string.Format(LanguageUtils.Get("AddGameWindow.Tab2.Text10"), res.ToString()));
            }
        }
    }

    /// <summary>
    /// 添加压缩包
    /// </summary>
    [RelayCommand]
    public void AddPackGame()
    {
        if (Type == null)
        {
            Window.Show(LanguageUtils.Get("AddGameWindow.Tab2.Text14"));
            return;
        }

        AddPack(Type.Value);
    }

    /// <summary>
    /// 选择压缩包
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task SelectPack()
    {
        var top = Window.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectFileAsync(top, FileType.Modpack);
        if (file.Path != null)
        {
            ZipLocal = file.Path;
        }
    }

    /// <summary>
    /// 添加游戏实例
    /// </summary>
    /// <param name="type">压缩包类型</param>
    private async void AddPack(PackType type)
    {
        if (string.IsNullOrWhiteSpace(ZipLocal))
        {
            Window.Show(LanguageUtils.Get("AddGameWindow.Tab2.Text13"));
            return;
        }
        var dialog = Window.ShowProgress(LanguageUtils.Get("AddGameWindow.Tab2.Text9"));
        //开始导入压缩包
        var pack = new TopModPackGui(dialog);
        var res = await AddGameHelper.InstallZip(Name, Group, ZipLocal, type, new OverGameGui(Window), pack);
        pack.Stop();
        Window.CloseDialog(dialog);
        if (!res.State)
        {
            Window.Show(LanguageUtils.Get("AddGameWindow.Tab2.Text12"));
            return;
        }

        Window.Notify(LanguageUtils.Get("AddGameWindow.Tab2.Text8"));

        if (Type == PackType.ZipPack)
        {
            WindowManager.ShowGameEdit(res.Game!);
        }

        Done(res.Game!.UUID);
    }

    /// <summary>
    /// 设置文件位置
    /// </summary>
    /// <param name="file"></param>
    public void SetFile(string file)
    {
        ZipLocal = file;
    }
}
