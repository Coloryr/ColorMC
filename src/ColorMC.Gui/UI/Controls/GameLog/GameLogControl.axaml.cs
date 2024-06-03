using System.ComponentModel;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core.LaunchPath;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameLog;
using ColorMC.Gui.UI.Windows;

namespace ColorMC.Gui.UI.Controls.GameLog;

public partial class GameLogControl : UserControl, IUserControl
{
    public IBaseWindow Window => App.FindRoot(VisualRoot);

    private readonly GameSettingObj _obj;

    public string Title => string.Format(App.Lang("GameLogWindow.Title"),
            _obj.Name);

    private Bitmap _icon;
    public Bitmap GetIcon() => _icon;

    public string UseName { get; }

    public GameLogControl()
    {
        InitializeComponent();

        UseName = ToString() ?? "GameLogControl";
    }

    public GameLogControl(GameSettingObj obj) : this()
    {
        _obj = obj;

        TextEditor1.TextArea.Background = Brushes.Transparent;

        TextEditor1.PointerWheelChanged += TextEditor1_PointerWheelChanged;
        TextEditor1.TextArea.PointerWheelChanged += TextEditor1_PointerWheelChanged;
        TextEditor1.TextArea.TextView.ScrollOffsetChanged += TextView_ScrollOffsetChanged;
    }

    private void TextView_ScrollOffsetChanged(object? sender, System.EventArgs e)
    {
        (DataContext as GameLogModel)?.SetNotAuto();
    }

    public void ClearLog()
    {
        Dispatcher.UIThread.Post(() =>
        {
            (DataContext as GameLogModel)!.Clear();
        });
    }

    public void Log(string? data)
    {
        if (data == null)
            return;

        Dispatcher.UIThread.Post(() =>
        {
            (DataContext as GameLogModel)!.Log(data);
        });
    }

    public void Update()
    {
        Dispatcher.UIThread.Post(() =>
        {
            (DataContext as GameLogModel)?.Load();
        });
    }

    public void Opened()
    {
        Window.SetTitle(Title);

        var icon = _obj.GetIconFile();
        if (File.Exists(icon))
        {
            _icon = new(icon);
            Window.SetIcon(_icon);
        }

        (DataContext as GameLogModel)!.Load();
        (DataContext as GameLogModel)!.Load1();
    }

    public void Closed()
    {
        _icon?.Dispose();

        App.GameLogWindows.Remove(_obj.UUID);
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
            TextEditor1.AppendText((DataContext as GameLogModel)!.Temp);
        }
        else if (e.PropertyName == "Top")
        {
            Dispatcher.UIThread.Post(TextEditor1.ScrollToHome);
        }
        else if (e.PropertyName == "Search")
        {
            TextEditor1.SearchPanel.IsVisible = true;
        }
    }

    private void TextEditor1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        (DataContext as GameLogModel)?.SetNotAuto();
    }

    public void SetBaseModel(BaseModel model)
    {
        var amodel = new GameLogModel(model, _obj);
        amodel.PropertyChanged += Model_PropertyChanged;
        DataContext = amodel;
    }
}
