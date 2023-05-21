using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ColorMC.Gui.Objs;
using ColorMC.Gui.UI.Flyouts;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ColorMC.Gui.UI.Model.GameEdit;

public partial class ResourcePackModel : ObservableObject
{
    public ResourcepackDisplayObj Pack { get; }

    [ObservableProperty]
    private bool isSelect;

    private readonly IResourcePackFuntion Top;
    private readonly IUserControl Con;

    public string Local => Pack.Local;
    public string PackFormat => Pack.PackFormat.ToString();
    public string Description => Pack.Description;
    public string Broken => Pack.Pack.Broken ?
            App.GetLanguage("GameEditWindow.Tab8.Info4") : "";

    public Bitmap Pic => Pack.Icon ?? App.GameIcon;

    public ResourcePackModel(IUserControl con, IResourcePackFuntion top, ResourcepackDisplayObj pack)
    {
        Con = con;
        Top = top;
        Pack = pack;
    }

    public void Select()
    {
        Top.SetSelect(this);
    }

    public void Flyout(Control con)
    {
        _ = new GameEditFlyout3(con, this, Pack);
    }

    public async void Delete(ResourcepackDisplayObj obj)
    {
        var window = Con.Window;
        var res = await window.OkInfo.ShowWait(
            string.Format(App.GetLanguage("GameEditWindow.Tab8.Info1"), obj.Local));
        if (!res)
        {
            return;
        }

        GameBinding.DeleteResourcepack(obj.Pack);
        window.NotifyInfo.Show(App.GetLanguage("GameEditWindow.Tab4.Info3"));
        Top.Load();
    }
}