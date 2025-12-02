namespace ColorMC.Core.GuiHandle;

public interface IProgressGui
{
    /// <summary>
    /// 进度
    /// </summary>
    /// <param name="value"></param>
    /// <param name="all"></param>
    void SetNowProcess(int value, int all);
}
