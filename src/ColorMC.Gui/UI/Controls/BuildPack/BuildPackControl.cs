using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Controls.Download;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.BuildPack;
using System.ComponentModel;

namespace ColorMC.Gui.UI.Controls.BuildPack;

public partial class BuildPackControl : MenuControl
{
    private Tab1Control _tab1;
    private Tab2Control _tab2;

    public BuildPackControl() : base(WindowManager.GetUseName<DownloadControl>())
    {
        Title = App.Lang("BuildPackWindow.Title");
    }

    public override Bitmap GetIcon()
    {
        return ImageManager.GameIcon;
    }

    protected override void Opened()
    {
        (DataContext as BuildPackModel)?.Load();
    }

    public override void Closed()
    {
        WindowManager.BuildPackWindow = null;
    }

    protected override TopModel GenModel(BaseModel model)
    {
        return new BuildPackModel(model);
    }

    protected override Control ViewChange(int old, int index)
    {
        var model = (DataContext as BuildPackModel)!;
        switch (index)
        {
            case 0:
                _tab1 ??= new();
                return _tab1;
            case 1:
                _tab2 ??= new();
                return _tab2;
            default:
                throw new InvalidEnumArgumentException();
        }
    }
}
