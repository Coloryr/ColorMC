using Avalonia.Controls;
using Avalonia.Interactivity;
using ColorMC.Gui.UI.Model;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls;

public partial class Info1Control : UserControl
{
    private Action? _call;
    private bool _display = false;

    public Info1Control()
    {
        InitializeComponent();

        DataContextChanged += Info1Control_DataContextChanged;
    }

    private void Info1Control_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is BaseModel model)
        {
            model.PropertyChanged += Model_PropertyChanged;
        }
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "Info1Show")
        {

        }
        else if (e.PropertyName == "Info1Close")
        {
            App.CrossFade300.Start(this, null);
        }
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        _display = false;

        
    }

    public void Close()
    {
        _display = false;

        Button_Cancel.IsEnabled = false;
        App.CrossFade300.Start(this, null);
    }

    public Task CloseAsync()
    {
        _display = false;

        Button_Cancel.IsEnabled = false;
        return App.CrossFade300.Start(this, null, CancellationToken.None);
    }

    public void Show()
    {
        _display = true;

        App.CrossFade300.Start(null, this);
    }

    public void Show(string title)
    {
        if (_display)
        {
            NextText(title);
        }
        _display = true;

        Button_Cancel.IsEnabled = true;
        TextBlock_Text.Text = title;
        _call = null;

        Button_Cancel.IsVisible = false;

        App.CrossFade300.Start(null, this);
    }

    public Task ShowAsync(string title)
    {
        if (_display)
        {
            NextText(title);
            return Task.CompletedTask;
        }
        _display = true;

        Button_Cancel.IsEnabled = true;
        TextBlock_Text.Text = title;
        _call = null;

        Button_Cancel.IsVisible = false;

        return App.CrossFade300.Start(null, this, CancellationToken.None);
    }

    public void Show(string title, Action cancel)
    {
        Button_Cancel.IsEnabled = true;
        TextBlock_Text.Text = title;
        _call = cancel;

        App.CrossFade300.Start(null, this);
    }

    public void Progress(double value)
    {
        if (value == -1)
        {
            ProgressBar_Value.IsIndeterminate = true;
            ProgressBar_Value.ShowProgressText = false;
        }
        else
        {
            ProgressBar_Value.IsIndeterminate = false;
            ProgressBar_Value.Value = value;
            ProgressBar_Value.ShowProgressText = true;
        }
    }

    public void NextText(string title)
    {
        TextBlock_Text.Text = title;
    }
}
