using System;
using ColorMC.Gui.Objs;

namespace ColorMC.Gui.Hook;

/// <summary>
/// ��Ϸʵ����ϵͳƽ̨��
/// </summary>
/// <param name="id">���̾��</param>
public abstract class BaseNative(IntPtr id)
{
    public IntPtr Target { get; init; } = id;

    /// <summary>
    /// ֹͣ���й���
    /// </summary>
    public abstract void Stop();
    /// <summary>
    /// �������λ��
    /// </summary>
    /// <param name="cursorX">Xλ��</param>
    /// <param name="cursorY">Yλ��</param>
    public abstract void SendMouse(double cursorX, double cursorY);
    /// <summary>
    /// ���ͼ��̰���
    /// </summary>
    /// <param name="key">����ֵ</param>
    /// <param name="down">�Ƿ���</param>
    public abstract void SendKey(InputKeyObj key, bool down);
    /// <summary>
    /// ���͹���
    /// </summary>
    /// <param name="up">�Ƿ�λ����</param>
    public abstract void SendScoll(bool up);
}