using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.GameLog;
using ColorMC.Gui.UI.Windows;
using System.ComponentModel;
using System.Threading;

namespace ColorMC.Gui.UI.Controls.GameLog;

public partial class GameLogControl : UserControl, IUserControl
{
    private readonly GameLogTabModel model;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public GameLogControl() : this(new GameSettingObj { Empty = true } )
    { 
        
    }

    public GameLogControl(GameSettingObj obj)
    {
        InitializeComponent();

        model = new(this, obj);
        DataContext = model;

        model.PropertyChanged += Model_PropertyChanged;

        TextEditor1.TextArea.Background = Brushes.Transparent;

        TextEditor1.PointerWheelChanged += TextEditor1_PointerWheelChanged;
        TextEditor1.TextArea.PointerWheelChanged += TextEditor1_PointerWheelChanged;
    }
    public void ClearLog()
    {
        model.Clear();
    }

    public void Log(string? data)
    {
        if (data == null)
            return;

        model.Log(data);
    }

    public void Update()
    {
        model.Load();
    }

    public void Opened()
    {
        Window.SetTitle(string.Format(App.GetLanguage("GameLogWindow.Title"),
            model.Obj.Name));
    }

    public void Closed()
    {
        App.GameLogWindows.Remove(model.Obj.UUID);
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
            TextEditor1.AppendText(model.Temp);
        }
        else if (e.PropertyName == "Top")
        {
            Dispatcher.UIThread.Post(TextEditor1.ScrollToHome);
        }
    }

    private void TextEditor1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        model.SetNotAuto();
    }
}
