using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UI.Windows;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ColorMC.Gui.UI.Controls;

/// <summary>
/// 基础窗口
/// </summary>
public abstract class BaseUserControl : UserControl, ITopWindow
{
    public BaseUserControl(string id)
    {
        WindowId = id;

        SizeChanged += BaseUserControl_SizeChanged;
    }

    private void BaseUserControl_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (DataContext is TopModel model)
        {
            model.WidthChange(-1, e.NewSize.Width);
        }
    }

    /// <summary>
    /// 实际窗口
    /// </summary>
    public IBaseWindow Window => WindowManager.FindRoot(VisualRoot);
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; protected set; }
    /// <summary>
    /// 窗口Id
    /// </summary>
    public string WindowId { get; private init; }
    /// <summary>
    /// 获取图标
    /// </summary>
    /// <returns></returns>
    public abstract Bitmap GetIcon();
    /// <summary>
    /// 设置模型
    /// </summary>
    /// <param name="model">基础窗口模型</param>
    public void SetBaseModel(BaseModel model)
    {
        var topmodel = GenModel(model);
        topmodel.PropertyChanged += Model_PropertyChanged;
        DataContext = topmodel;
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == TopModel.WindowCloseName)
        {
            Window?.Close();
        }
    }

    /// <summary>
    /// 生成模型
    /// </summary>
    /// <param name="model">基础窗口模型</param>
    /// <returns></returns>
    protected abstract TopModel GenModel(BaseModel model);
    /// <summary>
    /// 窗口状态修改
    /// </summary>
    /// <param name="state"></param>
    public virtual void WindowStateChange(WindowState state) { }
    /// <summary>
    /// 按钮按下
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    public virtual Task<bool> OnKeyDown(object? sender, KeyEventArgs e)
    {
        return Task.FromResult(false);
    }
    /// <summary>
    /// 鼠标按下
    /// </summary>
    /// <param name="e"></param>
    public virtual void IPointerPressed(PointerPressedEventArgs e) { }
    /// <summary>
    /// 鼠标释放
    /// </summary>
    /// <param name="e"></param>
    public virtual void IPointerReleased(PointerReleasedEventArgs e) { }
    /// <summary>
    /// 窗口打开
    /// </summary>
    public void TopOpened()
    {
        Window.SetTitle(Title);
        Opened();
    }
    /// <summary>
    /// 窗口打开
    /// </summary>
    protected virtual void Opened() { }
    /// <summary>
    /// 窗口关闭
    /// </summary>
    public virtual void Closed() { }
    /// <summary>
    /// 窗口更新
    /// </summary>
    public virtual void Update() { }
    /// <summary>
    /// 窗口要关闭时
    /// </summary>
    /// <returns>是否阻止关闭</returns>
    public virtual Task<bool> Closing() { return Task.FromResult(false); }
}
