using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.LuckBlock;

namespace ColorMC.Gui.UI.Controls.LuckBlock;

public partial class LuckBlockControl : BaseUserControl
{
    private DispatcherTimer _animationTimer;

    public LuckBlockControl() : base(WindowManager.GetUseName<LuckBlockControl>())
    {
        InitializeComponent();

        Title = App.Lang("LuckBlockWindow.Title");

        SetupAnimationTimer();

        Loaded += LuckBlockControl_Loaded;
    }

    private void LuckBlockControl_Loaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is LuckBlockModel model && ItemsContainer.IsMeasureValid)
        {
            model.ContainerWidth = ItemsContainer.Bounds.Width;
        }
    }

    public override void Opened()
    {
        (DataContext as LuckBlockModel)?.LoadBlocks();
    }

    public override void Closed()
    {
        WindowManager.LuckBlockWindow = null;
    }

    protected override TopModel GenModel(BaseModel model)
    {
        return new LuckBlockModel(model);
    }

    private void SetupAnimationTimer()
    {
        _animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(16)
        };
        _animationTimer.Tick += OnAnimationTick;
    }

    private void OnAnimationTick(object? sender, EventArgs e)
    {
        (DataContext as LuckBlockModel)?.UpdateItemsPosition();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is LuckBlockModel model)
        {
            model.PropertyChanged += Model_PropertyChanged;
        }
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LuckBlockModel.IsAnimating))
        {
            var model = DataContext as LuckBlockModel;
            if (model?.IsAnimating == true)
                _animationTimer.Start();
            else
                _animationTimer.Stop();
        }
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        if (DataContext is LuckBlockModel model)
        {
            model.ContainerWidth = ItemsContainer.Bounds.Width;
        }
    }
}