using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Core.Game;

public interface ILoginGui
{
    /// <summary>
    /// 选择项目
    /// </summary>
    /// <param name="items">项目列表</param>
    /// <returns>选择的项目</returns>
    public Task<int> SelectAuth(List<string> items);
}
