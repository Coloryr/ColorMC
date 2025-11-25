namespace ColorMC.Core.GuiHandle;

public interface IZipGui
{
    /// <summary>
    /// 压缩包更新
    /// </summary>
    /// <param name="text">名字</param>
    /// <param name="size">目前进度</param>
    /// <param name="all">总进度</param>
    public void ZipUpdate(string text, int size, int all);
    /// <summary>
    /// 请求替换名字
    /// </summary>
    /// <param name="text">显示内容</param>
    /// <returns>是否同意</returns>
    public Task<bool> FileRename(string? text);
    /// <summary>
    /// 解压
    /// </summary>
    public void Unzip();
    /// <summary>
    /// 解压完毕
    /// </summary>
    public void Done();
}
