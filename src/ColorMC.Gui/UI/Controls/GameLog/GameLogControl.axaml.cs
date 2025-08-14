using System.ComponentModel;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.GameLog;

namespace ColorMC.Gui.UI.Controls.GameLog;

/// <summary>
/// 游戏实例日志
/// </summary>
public partial class GameLogControl : BaseUserControl
{
    /// <summary>
    /// 游戏实例
    /// </summary>
    private readonly GameSettingObj _obj;

    public GameLogControl() : base(WindowManager.GetUseName<GameLogControl>())
    {
        InitializeComponent();
    }

    public GameLogControl(GameSettingObj obj) : base(WindowManager.GetUseName<GameLogControl>(obj))
    {
        InitializeComponent();

        _obj = obj;

        Title = string.Format(App.Lang("GameLogWindow.Title"), obj.Name);

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

    /// <summary>
    /// 日志着色用
    /// </summary>
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

    /// <summary>
    /// 更新日志
    /// </summary>
    public override void Update()
    {
        Dispatcher.UIThread.Post(() =>
        {
            (DataContext as GameLogModel)?.Load();
        });
    }

    public override void Opened()
    {
        (DataContext as GameLogModel)!.Load();
        (DataContext as GameLogModel)!.LoadFileList();
    }

    protected override TopModel GenModel(BaseModel model)
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
        return ImageManager.GetGameIcon(_obj) ?? ImageManager.GameIcon;
    }

    /// <summary>
    /// 清理当前日志
    /// </summary>
    public void ClearLog()
    {
        Dispatcher.UIThread.Post(() =>
        {
            (DataContext as GameLogModel)?.Clear();
        });
    }

    /// <summary>
    /// 添加新的日志
    /// </summary>
    /// <param name="data">日志内容</param>
    public void Log(GameLogItemObj? data)
    {
        if (data == null)
            return;

        Dispatcher.UIThread.Post(() =>
        {
            (DataContext as GameLogModel)?.Log(data);
        });
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == GameLogModel.NameEnd)
        {
            TextEditor1.ScrollToLine(TextEditor1.LineCount - 2);
        }
        else if (e.PropertyName == GameLogModel.NameInsert)
        {
            TextEditor1.AppendText((DataContext as GameLogModel)!.Temp);
        }
        else if (e.PropertyName == GameLogModel.NameTop)
        {
            TextEditor1.ScrollToHome();
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

    /// <summary>
    /// 重载标题
    /// </summary>
    public void ReloadTitle()
    {
        Title = string.Format(App.Lang("GameLogWindow.Title"), _obj.Name);
    }

    /// <summary>
    /// 游戏退出触发
    /// </summary>
    /// <param name="code">错误码</param>
    public void GameExit(int code)
    {
        (DataContext as GameLogModel)?.GameExit(code);
    }
}
