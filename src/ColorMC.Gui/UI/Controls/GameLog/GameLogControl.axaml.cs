using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.GameLog;
using ColorMC.Gui.UI.Windows;
using System.ComponentModel;

namespace ColorMC.Gui.UI.Controls.GameLog;

public partial class GameLogControl : UserControl, IUserControl
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public UserControl Con => this;

    public string Title => string.Format(App.GetLanguage("GameLogWindow.Title"),
            _model.Obj.Name);

    private readonly GameLogTabModel _model;

    public GameLogControl() : this(new GameSettingObj { Empty = true })
    {

    }

    public GameLogControl(GameSettingObj obj)
    {
        InitializeComponent();

        _model = new(this, obj);
        DataContext = _model;

        _model.PropertyChanged += Model_PropertyChanged;

        TextEditor1.TextArea.Background = Brushes.Transparent;

        TextEditor1.PointerWheelChanged += TextEditor1_PointerWheelChanged;
        TextEditor1.TextArea.PointerWheelChanged += TextEditor1_PointerWheelChanged;
    }
    public void ClearLog()
    {
        _model.Clear();
    }

    public void Log(string? data)
    {
        if (data == null)
            return;

        _model.Log(data);
    }

    public void Update()
    {
        _model.Load();
    }

    public void Opened()
    {
        Window.SetTitle(Title);
    }

    public void Closed()
    {
        App.GameLogWindows.Remove(_model.Obj.UUID);
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "End")
        {
            Dispatcher.UIThread.Post(() =>
            {
                TextEditor1.ScrollToLine(TextEditor1.LineCount - 2);
            });
        }
        else if (e.PropertyName == "Insert")
        {
            TextEditor1.AppendText(_model.Temp);
        }
        else if (e.PropertyName == "Top")
        {
            Dispatcher.UIThread.Post(TextEditor1.ScrollToHome);
        }
    }

    private void TextEditor1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        _model.SetNotAuto();
    }
}
