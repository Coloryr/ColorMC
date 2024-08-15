using System.ComponentModel;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameLog;

namespace ColorMC.Gui.UI.Controls.GameLog;

public partial class GameLogControl : BaseUserControl
{
    private readonly GameSettingObj _obj;

    public GameLogControl()
    {
        InitializeComponent();

        UseName = ToString() ?? "GameLogControl";
    }

    public GameLogControl(GameSettingObj obj) : this()
    {
        _obj = obj;

        Title = string.Format(App.Lang("GameLogWindow.Title"), obj.Name);
        UseName += ":" + obj.UUID;

        TextEditor1.TextArea.Background = Brushes.Transparent;

        TextEditor1.PointerWheelChanged += TextEditor1_PointerWheelChanged;
        TextEditor1.TextArea.PointerWheelChanged += TextEditor1_PointerWheelChanged;
    }

    public override void Update()
    {
        Dispatcher.UIThread.Post(() =>
        {
            (DataContext as GameLogModel)?.Load();
        });
    }

    public override void Opened()
    {
        Window.SetTitle(Title);

        (DataContext as GameLogModel)!.Load();
        (DataContext as GameLogModel)!.Load1();
    }

    public override TopModel GenModel(BaseModel model)
    {
        var amodel = new GameLogModel(model, _obj);
        amodel.PropertyChanged += Model_PropertyChanged;
        return amodel;
    }

    public override void Closed()
    {
        WindowManager.GameLogWindows.Remove(_obj.UUID);
    }

    public override Bitmap GetIcon()
    {
        var icon = ImageManager.GetGameIcon(_obj);
        return icon ?? ImageManager.GameIcon;
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

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == GameLogModel.NameEnd)
        {
            Dispatcher.UIThread.Post(() =>
            {
                TextEditor1.ScrollToLine(TextEditor1.LineCount - 2);
            });
        }
        else if (e.PropertyName == GameLogModel.NameInsert)
        {
            TextEditor1.AppendText((DataContext as GameLogModel)!.Temp);
        }
        else if (e.PropertyName == GameLogModel.NameTop)
        {
            Dispatcher.UIThread.Post(TextEditor1.ScrollToHome);
        }
        else if (e.PropertyName == GameLogModel.NameSearch)
        {
            TextEditor1.SearchPanel.Open();
        }
    }

    private void TextEditor1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        (DataContext as GameLogModel)?.SetNotAuto();
    }
}
