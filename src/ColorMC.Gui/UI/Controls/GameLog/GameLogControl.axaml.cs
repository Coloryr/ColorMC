using System.ComponentModel;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.Objs;
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

        //var registryOptions = new RegistryOptions(ThemeManager.NowTheme == PlatformThemeVariant.Light ? ThemeName.LightPlus : ThemeName.DarkPlus);
        //var textMateInstallation = TextEditor1.InstallTextMate(registryOptions);
        //var lang = registryOptions.GetLanguageByExtension(".log");
        //var temp = registryOptions.GetScopeByLanguageId(lang.Id);
        //textMateInstallation.SetGrammar(temp);

        TextEditor1.TextArea.TextView.LineTransformers.Add(new LogTransformer());
    }

    private class LogTransformer : DocumentColorizingTransformer
    {
        private LogLevel FindLast(DocumentLine line, int max)
        {
            if (line == null || max == 0)
            {
                return LogLevel.None;
            }

            var level = GetLogLevel(line);
            if (level != LogLevel.Base)
            {
                return level;
            }

            return FindLast(line.PreviousLine, max - 1);
        }

        private LogLevel GetLogLevel(DocumentLine line)
        {
            string lineText = CurrentContext.Document.GetText(line);

            if (lineText.Contains("/WARN]"))
            {
                return LogLevel.Warn;
            }
            else if (lineText.Contains("/ERROR]") || lineText.Contains("[STDERR]")
                || lineText.Contains("[java.lang.Throwable$WrappedPrintStream:println:-1]"))
            {
                return LogLevel.Error;
            }
            else if (lineText.Contains("/DEBUG]"))
            {
                return LogLevel.Debug;
            }
            else if (lineText.Contains("/FATAL]"))
            {
                return LogLevel.Fatal;
            }
            else if (lineText.Contains("/INFO]"))
            {
                return LogLevel.Info;
            }

            return LogLevel.Base;
        }

        protected override void ColorizeLine(DocumentLine line)
        {
            string lineText = CurrentContext.Document.GetText(line);

            var level2 = GetLogLevel(line);

            if (level2 == LogLevel.Base)
            {
                level2 = FindLast(line, 50);
            }

            ChangeLinePart(line.Offset, line.Offset + lineText.Length,
                visualLine =>
                {
                    if (level2 == LogLevel.Fatal)
                    {
                        visualLine.BackgroundBrush = Brushes.White;
                    }
                    visualLine.TextRunProperties.SetForegroundBrush(ColorManager.GetColor(level2));
                }
            );
        }
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

    public void Log(GameLogItemObj? data)
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

    public void ReloadTitle()
    {
        Title = string.Format(App.Lang("GameLogWindow.Title"), _obj.Name);
        Window.SetTitle(Title);
    }
}
