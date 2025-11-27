namespace ColorMC.Core.GuiHandle;

public interface ILoginGui
{
    /// <summary>
    /// 选择项目
    /// </summary>
    /// <param name="items">项目列表</param>
    /// <returns>选择的项目</returns>
    public Task<int> SelectAuth(List<string> items);
}
