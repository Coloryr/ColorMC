using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model.BuildPack;

public partial class BuildPackModel : MenuModel
{
    public BuildPackModel(BaseModel model) : base(model)
    {
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
            //new()
            //{
            //    Icon = "/Resource/Icon/GameExport/item3.svg",
            //    Text = App.Lang("GameExportWindow.Tabs.Text3")
            //},
            //new()
            //{
            //    Icon = "/Resource/Icon/GameExport/item4.svg",
            //    Text = App.Lang("GameExportWindow.Tabs.Text4")
            //},
        ]);


    }

    public override void Close()
    {
        
    }

    public void Load()
    {
        NowView = 0;
    }
}
