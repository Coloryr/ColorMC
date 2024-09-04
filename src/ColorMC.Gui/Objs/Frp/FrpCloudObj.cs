using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorMC.Gui.Objs.Frp;

public record FrpCloudObj
{
    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 地址
    /// </summary>
    public string IP { get; set; }
    /// <summary>
    /// 当前在线人数
    /// </summary>
    public string Now { get; set; }
    /// <summary>
    /// 最大在线人数
    /// </summary>
    public string Max { get; set; }

    public FrpShareObj? Custom { get; set; }
}
