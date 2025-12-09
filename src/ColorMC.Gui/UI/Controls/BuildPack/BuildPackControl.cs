using System.ComponentModel;
using Avalonia.Controls;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Controls.Download;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.BuildPack;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.BuildPack;

public partial class BuildPackControl : MenuControl
{
    private Tab1Control _tab1;
    private Tab2Control _tab2;
    private Tab3Control _tab3;

    public BuildPackControl() : base(WindowManager.GetUseName<DownloadControl>())
    {
        Title = LangUtils.Get("BuildPackWindow.Title");
    }

    public override void Opened()
    {
        (DataContext as BuildPackModel)?.Load();
    }

    public override void Closed()
    {
        WindowManager.BuildPackWindow = null;
    }

    protected override ControlModel GenModel(WindowModel model)
    {
        return new BuildPackModel(model);
    }

    protected override Control ViewChange(int old, int index)
    {
        switch (index)
        {
            case 0:
                _tab1 ??= new();
                return _tab1;
            case 1:
                _tab2 ??= new();
                return _tab2;
            case 2:
                _tab3 ??= new();
                return _tab3;
            default:
                throw new InvalidEnumArgumentException();
        }
    }
}
