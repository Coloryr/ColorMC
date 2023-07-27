using ColorMC.Gui.UI.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Model;

public partial class BaseModel : ObservableObject
{
    protected readonly IUserControl Control;

    public IBaseWindow Window => Control.Window;

    public BaseModel(IUserControl con)
    {
        Control = con;
    }

    public void Show(string data)
    {
        Control.Window.OkInfo.Show(data);
    }

    public void ShowOk(string data, Action data1)
    {
        Control.Window.OkInfo.ShowOk(data, data1);
    }

    public void Notify(string data)
    {
        Control.Window.NotifyInfo.Show(data);
    }

    public void Progress()
    {
        Control.Window.ProgressInfo.Show();
    }

    public void Progress(string data)
    {
        Control.Window.ProgressInfo.Show(data);
    }

    public void ProgressUpdate(double data)
    {
        Control.Window.ProgressInfo.Progress(data);
    }

    public void ProgressUpdate(string data)
    {
        Control.Window.ProgressInfo.NextText(data);
    }

    public void ProgressClose()
    {
        Control.Window.ProgressInfo.Close();
    }

    public void InputClose()
    {
        Control.Window.InputInfo.Close();
    }

    public Task<bool> ShowWait(string data)
    {
        return Control.Window.OkInfo.ShowWait(data);
    }

    public Task<bool> TextInfo(string data, string data1)
    {
        return Control.Window.TextInfo.ShowWait(data, data1);
    }

    public Task<(bool Cancel, string? Text1, string? Text2)>
        ShowEdit(string data, string data1)
    {
        return Control.Window.InputInfo.ShowEdit(data, data1);
    }

    public Task<(bool Cancel, string? Text)>
        ShowOne(string data, bool data1)
    {
        return Control.Window.InputInfo.ShowOne(data, data1);
    }

    public Task<(bool Cancel, string? Text1, string? Text2)>
        ShowInput(string data, string data1, bool data2)
    {
        return Control.Window.InputInfo.ShowInput(data, data1, data2);
    }

    public void ShowInput(string title, string title1, Action cancel)
    {
        Control.Window.InputInfo.Show(title, title1, cancel);
    }

    public Task ProgressCloseAsync()
    {
        return Control.Window.ProgressInfo.CloseAsync();
    }

    public Task<(bool Cancel, int Index, string? Item)>
        ShowCombo(string data, List<string> data1)
    {
        return Control.Window.ComboInfo.Show(data, data1);
    }
}
