using Avalonia.Controls;
using Avalonia.Media.Imaging;
using ColorMC.Gui.Manager;
using ColorMC.Gui.UI.Controls;
using ColorMC.Gui.UI.Model;
using ColorMC.Gui.UIBinding;

namespace ColorMC.Gui.UI.Windows;

/// <summary>
/// ʵ�ʴ���
/// </summary>
public interface IBaseWindow
{
    /// <summary>
    /// ���ڻ���ģ��
    /// </summary>
    public BaseModel Model { get; }
    /// <summary>
    /// ��������
    /// </summary>
    public BaseUserControl ICon { get; }
    /// <summary>
    /// ���ô��ڱ���
    /// </summary>
    /// <param name="data"></param>
    public void SetTitle(string data);
    /// <summary>
    /// ����ͼ��
    /// </summary>
    /// <param name="icon"></param>
    public void SetIcon(Bitmap icon);
    /// <summary>
    /// �رմ���
    /// </summary>
    virtual public void Close()
    {
        if (ConfigBinding.WindowMode())
        {
            WindowManager.AllWindow?.Close(ICon);
        }
        else if (this is Window window)
        {
            window.Close();
        }
    }
    /// <summary>
    /// ��ʾ����
    /// </summary>
    virtual public void Show()
    {
        if (ConfigBinding.WindowMode())
        {
            WindowManager.AllWindow?.Add(ICon);
        }
        else if (this is Window window)
        {
            window.Show();
        }
    }
    /// <summary>
    /// ת�����
    /// </summary>
    virtual public void WindowActivate()
    {
        if (ConfigBinding.WindowMode())
        {
            WindowManager.AllWindow?.Active(ICon);
        }
        else if (this is Window window)
        {
            if (window.WindowState == WindowState.Minimized)
            {
                window.WindowState = WindowState.Normal;
            }
            window.Show();
            window.Activate();
        }
    }
    /// <summary>
    /// ����ͼ��
    /// </summary>
    void ReloadIcon();
}