using System.Threading.Tasks;
using ColorMC.Core.GuiHandel;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Download;
using ColorMC.Gui.Utils;

namespace ColorMC.Gui.UI.Controls.Download;

/// <summary>
/// 下载窗口
/// </summary>
public partial class DownloadControl : BaseUserControl
{
    private int _thread;

    public DownloadControl() : base(WindowManager.GetUseName<DownloadControl>())
    {
        InitializeComponent();

        Title = LanguageUtils.Get("DownloadWindow.Title");
    }

    public DownloadControl(int thread) : this()
    {
        _thread = thread;
    }

    public override void Closed()
    {
        WindowManager.DownloadWindow = null;
    }

    public override async Task<bool> Closing()
    {
        return DataContext is DownloadModel model && !await model.Stop();
    }

    protected override TopModel GenModel(BaseModel model)
    {
        return new DownloadModel(_thread, model);
    }

    /// <summary>
    /// 开始下载更新
    /// </summary>
    /// <returns></returns>
    public IDownloadGuiHandel Start()
    {
        return (DataContext as DownloadModel)!.Start();
    }
}
