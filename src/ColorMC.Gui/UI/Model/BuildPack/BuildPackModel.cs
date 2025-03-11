using ColorMC.Gui.UIBinding;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.BuildPack;

public partial class BuildPackModel : MenuModel
{
    private string _useName;

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
            }
        ]);

        Model.SetChoiseCall(_useName, Build);
        Model.SetChoiseContent(_useName, App.Lang("BuildPackWindow.Tabs.Text3"));
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
        Model.Progress(App.Lang("BuildPackWindow.Tabs.Text4"));
        var res = await BaseBinding.BuildPack(this);
        Model.ProgressClose();
    }
}
