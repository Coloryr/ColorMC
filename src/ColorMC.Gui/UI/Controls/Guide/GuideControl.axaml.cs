using Avalonia.Controls;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Guide;
using ColorMC.Gui.UI.Windows;
using System.ComponentModel;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.Guide;

public partial class GuideControl : UserControl, IUserControl
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);
    public string Title => App.GetLanguage("GuideWindow.Ttile");

    private readonly Tab1Control _tab1 = new();
    private readonly Tab2Control _tab2 = new();
    private readonly Tab3Control _tab3 = new();
    private readonly Tab4Control _tab4 = new();
    private readonly Tab5Control _tab5 = new();

    private bool _switch1 = false;

    private CancellationTokenSource _cancel = new();

    private int _now;

    public GuideControl()
    {
        InitializeComponent();
    }

    public void Closed()
    {
        App.GuideWindow = null;
    }

    public void Opened()
    {
        Window.SetTitle(Title);

        Content1.Content = _tab1;
        _tab1.Start();
    }

    public void SetBaseModel(BaseModel model)
    {
        var amodel = new GuideModel(model);
        amodel.PropertyChanged += Amodel_PropertyChanged;

        DataContext = amodel;
    }

    public void Load()
    {
        (DataContext as GuideModel)!.NowView = 0;
    }

    private void Amodel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var model = (DataContext as GuideModel)!;
        if (e.PropertyName == "NowView")
        {
            switch (model.NowView)
            {
                case 0:
                    Go(_tab1, model.NowView);
                    _tab1.Start();
                    break;
                case 1:
                    Go(_tab2, model.NowView);
                    _tab2.Start();
                    break;
                case 2:
                    Go(_tab3, model.NowView);
                    _tab3.Start();
                    break;
                case 3:
                    Go(_tab4, model.NowView);
                    _tab4.Start();
                    break;
                case 4:
                    Go(_tab5, model.NowView);
                    _tab5.Start();
                    break;
            }

            _now = model.NowView;
        }
    }

    private void Go(UserControl to, int now)
    {
        _cancel.Cancel();
        _cancel.Dispose();

        _cancel = new();

        if (!_switch1)
        {
            Content2.Content = to;
            _ = App.PageSlide500.Start(Content1, Content2, _now < now, _cancel.Token);
        }
        else
        {
            Content1.Content = to;
            _ = App.PageSlide500.Start(Content2, Content1, _now < now, _cancel.Token);
        }

        _switch1 = !_switch1;
    }
}
