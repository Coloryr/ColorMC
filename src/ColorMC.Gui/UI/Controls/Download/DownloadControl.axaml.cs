using System.Threading.Tasks;
using ColorMC.Core.Objs;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Model.Download;

namespace ColorMC.Gui.UI.Controls.Download;

/// <summary>
/// 下载窗口
/// </summary>
public partial class DownloadControl : BaseUserControl
{
    public DownloadControl() : base(WindowManager.GetUseName<DownloadControl>())
    {
        InitializeComponent();

        Title = App.Lang("DownloadWindow.Title");
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
        return new DownloadModel(model);
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
