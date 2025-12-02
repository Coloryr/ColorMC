using ColorMC.Core;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Model.BuildPack;

/// <summary>
/// 导出客户端
/// </summary>
public partial class BuildPackModel : MenuModel
{
    private readonly string _useName;

    public BuildPackModel(WindowModel model) : base(model)
    {
        _useName = ToString() ?? "BuildPackModel";

        SetMenu(
        [
            new()
            {
                Icon = "/Resource/Icon/GameExport/item1.svg",
                Text = LanguageUtils.Get("BuildPackWindow.Tabs.Text1")
            },
            new()
            {
                Icon = "/Resource/Icon/GameExport/item2.svg",
                Text = LanguageUtils.Get("BuildPackWindow.Tabs.Text2")
            },
            new()
            {
                Icon = "/Resource/Icon/GameExport/item3.svg",
                Text = LanguageUtils.Get("BuildPackWindow.Tabs.Text3")
            }
        ]);

        Window.SetChoiseCall(_useName, Build);
        Window.SetChoiseContent(_useName, LanguageUtils.Get("ServerPackWindow.Tab1.Text10"));
    }

    public override void Close()
    {
        Window.RemoveChoiseData(_useName);
    }

    /// <summary>
    /// 加载数据
    /// </summary>
    public void Load()
    {
        LoadSetting();
        LoadGames();

        NowView = 0;
    }

    /// <summary>
    /// 开始导出客户端
    /// </summary>
    private async void Build()
    {
        var top = Window.GetTopLevel();
        if (top == null)
        {
            return;
        }

        string ext = PackLaunch ? Names.NameZipExt : GuiNames.NameColorMCExt;
        var local = await PathBinding.SaveFileAsync(top, LanguageUtils.Get("BuildPackWindow.Text2"), ext, GuiNames.NameClientFile + ext);
        if (local == null)
        {
            return;
        }

        var dialog = Window.ShowProgress(LanguageUtils.Get("BuildPackWindow.Text1"));
        var res = await BaseBinding.BuildPackAsync(this, dialog, local.GetPath()!);
        Window.CloseDialog(dialog);
        if (!res)
        {
            Window.Show(LanguageUtils.Get("BuildPackWindow.Text8"));
        }
        else
        {
            Window.Notify(LanguageUtils.Get("BuildPackWindow.Text7"));
        }
    }
}
