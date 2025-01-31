using System;
using ColorMC.Gui.Objs;

namespace ColorMC.Gui.Hook;

public interface INative
{
    /// <summary>
    /// ��ӳ�����
    /// </summary>
    /// <param name="id">���̾��</param>
    void AddHook(IntPtr id);
    /// <summary>
    /// ֹͣ���й���
    /// </summary>
    void Stop();
    /// <summary>
    /// �������λ��
    /// </summary>
    /// <param name="cursorX">Xλ��</param>
    /// <param name="cursorY">Yλ��</param>
    /// <param name="message">�Ƿ���Message����</param>
    void SendMouse(double cursorX, double cursorY, bool message);
    /// <summary>
    /// ���ͼ��̰���
    /// </summary>
    /// <param name="key">����ֵ</param>
    /// <param name="down">�Ƿ���</param>
    /// <param name="message">�Ƿ���Message����</param>
    void SendKey(InputKeyObj key, bool down, bool message);
    /// <summary>
    /// ���͹���
    /// </summary>
    /// <param name="up">�Ƿ�λ����</param>
    void SendScoll(bool up);
}