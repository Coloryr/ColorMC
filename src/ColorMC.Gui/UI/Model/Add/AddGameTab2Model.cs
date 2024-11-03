using System.Threading.Tasks;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ColorMC.Gui.UI.Model.Add;

public partial class AddGameModel
{
    /// <summary>
    /// 压缩包类型列表
    /// </summary>
    public string[] PackTypeList { get; init; } = LanguageBinding.GetPackType();

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
    partial void OnZipLocalChanged(string? value)
    {
        if (value != null && Type == null)
        {
            var res = GameBinding.CheckType(value);
            if (res == null)
            {
                Model.Show(App.Lang("AddGameWindow.Tab2.Error4"));
            }
            else
            {
                Type = res;
                Model.Notify(string.Format(App.Lang("AddGameWindow.Tab2.Info7"), res.ToString()));
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
            Model.Show(App.Lang("AddGameWindow.Tab2.Error3"));
            return;
        }

        AddPack((PackType)Type);
    }

    /// <summary>
    /// 选择压缩包
    /// </summary>
    /// <returns></returns>
    [RelayCommand]
    public async Task SelectPack()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }
        var file = await PathBinding.SelectFile(top, FileType.ModPack);
        if (file.Item1 != null)
        {
            ZipLocal = file.Item1;
        }
    }

    /// <summary>
    /// 添加游戏实例
    /// </summary>
    /// <param name="type">压缩包类型</param>
    private async void AddPack(PackType type)
    {
        string temp = App.Lang("AddGameWindow.Tab1.Info21");

        if (string.IsNullOrWhiteSpace(ZipLocal))
        {
            Model.Show(App.Lang("AddGameWindow.Tab2.Error2"));
            return;
        }
        Model.Progress(App.Lang("AddGameWindow.Tab2.Info6"));
        var res = await GameBinding.AddPack(ZipLocal, type, Name, Group,
        (a, b, c) =>
        {
            Dispatcher.UIThread.Post(() => Model.ProgressUpdate($"{temp} {a} {b}/{c}"));
        }, GameRequest, GameOverwirte, (size, now) =>
        {
            Model.ProgressUpdate((double)now / size);
        }, PackState);
        Model.ProgressClose();
        if (!res.State)
        {
            Model.Show(App.Lang("AddGameWindow.Tab2.Error1"));
            return;
        }

        var model = (WindowManager.MainWindow?.DataContext as MainModel)!;
        model.Model.Notify(App.Lang("AddGameWindow.Tab2.Info5"));

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
