using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Custom;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Controls.Custom;

public partial class CustomControl : BaseUserControl, IMainTop
{
    private GameSettingObj? _obj;
    private CustomPanelControl? _ui;

    private string _uiPath;

    public CustomControl()
    {
        InitializeComponent();

        UseName = ToString() ?? "CustomControl";
    }

    public override void Closed()
    {
        WindowManager.CustomWindow = null;

        if (WindowManager.MainWindow == null)
        {
            App.Close();
        }
    }

    public void Load(string local)
    {
        _uiPath = local;

        Grid1.Children.Clear();
    }

    public void Load1()
    {
        Dispatcher.UIThread.Post(Load);
    }

    private void Load()
    {
        var config = ConfigBinding.GetAllConfig();
        _obj = GameBinding.GetGame(config.Item2.ServerCustom?.GameName);
        if (_obj == null)
        {
            Grid1.Children.Add(new TextBlock()
            {
                Text = App.Lang("MainWindow.Info18"),
                Foreground = Brushes.Black,
                Background = Brush.Parse("#EEEEEE"),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            return;
        }

        _ui = AvaloniaRuntimeXamlLoader.Parse<CustomPanelControl>(PathHelper.ReadText(_uiPath)!);

        Title = _ui.Title;
        Window.SetTitle(Title);

        Grid1.Children.Add(_ui);
        var basemodel = (DataContext as BaseModel)!;
        var amodel = new CustomControlPanelModel(this, basemodel, _obj);

        _ui.DataContext = amodel;
        amodel.App_UserEdit();
        amodel.MotdLoad();
    }

    public override async Task<bool> Closing()
    {
        if (_obj == null)
            return false;

        var model = _ui?.DataContext as CustomControlPanelModel;
        if (model == null)
        {
            return false;
        }
        if (model.IsLaunch)
        {
            var res = await model.Model.ShowWait(App.Lang("MainWindow.Info34"));
            if (res)
            {
                return false;
            }
            return true;
        }

        if (BaseBinding.IsGameRuning())
        {
            App.Hide();
            return true;
        }

        return false;
    }

    public override Bitmap GetIcon()
    {
        return ImageManager.GameIcon;
    }

    public void Launch(GameItemModel obj)
    {
        (DataContext as CustomControlPanelModel)!.Launch(obj);
    }

    public void Select(GameItemModel? model)
    {

    }

    public void EditGroup(GameItemModel model)
    {

    }

    public override void SetBaseModel(BaseModel model)
    {
        DataContext = model;
    }
}
