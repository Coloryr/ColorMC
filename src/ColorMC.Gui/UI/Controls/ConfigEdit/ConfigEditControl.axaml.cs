using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using AvaloniaEdit.Indentation.CSharp;
using ColorMC.Core.Objs;
using ColorMC.Core.Objs.Minecraft;
using ColorMC.Gui.UI.Model.ConfigEdit;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.Gui.UI.Controls.ConfigEdit;

public partial class ConfigEditControl : UserControl, IUserControl
{
    private readonly ConfigEditModel model;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public ConfigEditControl()
    {
        InitializeComponent();

        NbtViewer.PointerPressed += NbtViewer_PointerPressed;
        NbtViewer.KeyDown += NbtViewer_KeyDown;

        TextEditor1.KeyDown += NbtViewer_KeyDown;
        TextEditor1.Options.ShowBoxForControlCharacters = true;
        TextEditor1.TextArea.IndentationStrategy =
            new CSharpIndentationStrategy(TextEditor1.Options);

        DataGrid1.CellEditEnded += DataGrid1_CellEditEnded;
        DataGrid1.CellPointerPressed += DataGrid1_CellPointerPressed;
    }

    private void DataGrid1_CellPointerPressed(object? sender, DataGridCellPointerPressedEventArgs e)
    {
        if (e.PointerPressedEventArgs.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            Dispatcher.UIThread.Post(() =>
            {
                (DataContext as ConfigEditModel)?.Flyout(this);
            });
        }
    }

    private void DataGrid1_CellEditEnded(object? sender, DataGridCellEditEndedEventArgs e)
    {
        if (e.EditAction == DataGridEditAction.Commit)
        {
            (DataContext as ConfigEditModel)?.DataEdit();
        }
    }

    private void NbtViewer_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.S && e.KeyModifiers == KeyModifiers.Control)
        {
            (DataContext as ConfigEditModel)?.Save();
        }
    }

    private void NbtViewer_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            Dispatcher.UIThread.Post(() =>
            {
                model.Pressed(this);
            });
        }
    }

    public void Opened()
    {
        if (model.World == null)
        {
            Window.SetTitle(string.Format(App.GetLanguage("ConfigEditWindow.Title"),
                model.Obj?.Name));
        }
        else
        {
            Window.SetTitle(string.Format(App.GetLanguage("ConfigEditWindow.Title1"),
                model.Obj?.Name, model.World?.LevelName));
        }
    }

    public ConfigEditControl(WorldObj world) : this()
    {
        model = new(this, world.Game, world);
        DataContext = model;
    }

    public ConfigEditControl(GameSettingObj obj) : this()
    {
        model = new(this, obj, null);
        DataContext = model;
    }

    public void Update()
    {
        model.Load();
    }

    public void Closed()
    {
        string key;
        if (model.World != null)
        {
            key = model.Obj.UUID + ":" + model.World.LevelName;
        }
        else
        {
            key = model.Obj.UUID;
        }
        App.ConfigEditWindows.Remove(key);
    }
}
