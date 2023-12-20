using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

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
        if (value != null)
        {
            Type = GameBinding.CheckType(value);
        }
    }

    /// <summary>
    /// 添加压缩包
    /// </summary>
    [RelayCommand]
    public void AddPackGame()
    {
        if (BaseBinding.IsDownload)
        {
            Model.Show(App.Lang("AddGameWindow.Tab1.Error4"));
            return;
        }
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
        var res = await PathBinding.SelectFile(FileType.ModPack);
        if (!string.IsNullOrWhiteSpace(res))
        {
            ZipLocal = res;
        }
    }

    /// <summary>
    /// 解压进度
    /// </summary>
    /// <param name="size"></param>
    /// <param name="now"></param>
    private void PackUpdate(int size, int now)
    {
        Model.ProgressUpdate((double)now / size);
    }

    /// <summary>
    /// 添加进度
    /// </summary>
    /// <param name="state"></param>
    private void PackState(CoreRunState state)
    {
        if (state == CoreRunState.Read)
        {
            Model.Progress(App.Lang("AddGameWindow.Tab2.Info1"));
        }
        else if (state == CoreRunState.Init)
        {
            Model.ProgressUpdate(App.Lang("AddGameWindow.Tab2.Info2"));
        }
        else if (state == CoreRunState.GetInfo)
        {
            Model.ProgressUpdate(App.Lang("AddGameWindow.Tab2.Info3"));
        }
        else if (state == CoreRunState.Download)
        {
            Model.ProgressUpdate(App.Lang("AddGameWindow.Tab2.Info4"));
            Model.ProgressUpdate(-1);
        }
        else if (state == CoreRunState.End)
        {
            Name = "";
            Group = "";
        }
    }

    /// <summary>
    /// 添加游戏实例
    /// </summary>
    /// <param name="type">压缩包类型</param>
    private async void AddPack(PackType type)
    {
        ColorMCCore.GameOverwirte = Tab2GameOverwirte;
        ColorMCCore.GameRequest = Tab2GameRequest;
        string temp = App.Lang("Gui.Info27");

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
        });
        Model.ProgressClose();
        if (!res.Item1)
        {
            Model.Show(App.Lang("AddGameWindow.Tab2.Error1"));
            return;
        }

        var model = (App.MainWindow?.DataContext as MainModel)!;
        model.Model.Notify(App.Lang("AddGameWindow.Tab2.Info5"));
        App.MainWindow?.LoadMain();

        if (Type == PackType.ZipPack)
        {
            App.ShowGameEdit(res.Item2!);
        }

        WindowClose();
    }

    /// <summary>
    /// 设置文件位置
    /// </summary>
    /// <param name="file"></param>
    public void SetFile(string file)
    {
        ZipLocal = file;
    }

    /// <summary>
    /// 请求
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private async Task<bool> Tab2GameOverwirte(GameSettingObj obj)
    {
        Model.ProgressClose();
        var test = await Model.ShowWait(
            string.Format(App.Lang("AddGameWindow.Info2"), obj.Name));
        Model.Progress();
        return test;
    }

    /// <summary>
    /// 请求
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private async Task<bool> Tab2GameRequest(string text)
    {
        Model.ProgressClose();
        var test = await Model.ShowWait(text);
        Model.Progress();
        return test;
    }
}
