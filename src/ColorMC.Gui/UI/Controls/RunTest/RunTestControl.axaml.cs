using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using ColorMC.Core.Objs;
using ColorMC.Gui.UI.Model.RunTest;
using ColorMC.Gui.UI.Windows;
using System.ComponentModel;
using static ColorMC.Gui.Objs.CountObj;

namespace ColorMC.Gui.UI.Controls.RunTest;

public partial class RunTestControl : UserControl, IUserControl
{
    private readonly RunTestModel model;

    public IBaseWindow Window => App.FindRoot(VisualRoot);

    public RunTestControl() : this(new GameSettingObj { Empty = true } )
    { 
        
    }

    public RunTestControl(GameSettingObj obj)
    {
        InitializeComponent();

        model = new RunTestModel(this, obj);
        DataContext = model;

        model.PropertyChanged += Tab7Control_PropertyChanged;

        TextEditor1.PointerWheelChanged += TextEditor1_PointerWheelChanged;
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

    private void Tab7Control_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Top")
        {
            Dispatcher.UIThread.Post(() =>
            {
                TextEditor1.ScrollToLine(TextEditor1.LineCount - 5);
            });
        }
        else if (e.PropertyName == "Insert")
        {
            var model = (DataContext as RunTestModel)!;
            TextEditor1.AppendText(model.Temp);
        }
    }

    public void Update()
    {
        model.Load();
    }

    public void Opened()
    {
        Window.SetTitle(string.Format(App.GetLanguage("RunTestWindow.Title"), model.Obj.Name));
    }

    private void TextEditor1_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        (DataContext as RunTestModel)!.SetNotAuto();
    }

    public void Closed()
    {
        App.RunTestWindows.Remove(model.Obj.UUID);
    }
}
