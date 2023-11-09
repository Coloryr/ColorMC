using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using ColorMC.Core;
using ColorMC.Core.Helpers;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Custom;
using ColorMC.Gui.UI.Model.Items;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Custom;

public partial class CustomControl : UserControl, IUserControl, IMainTop
{
    private GameSettingObj? _obj;
    private CustomPanelControl? _ui;
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public string Title { get; set; }

    private string _uiPath;

    public string UseName { get; }

    public CustomControl()
    {
        InitializeComponent();

        UseName = ToString() ?? "CustomControl";
    }

    public void Closed()
    {
        ColorMCCore.GameLaunch = null;
        ColorMCCore.GameRequest = null;

        App.CustomWindow = null;

        if (App.MainWindow == null)
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
            Grid1.Children.Add(new Label()
            {
                Content = App.Lang("MainWindow.Info18"),
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

    public async Task<bool> Closing()
    {
        if (_obj == null)
            return false;

        var model = (_ui?.DataContext as CustomControlPanelModel);
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

    public void SetBaseModel(BaseModel model)
    {
        DataContext = model;
    }
}
