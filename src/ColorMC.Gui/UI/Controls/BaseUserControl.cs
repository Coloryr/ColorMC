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
/// ��������
/// </summary>
public abstract class BaseUserControl : UserControl, ITopWindow
{
    /// <summary>
    /// ʵ�ʴ���
    /// </summary>
    public IBaseWindow? Window => WindowManager.FindRoot(VisualRoot);
    /// <summary>
    /// ����
    /// </summary>
    public string Title
    {
        get
        {
            return _title;
        }
        protected set
        {
            _title = value;
            Window?.SetTitle(value);
        }
    }

    private string _title;

    /// <summary>
    /// ����Id
    /// </summary>
    public string WindowId { get; private init; }

    public BaseUserControl(string id)
    {
        WindowId = id;

        SizeChanged += BaseUserControl_SizeChanged;
    }

    /// <summary>
    /// ����ģ��
    /// </summary>
    /// <param name="model">��������ģ��</param>
    public void SetBaseModel(BaseModel model)
    {
        var topmodel = GenModel(model);
        topmodel.PropertyChanged += Model_PropertyChanged;
        DataContext = topmodel;
    }

    /// <summary>
    /// ����ģ��
    /// </summary>
    /// <param name="model">��������ģ��</param>
    /// <returns></returns>
    protected abstract TopModel GenModel(BaseModel model);
    /// <summary>
    /// ����״̬�޸�
    /// </summary>
    /// <param name="state"></param>
    public virtual void WindowStateChange(WindowState state) { }
    /// <summary>
    /// ��ť����
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    public virtual Task<bool> OnKeyDown(object? sender, KeyEventArgs e)
    {
        return Task.FromResult(false);
    }
    /// <summary>
    /// ��갴��
    /// </summary>
    /// <param name="e"></param>
    public virtual void IPointerPressed(PointerPressedEventArgs e) { }
    /// <summary>
    /// ����ͷ�
    /// </summary>
    /// <param name="e"></param>
    public virtual void IPointerReleased(PointerReleasedEventArgs e) { }
    /// <summary>
    /// ���ڴ�
    /// </summary>
    public void WindowOpened()
    {
        Window?.SetTitle(_title);
        Window?.ReloadIcon();
        Opened();
    }

    /// <summary>
    /// ���ڴ�
    /// </summary>
    public virtual void Opened() { }
    /// <summary>
    /// ���ڹر�
    /// </summary>
    public virtual void Closed() { }
    /// <summary>
    /// ���ڸ���
    /// </summary>
    public virtual void Update() { }
    /// <summary>
    /// ����Ҫ�ر�ʱ
    /// </summary>
    /// <returns>�Ƿ���ֹ�ر�</returns>
    public virtual Task<bool> Closing() { return Task.FromResult(false); }

    /// <summary>
    /// ����ͼ��
    /// </summary>
    public virtual void ReloadIcon() 
    {
        Window?.ReloadIcon();
    }
    /// <summary>
    /// ��ȡͼ��
    /// </summary>
    /// <returns></returns>
    public virtual Bitmap GetIcon()
    {
        return ImageManager.Icon;
    }

    private void Model_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == TopModel.WindowCloseName)
        {
            Window?.Close();
        }
    }

    private void BaseUserControl_SizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (DataContext is TopModel model)
        {
            model.WidthChange(-1, e.NewSize.Width);
        }
    }
}
