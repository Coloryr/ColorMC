using ColorMC.Core;
using ColorMC.Gui.UIBinding;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Model.BuildPack;

public partial class BuildPackModel : MenuModel
{
    private readonly string _useName;

    public BuildPackModel(BaseModel model) : base(model)
    {
        _useName = ToString() ?? "BuildPackModel";

        SetMenu(
        [
            new()
            {
                Icon = "/Resource/Icon/GameExport/item1.svg",
                Text = App.Lang("BuildPackWindow.Tabs.Text1")
            },
            new()
            {
                Icon = "/Resource/Icon/GameExport/item2.svg",
                Text = App.Lang("BuildPackWindow.Tabs.Text2")
            },
            new()
            {
                Icon = "/Resource/Icon/GameExport/item3.svg",
                Text = App.Lang("BuildPackWindow.Tabs.Text3")
            }
        ]);

        Model.SetChoiseCall(_useName, Build);
        Model.SetChoiseContent(_useName, App.Lang("BuildPackWindow.Text1"));
    }

    public override void Close()
    {
        Model.RemoveChoiseData(_useName);
    }

    public void Load()
    {
        LoadSetting();
        LoadGames();

        NowView = 0;
    }

    private async void Build()
    {
        var top = Model.GetTopLevel();
        if (top == null)
        {
            return;
        }

        string ext = PackLaunch ? Names.NameZipExt : GuiNames.NameColorMCExt;
        var local = await PathBinding.SaveFile(top, App.Lang("BuildPackWindow.Info2"), ext, GuiNames.NameClientFile + ext);
        if (local == null)
        {
            return;
        }

        Model.Progress(App.Lang("BuildPackWindow.Info1"));
        var res = await BaseBinding.BuildPack(this, local.GetPath()!);
        Model.ProgressClose();
        if (!res)
        {
            Model.Show(App.Lang("BuildPackWindow.Error1"));
        }
        else
        {
            Model.Notify(App.Lang("BuildPackWindow.Info7"));
        }
    }
}
