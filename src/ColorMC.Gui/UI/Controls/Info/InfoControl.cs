using Avalonia.Controls;
using System.Threading;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls.Info;

public abstract class InfoControl : UserControl
{
    private bool _display = false;
    private CancellationTokenSource _cancel = new();

    public async void Display()
    {
        if (!_display)
        {
            _cancel.Cancel();
            _cancel.Dispose();
            _cancel = new();
            _display = true;
            await App.CrossFade300.Start(null, this, _cancel.Token);
        }
    }

    public Task Close()
    {
        if (_display)
        {
            _cancel.Cancel();
            _cancel.Dispose();
            _cancel = new();
            _display = false;
            return App.CrossFade300.Start(this, null, _cancel.Token);
        }

        return Task.CompletedTask;
    }
}
