using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ColorMC.Core;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Custom;
using ColorMC.Gui.UI.Model.Main;
using ColorMC.Gui.UI.Windows;
using ColorMC.Gui.UIBinding;
using System.IO;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Custom;

public partial class CustomControl : UserControl, IUserControl, IMainTop
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public UserControl Con => this;

    public string Title { get; set; }

    public BaseModel Model => _model;

    private CustomControlPanelModel _model;

    public CustomControl()
    {
        InitializeComponent();
    }

    public void Closed()
    {
        ColorMCCore.GameLaunch = null;
        ColorMCCore.GameDownload = null;

        App.CustomWindow = null;

        if (App.MainWindow == null)
        {
            App.Close();
        }
    }

    public void Load(string local)
    {
        var config = ConfigBinding.GetAllConfig();
        if (config.Item2 == null)
        {
            return;
        }

        Grid1.Children.Clear();

        var obj = GameBinding.GetGame(config.Item2.ServerCustom?.GameName);
        if (obj == null)
        {
            Grid1.Children.Add(new Label()
            {
                Content = App.GetLanguage("MainWindow.Info18"),
                Foreground = Brushes.Black,
                Background = Brush.Parse("#EEEEEE"),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            return;
        }

        var ui1 = AvaloniaRuntimeXamlLoader.Parse<CustomPanelControl>(File.ReadAllText(local));

        _model = new CustomControlPanelModel(this, obj);

        ui1.DataContext = _model;

        Title = ui1.Title;
        Window.SetTitle(Title);

        Grid1.Children.Add(ui1);

        _model.App_UserEdit();
        _model.MotdLoad();

        Task.Run(() => BaseBinding.ServerPackCheck(obj));
    }

    public async Task<bool> Closing()
    {
        var windows = App.FindRoot(VisualRoot);
        if (_model.IsLaunch)
        {
            var res = await windows.OkInfo.ShowWait(App.GetLanguage("MainWindow.Info34"));
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
        _model.Launch(obj);
    }

    public void Select(GameItemModel? model)
    {

    }

    public void EditGroup(GameItemModel model)
    {

    }
}
